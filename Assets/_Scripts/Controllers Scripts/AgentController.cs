using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class AgentController : ControllersParent
{
    #region PRIVATE FIELDS

    [Header("Ball Physical Behavior Parameters")]
    [SerializeField] private List<NamedActions> _possibleActions;
    [SerializeField] private List<NamedPhysicMaterials> _possiblePhysicMaterials;

    [Header("Components")]
    [SerializeField] private Rigidbody _rigidBody;
    [SerializeField] private BallDetection _ballDetectionArea;

    [Header("Movements and Hit Parameters")]
    [SerializeField] private float _movementSpeed;
    [SerializeField] private float _minimumHitKeyPressTimeToIncrementForce;
    [SerializeField] private float _maximumHitKeyPressTime;

    private Vector2 _movementVector;
    private int _actionIndex;
    private float _currentSpeed;
    private ControllersParent _otherPlayer;
    private FieldBorderPointsContainer _borderPointsContainer;
    private float _shootingDirectionLateralComponent;
    private float _shootingDirectionForwardComponent;

    private int _wrongServicesCount;
    private bool _ballHasBeenHitBackByAgent;
    private float _serviceDurationCounter;
    private bool _otherPlayerHitBall;
    private Vector3 _ballFirstReboundPosition;
    private Vector3 _distanceToBallTrajectoryVector;
    private float _xMinimumDistanceToReboundPosition;
    private float _zMinimumDistanceToReboundPosition;

    #endregion

    #region SETTERS

    public int ActionIndex { set { _actionIndex = value; } }
    public bool OtherPlayerHitBall { set { _otherPlayerHitBall = value; } }

    #endregion

    #region UNITY METHODS

    private void Start()
    {
        _otherPlayer = _trainingManager.Controllers[0] == this ? _trainingManager.Controllers[1] : _trainingManager.Controllers[0];
        UpdateBorderPointsContainer();

        _xMinimumDistanceToReboundPosition = _ballDetectionArea.BoxCollider.size.x * _ballDetectionArea.transform.localScale.x * transform.localScale.x / 2f;
        _zMinimumDistanceToReboundPosition = _ballDetectionArea.BoxCollider.size.z * _ballDetectionArea.transform.localScale.z * transform.localScale.z / 2f;

        ServicesCount = 0;
        _hitKeyPressedTime = 0f;
        _isCharging = false;
        _currentSpeed = _movementSpeed;
        _actionIndex = 0;

        _wrongServicesCount = 0;
        _ballHasBeenHitBackByAgent = false;
        _serviceDurationCounter = 0f;
    }

    void Update()
    {
        // The player is pressing the hit key.
        if (_isCharging)
        {
            if (_hitKeyPressedTime < _maximumHitKeyPressTime)
            {
                _hitKeyPressedTime += Time.deltaTime;
            }
        }

        if (PlayerState == PlayerStates.SERVE && IsServing && _trainingManager.GameState == GameState.SERVICE && _ballDetectionArea.IsBallInHitZone)
        {
            _serviceDurationCounter += Time.deltaTime;

            // If the agent has to serve but 2 seconds passed without a service, he gets a negative reward.
            if (_serviceDurationCounter >= 2f)
            {
                AgentDoesntServe();
            }
        }

        // If the other player hit the ball, the agent must be close to the first rebound position.
        if (_otherPlayerHitBall)
        {
            _distanceToBallTrajectoryVector = Vector3.ProjectOnPlane(_ballFirstReboundPosition - gameObject.transform.position, Vector3.up);

            if (_distanceToBallTrajectoryVector.x <= _xMinimumDistanceToReboundPosition && _distanceToBallTrajectoryVector.z <= _zMinimumDistanceToReboundPosition)
            {
                AgentCloseToBallReboundPoint();
            }
            else
            {
                AgentAwayFromBallReboundPoint(_distanceToBallTrajectoryVector.magnitude);
            }
        }
        // Otherwise, if the agent is the last controller to hit the ball, he has to get back to the center of the field.
        else if (PlayerState != PlayerStates.SERVE)
        {
            float zMinimumDistance = Mathf.Abs(_borderPointsContainer.FrontPointTransform.position.z - _borderPointsContainer.BackPointTransform.position.z) / 2f - 1f;
            float xMinimumDistance = Mathf.Abs(_borderPointsContainer.RightPointTransform.position.x - _borderPointsContainer.LeftPointTransform.position.x) / 2f - 1f;

            if ((Vector3.Distance(transform.position, _borderPointsContainer.FrontPointTransform.position) < zMinimumDistance) ||
                (Vector3.Distance(transform.position, _borderPointsContainer.BackPointTransform.position) < zMinimumDistance) ||
                (Vector3.Distance(transform.position, _borderPointsContainer.RightPointTransform.position) < xMinimumDistance) ||
                (Vector3.Distance(transform.position, _borderPointsContainer.LeftPointTransform.position) < xMinimumDistance)) 
            {
                AgentNotInMiddleSquareAfterHittingBall();
            }
            else
            {
                AgentInMiddleSquareAfterHittingBall();
            }
        }

        // The agent earns a reward each frame from the moment he hit the ball back and while the point is faught by the agent and the bot.
        if (_ballHasBeenHitBackByAgent)
        {
            PointFaughtAfterAgentHitBackTheBall();
        }
    }

    #endregion

    public void CalculateBallFirstReboundHorizontalPosition()
    {
        AIBall ball = _trainingManager.BallInstance.GetComponent<AIBall>();
        _ballFirstReboundPosition = ball.gameObject.transform.position + Vector3.ProjectOnPlane(ball.gameObject.GetComponent<Rigidbody>().velocity.normalized, Vector3.up).normalized *
            ball.ActualHorizontalForce * ball.ShotParameters.ForceToDistanceFactor;
    }

    private void UpdateBorderPointsContainer()
    {
        foreach (FieldBorderPointsContainer borderPointContainer in _trainingManager.BorderPointsContainers)
        {
            if (borderPointContainer.Team == _playerTeam)
            {
                _borderPointsContainer = borderPointContainer;
                return;
            }
        }
    }

    #region ACTION METHODS

    private void Shoot(HitType hitType)
    {
        // If there is no ball in the hit volume or if the ball rigidbody is kinematic or if the player already applied force to the ball or if the game phase is in end of point,
        // then the player can't shoot in the ball.
        if (!_ballDetectionArea.IsBallInHitZone || _ballDetectionArea.Ball.gameObject.GetComponent<Rigidbody>().isKinematic
            || _ballDetectionArea.Ball.LastPlayerToApplyForce == this || _trainingManager.GameState == GameState.ENDPOINT)
        {
            TryToHitBall();
            _hitKeyPressedTime = 0f;
            _isCharging = false;
            return;
        }

        // The agent gains rewards when it hits the ball.
        HasHitBall();
        _otherPlayerHitBall = false;

        // The force to apply to the ball is calculated considering how long the player pressed the key and where is the player compared to the net position.
        float hitKeyPressTime = hitType == HitType.Lob ? _minimumHitKeyPressTimeToIncrementForce : Mathf.Clamp(_hitKeyPressedTime, _minimumHitKeyPressTimeToIncrementForce, _maximumHitKeyPressTime);
        float wantedHitForce = _minimumShotForce + ((hitKeyPressTime - _minimumHitKeyPressTimeToIncrementForce) / (_maximumHitKeyPressTime - _minimumHitKeyPressTimeToIncrementForce)) * (_maximumShotForce - _minimumShotForce);
        float hitForce = CalculateActualForce(wantedHitForce);

        // Hit charging variables are reset.
        _hitKeyPressedTime = 0f;
        _isCharging = false;

        // The player enters in the PLAY state.
        if (PlayerState != PlayerStates.PLAY)
        {
            // if the player was serving, the service detection volume of each player and the service lock colliders are disabled.
            if (PlayerState == PlayerStates.SERVE)
            {
                _trainingManager.DesactivateAllServiceDetectionVolumes();
                _trainingManager.DisableLockServiceColliders();
                _serviceDurationCounter = 0f;
            }
            else
            {
                _ballHasBeenHitBackByAgent = true;
            }

            PlayerState = PlayerStates.PLAY;
        }

        // The game enters in playing phase when the ball is hit by the other player after the service.
        if (_ballDetectionArea.Ball.LastPlayerToApplyForce != null && _trainingManager.GameState == GameState.SERVICE)
            _trainingManager.GameState = GameState.PLAYING;

        Vector3 horizontalDirection;

        horizontalDirection = _shootingDirectionLateralComponent * Vector3.right + _shootingDirectionForwardComponent * Vector3.forward;

        // Initialization of the correct ball physic material.
        _ballDetectionArea.Ball.InitializePhysicsMaterial(hitType == HitType.Drop ? NamedPhysicMaterials.GetPhysicMaterialByName(_possiblePhysicMaterials, "Drop") :
            NamedPhysicMaterials.GetPhysicMaterialByName(_possiblePhysicMaterials, "Normal"));

        // Initialization of the other ball physic parameters.
        _ballDetectionArea.Ball.InitializeActionParameters(NamedActions.GetActionParametersByName(_possibleActions, hitType.ToString()));

        // Applying a specific force in a specific direction and with a specific rising force factor.
        // If the player is doing a lob, there is no need to multiply the rising force of the ball by a factor.
        _ballDetectionArea.Ball.ApplyForce(hitForce, hitType == HitType.Lob ? 1f : _ballDetectionArea.GetRisingForceFactor(hitType), horizontalDirection.normalized, this);
    }

    public void Move(InputAction.CallbackContext context)
    {
        _movementVector = context.ReadValue<Vector2>();
    }

    public void ChargeShot(InputAction.CallbackContext context)
    {
        if (context.performed && PlayerState != PlayerStates.SERVE)
        {
            _isCharging = true;
        }
    }

    public void Flat(InputAction.CallbackContext context)
    {
        if (context.performed && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftShift))
        {
            _actionIndex = 3;
        }
    }

    private void Flat()
    {
        Shoot(HitType.Flat);
    }

    public void TopSpin(InputAction.CallbackContext context)
    {
        if (context.performed && !Input.GetKey(KeyCode.LeftControl))
        {
            _actionIndex = 4;
        }
    }

    private void TopSpin()
    {
        Shoot(HitType.TopSpin);
    }

    public void Drop(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _actionIndex = 6;
        }
    }

    private void Drop()
    {
        if(PlayerState != PlayerStates.SERVE)
        {
            Shoot(HitType.Drop);
        }
    }

    public void Slice(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _actionIndex = 5;
        }
    }

    private void Slice()
    {
        if (PlayerState != PlayerStates.SERVE)
        {
            Shoot(HitType.Slice);
        }
    }

    public void Lob(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _actionIndex = 7;
        }
    }

    private void Lob()
    {
        if(PlayerState != PlayerStates.SERVE)
        {
            Shoot(HitType.Lob);
        }
    }

    /*    public void SlowTime(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _actionIndex = 2;
            }
            else if (context.canceled)
            {
                Time.timeScale = 1f;
                _currentSpeed = _movementSpeed;
                _actionIndex = 0;
            }
        }

        public void SlowTime()
        {
            if (PlayerState != PlayerStates.SERVE && GameManager.Instance.BallInstance.GetComponent<Ball>().LastPlayerToApplyForce != this)
            {
                Time.timeScale = _actionParameters.SlowTimeScaleFactor;
                _currentSpeed = _movementSpeed / Time.timeScale;
            }
        }*/

    public void TechnicalShot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _actionIndex = 2;
        }
    }

    public void TechnicalShot(int movementDirectionIndex)
    {
        if(PlayerState != PlayerStates.SERVE && _trainingManager.BallInstance.GetComponent<AIBall>().LastPlayerToApplyForce != this)
        {
            float forwardMovementFactor = 0f;
            float rightMovementFactor = 0f;

            if (movementDirectionIndex == 0)
            {
                forwardMovementFactor = 1f;
            }
            else if (movementDirectionIndex == 1)
            {
                rightMovementFactor = 1f;
            }
            else if (movementDirectionIndex == 2)
            {
                forwardMovementFactor = -1f;
            }
            else
            {
                rightMovementFactor = -1f;
            }

            Vector3 forwardVector = Vector3.Project(_trainingManager.CameraTransform.forward, Vector3.forward).normalized;
            Vector3 rightVector = Vector3.Project(_trainingManager.CameraTransform.right, Vector3.right).normalized;
            Vector3 wantedDirection = forwardMovementFactor * forwardVector + rightMovementFactor * rightVector;

            float distanceToBorderInWantedDirection = _trainingManager.GetDistanceToBorderByDirection(this, wantedDirection, forwardVector, rightVector);

            if (distanceToBorderInWantedDirection > _actionParameters.TechnicalShotMovementLength)
            {
                transform.position += wantedDirection * _actionParameters.TechnicalShotMovementLength;
            }
            else
            {
                transform.position += wantedDirection * distanceToBorderInWantedDirection;
            }
        }
    }

    #endregion

    #region ENUMERATION TO INDEX CONVERSION METHODS

    private int GetIndexOfEnumerationValue(object enumerationValue)
    {
        Type enumerationType = enumerationValue.GetType();
        Array enumerationValues = Enum.GetValues(enumerationType);

        for (int i = 0; i < enumerationValues.Length; i++) 
        {
            if (enumerationValues.GetValue(i) == enumerationValue) 
            {
                return i;
            }
        }

        return -1;
    }

    #endregion

    #region AGENT TRAINING METHODS

    public override void OnEpisodeBegin()
    {
        _hitKeyPressedTime = 0f;
        _isCharging = false;
        _currentSpeed = _movementSpeed;
        _actionIndex = 0;
        _ballHasBeenHitBackByAgent = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // The ball presence in the hit zone is observed.
        sensor.AddObservation(_ballDetectionArea.IsBallInHitZone);
        // The ball kinematic state is observed.
        sensor.AddObservation(_trainingManager.BallInstance.GetComponent<Rigidbody>().isKinematic);
        // The boolean describing if the agent already hit the ball is observed.
        sensor.AddObservation(_trainingManager.BallInstance.GetComponent<AIBall>().LastPlayerToApplyForce == this);
        // The player state is observed.
        sensor.AddObservation(GetIndexOfEnumerationValue(PlayerState));
        // The game state is observed.
        sensor.AddObservation(GetIndexOfEnumerationValue(_trainingManager.GameState));
        // The other player's position is observed.
        sensor.AddObservation(_otherPlayer.transform.position);
        // The ball position is observed.
        sensor.AddObservation(_trainingManager.BallInstance.transform.position);
        // The field limits are observed.
        sensor.AddObservation(_borderPointsContainer.FrontPointTransform.position);
        sensor.AddObservation(_borderPointsContainer.BackPointTransform.position);
        sensor.AddObservation(_borderPointsContainer.RightPointTransform.position);
        sensor.AddObservation(_borderPointsContainer.LeftPointTransform.position);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        continuousActions[0] = _movementVector.x;
        continuousActions[1] = _movementVector.y;

        // The hit direction is set according to the mouse position on the screen.
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, float.MaxValue, ~LayerMask.GetMask("Player")))
        {
            continuousActions[2] = Vector3.Project(hit.point - transform.position, Vector3.right).x;
            continuousActions[3] = Vector3.Project(hit.point - transform.position, Vector3.forward).z;
        }
        else
        {
            continuousActions[2] = 0f;
            continuousActions[3] = 1f;
        }

        continuousActions[4] = _hitKeyPressedTime;

        discreteActions[0] = _actionIndex;

        float tempForwardMovementFactor = 0f;
        float tempRightMovementFactor = 0f;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
        {
            tempForwardMovementFactor = MathF.Sign(Input.GetAxis("Vertical"));
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            tempRightMovementFactor = MathF.Sign(Input.GetAxis("Horizontal"));
        }

        if (Mathf.Abs(tempForwardMovementFactor) == Mathf.Abs(tempRightMovementFactor))
        {
            discreteActions[1] = (int)tempRightMovementFactor > 0 ? 1 : 3;
        }
        else
        {
            if(Mathf.Abs(tempForwardMovementFactor) > Mathf.Abs(tempRightMovementFactor))
            {
                discreteActions[1] = (int)tempForwardMovementFactor > 0 ? 0 : 2;
            }
            else
            {
                discreteActions[1] = (int)tempRightMovementFactor > 0 ? 1 : 3;
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // If the game is in the end of point or the the end of match phase, the player can't move.
        // If the player is serving and threw the ball in the air, he can't move either.
        // Otherwise he can move with at least one liberty axis.
        if (_trainingManager.GameState != GameState.ENDPOINT && _trainingManager.GameState != GameState.ENDMATCH
            && !(PlayerState == PlayerStates.SERVE && !_trainingManager.BallInstance.GetComponent<Rigidbody>().isKinematic))
        {
            // The global player directions depend on the side he is on and its forward movement depends on the game phase.
            Vector3 rightVector = _trainingManager.CameraTransform.right;

            Vector3 forwardVector = Vector3.zero;
            if (_trainingManager.GameState != GameState.SERVICE || !IsServing || PlayerState == PlayerStates.PLAY)
            {
                forwardVector = Vector3.Project(_trainingManager.CameraTransform.forward, Vector3.forward);
            }

            Vector3 movementDirection = rightVector.normalized * actions.ContinuousActions[0] + forwardVector.normalized * actions.ContinuousActions[1];

            // The player moves according to the movement inputs.
            _rigidBody.velocity = movementDirection.normalized * _currentSpeed + new Vector3(0, _rigidBody.velocity.y, 0);
        }
        else
        {
            _rigidBody.velocity = new Vector3(0, _rigidBody.velocity.y, 0);
        }

        _shootingDirectionLateralComponent = actions.ContinuousActions[2];
        _shootingDirectionForwardComponent = actions.ContinuousActions[3];

        if (GetComponent<BehaviorParameters>().BehaviorType != BehaviorType.HeuristicOnly)
        {
            _hitKeyPressedTime = actions.ContinuousActions[4];
        }

        if ((GetComponent<BehaviorParameters>().BehaviorType == BehaviorType.HeuristicOnly && actions.DiscreteActions[0] == _actionIndex) ||
            GetComponent<BehaviorParameters>().BehaviorType != BehaviorType.HeuristicOnly) 
        {
            switch (actions.DiscreteActions[0])
            {
                case 0:
                    // Doing nothing.
                    break;
                case 1:
                    // Throw the ball in the air during the service.
                    if (PlayerState == PlayerStates.SERVE && IsServing)
                    {
                        ThrowBall();
                    }
                    // The action index is set to 0 after each action.
                    _actionIndex = 0;
                    break;
                /*            case 2:
                                // Slowing time.
                                SlowTime();
                                // The action index is set to 0 after each action.
                                _actionIndex = 0;
                                break;*/
                case 2:
                    // Realising the technical shot.
                    TechnicalShot(actions.DiscreteActions[1]);
                    // The action index is set to 0 after each action.
                    _actionIndex = 0;
                    break;
                case 3:
                    // Flat shot.
                    Flat();
                    // The action index is set to 0 after each action.
                    _actionIndex = 0;
                    break;
                case 4:
                    // Top spin shot.
                    TopSpin();
                    // The action index is set to 0 after each action.
                    _actionIndex = 0;
                    break;
                case 5:
                    // Slice shot.
                    Slice();
                    // The action index is set to 0 after each action.
                    _actionIndex = 0;
                    break;
                case 6:
                    // Drop shot.
                    Drop();
                    // The action index is set to 0 after each action.
                    _actionIndex = 0;
                    break;
                case 7:
                    // Lob shot.
                    Lob();
                    // The action index is set to 0 after each action.
                    _actionIndex = 0;
                    break;
                default:
                    break;
            }
        }
    }

    #endregion

    #region POSITIVE & NEGATIVE REWARDS SYSTEM

    #region NEGATIVE REWARDS

    public void WrongFirstService()
    {
        WrongService();
    }

    public void MadeFault()
    {
        if (_trainingManager.GameState == GameState.SERVICE)
        {
            WrongService();
        }
        else
        {
            AddReward(-6f);
        }

        EndEpisode();
    }

    public void TouchedForbiddenCollider()
    {
        AddReward(-6f);
        _trainingManager.PlacingPlayers();
        _trainingManager.BallInstance.GetComponent<AIBall>().ResetBall();
        EndEpisode();
    }

    public void WrongService()
    {
        _wrongServicesCount++;
        AddReward(-2f * _wrongServicesCount);
    }

    public void TryToHitBall()
    {
        AddReward(-0.5f);
    }

    private void AgentDoesntServe()
    {
        AddReward(-0.005f);
    }

    public void AgentDoesntHitTheBall()
    {
        AddReward(-0.005f);
    }

    private void AgentAwayFromBallReboundPoint(float distance)
    {
        float clampedDistance = Mathf.Clamp(distance, 0f, 14f);
        AddReward(-0.007f * (clampedDistance / 14f));
    }

    private void AgentNotInMiddleSquareAfterHittingBall()
    {
        AddReward(-0.0025f);
    }

    #endregion

    #region POSITIVE REWARDS

    public void HasHitBall()
    {
        AddReward(5f);
    }

    public void BallTouchedFieldWithoutProvokingFault()
    {
        AddReward(8f);
    }

/*    public void ProperService()
    {
        _wrongServicesCount = 0;
        AddReward(3f);
    }*/

    private void PointFaughtAfterAgentHitBackTheBall()
    {
        AddReward(0.015f);
    }

    private void AgentCloseToBallReboundPoint()
    {
        AddReward(0.007f);
    }

    private void AgentInMiddleSquareAfterHittingBall()
    {
        AddReward(0.005f);
    }

    public void ScoredPoint()
    {
        AddReward(4f);  
        EndEpisode();
    }

    #endregion

    #endregion
}
