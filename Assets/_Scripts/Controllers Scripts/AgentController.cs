using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] protected float _minimumShotForce;
    [SerializeField] protected float _maximumShotForce;
    [SerializeField] private float _minimumHitKeyPressTimeToIncrementForce;
    [SerializeField] private float _maximumHitKeyPressTime;

    private Vector2 _movementVector;
    private int _actionIndex;
    private float _currentSpeed;

    #endregion

    public int ActionIndex { set { _actionIndex = value; } }

    #region UNITY METHODS

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
    }

    #endregion

    #region ACTION METHODS

    private void Shoot(HitType hitType)
    {
        // If there is no ball in the hit volume or if the ball rigidbody is kinematic or if the player already applied force to the ball or if the game phase is in end of point,
        // then the player can't shoot in the ball.
        if (!_ballDetectionArea.IsBallInHitZone || _ballDetectionArea.Ball.gameObject.GetComponent<Rigidbody>().isKinematic
            || _ballDetectionArea.Ball.LastPlayerToApplyForce == this || GameManager.Instance.GameState == GameState.ENDPOINT)
        {
            _hitKeyPressedTime = 0f;
            _isCharging = false;
            return;
        }

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
                GameManager.Instance.DesactivateAllServiceDetectionVolumes();
                GameManager.Instance.ServiceManager.DisableLockServiceColliders();
            }

            PlayerState = PlayerStates.PLAY;
        }

        // The game enters in playing phase when the ball is hit by the other player after the service.
        if (_ballDetectionArea.Ball.LastPlayerToApplyForce != null && GameManager.Instance.GameState == GameState.SERVICE)
            GameManager.Instance.GameState = GameState.PLAYING;

        Vector3 horizontalDirection;

        // The hit direction is set according to the mouse position on the screen.
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, float.MaxValue, ~LayerMask.GetMask("Player")))
        {
            horizontalDirection = Vector3.Project(hit.point - transform.position, Vector3.forward) + Vector3.Project(hit.point - transform.position, Vector3.right);
        }
        else
        {
            horizontalDirection = Vector3.forward;
        }

        // Initialization of the correct ball physic material.
        _ballDetectionArea.Ball.InitializePhysicsMaterial(hitType == HitType.Drop ? NamedPhysicMaterials.GetPhysicMaterialByName(_possiblePhysicMaterials, "Drop") :
            NamedPhysicMaterials.GetPhysicMaterialByName(_possiblePhysicMaterials, "Normal"));

        // Initialization of the other ball physic parameters.
        _ballDetectionArea.Ball.InitializeActionParameters(NamedActions.GetActionParametersByName(_possibleActions, hitType.ToString()));

        // Applying a specific force in a specific direction and with a specific rising force factor.
        // If the player is doing a lob, there is no need to multiply the rising force of the ball by a factor.
        _ballDetectionArea.Ball.ApplyForce(hitForce, hitType == HitType.Lob ? 1f : _ballDetectionArea.GetRisingForceFactor(), horizontalDirection.normalized, this);
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
            _actionIndex = 4;
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
            _actionIndex = 5;
        }
    }

    private void TopSpin()
    {
        Shoot(HitType.TopSpin);
    }

    public void Drop(InputAction.CallbackContext context)
    {
        if (context.performed && PlayerState != PlayerStates.SERVE)
        {
            _actionIndex = 7;
        }
    }

    private void Drop()
    {
        Shoot(HitType.Drop);
    }

    public void Slice(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _actionIndex = 6;
        }
    }

    private void Slice()
    {
        Shoot(HitType.Slice);
    }

    public void Lob(InputAction.CallbackContext context)
    {
        if (context.performed && PlayerState != PlayerStates.SERVE)
        {
            _actionIndex = 8;
        }
    }

    private void Lob()
    {
        Shoot(HitType.Lob);
    }

    public void SlowTime(InputAction.CallbackContext context)
    {
        if (context.performed && PlayerState != PlayerStates.SERVE && GameManager.Instance.BallInstance.GetComponent<Ball>().LastPlayerToApplyForce != this)
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
        Time.timeScale = _actionParameters.SlowTimeScaleFactor;
        _currentSpeed = _movementSpeed / Time.timeScale;
    }

    public void TechnicalShot(InputAction.CallbackContext context)
    {
        if (context.performed && PlayerState != PlayerStates.SERVE && GameManager.Instance.BallInstance.GetComponent<Ball>().LastPlayerToApplyForce != this)
        {
            _actionIndex = 3;
        }
    }

    public void TechnicalShot()
    {
        float tempForwardMovementFactor = 0f;
        float tempRightMovementFactor = 0f;
        int forwardMovementFactor = 0;
        int rightMovementFactor = 0;

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
            forwardMovementFactor = 0;
            rightMovementFactor = (int)tempRightMovementFactor;
        }
        else
        {
            forwardMovementFactor = Mathf.Abs(tempForwardMovementFactor) > Mathf.Abs(tempRightMovementFactor) ? (int)tempForwardMovementFactor : 0;
            rightMovementFactor = Mathf.Abs(tempRightMovementFactor) > Mathf.Abs(tempForwardMovementFactor) ? (int)tempRightMovementFactor : 0;
        }

        Vector3 forwardVector = Vector3.Project(GameManager.Instance.SideManager.ActiveCameraTransform.forward, Vector3.forward).normalized;
        Vector3 rightVector = Vector3.Project(GameManager.Instance.SideManager.ActiveCameraTransform.right, Vector3.right).normalized;
        Vector3 wantedDirection = forwardMovementFactor * forwardVector + rightMovementFactor * rightVector;

        float distanceToBorderInWantedDirection = GameManager.Instance.GetDistanceToBorderByDirection(this, wantedDirection, forwardVector, rightVector);

        if (distanceToBorderInWantedDirection > _actionParameters.TechnicalShotMovementLength)
        {
            transform.position += wantedDirection * _actionParameters.TechnicalShotMovementLength;
        }
        else
        {
            transform.position += wantedDirection * distanceToBorderInWantedDirection;
        }
    }

    #endregion

    #region AGENT TRAINING METHODS

    public override void OnEpisodeBegin()
    {
        ServicesCount = 0;
        _hitKeyPressedTime = 0f;
        _isCharging = false;
        _currentSpeed = _movementSpeed;
        _actionIndex = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        continuousActions[0] = _movementVector.x;
        continuousActions[1] = _movementVector.y;
        discreteActions[0] = _actionIndex;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // If the game is in the end of point or the the end of match phase, the player can't move.
        // If the player is serving and threw the ball in the air, he can't move either.
        // Otherwise he can move with at least one liberty axis.
        if (GameManager.Instance.GameState != GameState.ENDPOINT && GameManager.Instance.GameState != GameState.ENDMATCH
            && !(PlayerState == PlayerStates.SERVE && !GameManager.Instance.BallInstance.GetComponent<Rigidbody>().isKinematic))
        {
            // The global player directions depend on the side he is on and its forward movement depends on the game phase.
            Vector3 rightVector = GameManager.Instance.SideManager.ActiveCameraTransform.right;

            Vector3 forwardVector = Vector3.zero;
            if (GameManager.Instance.GameState != GameState.SERVICE || !IsServing || PlayerState == PlayerStates.PLAY)
            {
                forwardVector = Vector3.Project(GameManager.Instance.SideManager.ActiveCameraTransform.forward, Vector3.forward);
            }

            Vector3 movementDirection = rightVector.normalized * actions.ContinuousActions[0] + forwardVector.normalized * actions.ContinuousActions[1];

            // The player moves according to the movement inputs.
            _rigidBody.velocity = movementDirection.normalized * _currentSpeed + new Vector3(0, _rigidBody.velocity.y, 0);
        }
        else
        {
            _rigidBody.velocity = new Vector3(0, _rigidBody.velocity.y, 0);
        }

        switch (actions.DiscreteActions[0])
        {
            case 0:
                // Doing nothing.
                break;
            case 1:
                // Throw the ball in the air during the service.
                GameManager.Instance.ThrowBall();
                // The action index is set to 0 after each action.
                _actionIndex = 0;
                break;
            case 2:
                // Slowing time.
                SlowTime();
                // The action index is set to 0 after each action.
                _actionIndex = 0;
                break;
            case 3:
                // Realising the technical shot.
                TechnicalShot();
                // The action index is set to 0 after each action.
                _actionIndex = 0;
                break;
            case 4:
                // Flat shot.
                Flat();
                // The action index is set to 0 after each action.
                _actionIndex = 0;
                break;
            case 5:
                // Top spin shot.
                TopSpin();
                // The action index is set to 0 after each action.
                _actionIndex = 0;
                break;
            case 6:
                // Slice shot.
                Slice();
                // The action index is set to 0 after each action.
                _actionIndex = 0;
                break;
            case 7:
                // Drop shot.
                Drop();
                // The action index is set to 0 after each action.
                _actionIndex = 0;
                break;
            case 8:
                // Lob shot.
                Lob();
                // The action index is set to 0 after each action.
                _actionIndex = 0;
                break;
            default:
                break;
        }
    }

    #endregion
}
