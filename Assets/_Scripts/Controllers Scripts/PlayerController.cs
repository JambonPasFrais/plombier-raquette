using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : ControllersParent
{
    #region PRIVATE FIELDS

    [Header("Ball Physical Behavior Parameters")]
    [SerializeField] private List<NamedActions> _possibleActions;
    [SerializeField] private List<NamedPhysicMaterials> _possiblePhysicMaterials;

    [Header("Components")]
    [SerializeField] private Rigidbody _rigidBody;
    [SerializeField] private BallDetection _ballDetectionArea;
    [SerializeField] private PlayerCameraController _playerCameraController;
    
    [Header("Movements and Hit Parameters")]
    [SerializeField] private float _movementSpeed;

    [SerializeField] private float _chargingMoveSpeed;
    [SerializeField] private float _minimumHitKeyPressTimeToIncrementForce;
    [SerializeField] private float _maximumHitKeyPressTime;

    [Header("Smash Parameters")]
    [SerializeField] private bool _canSmash;
    
    private Vector2 _movementVector;
    private float _currentSpeed;
    private Ball _ballInstance;
    private ShootDirectionController _shootDirectionController;
    private Vector3 _shotDirection;

    #endregion

    #region GETTERS

    public List<NamedPhysicMaterials> PossiblePhysicMaterials { get { return _possiblePhysicMaterials; } }
    public List<NamedActions> PossibleActions { get { return _possibleActions; } }

    public PlayerCameraController PlayerCameraController => _playerCameraController;

    #endregion

    #region SETTERS

    public void SetCanSmash(bool canSmash)
    {
        _canSmash = canSmash;
    }

    public void SetDirectionController(ShootDirectionController shootDirectionController)
    {
        _shootDirectionController = shootDirectionController;
    }

    #endregion

    #region UNITY METHODS

    private void Start()
    {
        ServicesCount = 0;
        _hitKeyPressedTime = 0f;
        _isCharging = false;
        _currentSpeed = _movementSpeed;
        if (_playerCameraController == null)
            _playerCameraController = GetComponent<PlayerCameraController>();
        _ballInstance = GameManager.Instance.BallInstance.GetComponent<Ball>();
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
    }

    private void FixedUpdate()
    {
        // If the game is in the end of point or the the end of match phase, the player can't move.
        // If the player is serving and threw the ball in the air, he can't move either.
        // Otherwise he can move with at least one liberty axis.
        if (GameManager.Instance.GameState != GameState.ENDPOINT && GameManager.Instance.GameState != GameState.ENDMATCH
            && !(PlayerState == PlayerStates.SERVE && !_ballInstance.Rb.isKinematic) && _playerCameraController.IsFirstPersonView == false) 
        {
            // The global player directions depend on the side he is on and its forward movement depends on the game phase.
            Vector3 rightVector = GameManager.Instance.CameraManager.GetActiveCameraTransformBySide(IsInOriginalSide).right;
        
            Vector3 forwardVector = Vector3.zero;
            if (GameManager.Instance.GameState != GameState.SERVICE || !IsServing || PlayerState == PlayerStates.PLAY) 
            {
                forwardVector = Vector3.Project(GameManager.Instance.CameraManager.GetActiveCameraTransformBySide(IsInOriginalSide).forward, Vector3.forward);
            }

            Vector3 movementDirection = rightVector.normalized * _movementVector.x + forwardVector.normalized * _movementVector.y;

            // The player moves according to the movement inputs.
            _rigidBody.velocity = movementDirection.normalized * _currentSpeed + new Vector3(0, _rigidBody.velocity.y, 0);
            
            //_rigidBody.MovePosition();
            
            #region Animations

            if (!_isShooting && !_isSmashing)
            {
                if (movementDirection.normalized != Vector3.zero)
                {
                    // Type of walks
                    if (movementDirection.normalized.x >= 0.5) //left
                        if (IsInOriginalSide)
                            _playerAnimator.MoveLeftAnimation();
                        else
                            _playerAnimator.MoveRightAnimation();  
                    
                    else if (movementDirection.normalized.x <= -0.5) //right
                        if (IsInOriginalSide)
                            _playerAnimator.MoveRightAnimation();
                        else
                            _playerAnimator.MoveLeftAnimation();
                    
                    else if(movementDirection.normalized.z >= 0.5) // front
                        _playerAnimator.MoveFrontAnimation();
                    
                    else if (movementDirection.normalized.z <= -0.5) //back
                        _playerAnimator.MoveBackwardAnimation();
                }
                else
                {
                    _playerAnimator.IdleAnimation();
                }
            }

            #endregion
        }
        else
        {
            _rigidBody.velocity = new Vector3(0, _rigidBody.velocity.y, 0);
        }
    }

    #endregion

    #region ACTION METHODS

    private void Shoot(HitType hitType)
    {
        // If there is no ball in the hit volume or if the ball rigidbody is kinematic or if the player already applied force to the ball or if the game phase is in end of point,
        // then the player can't shoot in the ball.
        if (!_ballDetectionArea.IsBallInHitZone  || _ballDetectionArea.Ball.gameObject.GetComponent<Rigidbody>().isKinematic 
            || _ballDetectionArea.Ball.LastPlayerToApplyForce == this || GameManager.Instance.GameState == GameState.ENDPOINT)
        {
            _hitKeyPressedTime = 0f;
            _isCharging = false;
            _currentSpeed = _movementSpeed;
            return;
        }
        
        #region ANIMATIONS
        _playerAnimator.StrikeAnimation();
        _isShooting = true;
        #endregion
        
        // The force to apply to the ball is calculated considering how long the player pressed the key and where is the player compared to the net position.
        float hitKeyPressTime = hitType == HitType.Lob ? _minimumHitKeyPressTimeToIncrementForce : Mathf.Clamp(_hitKeyPressedTime, _minimumHitKeyPressTimeToIncrementForce, _maximumHitKeyPressTime);
        float wantedHitForce = _minimumShotForce + ((hitKeyPressTime - _minimumHitKeyPressTimeToIncrementForce) / (_maximumHitKeyPressTime - _minimumHitKeyPressTimeToIncrementForce)) * (_maximumShotForce - _minimumShotForce);
        float hitForce = CalculateActualForce(wantedHitForce);

        // Hit charging variables are reset.
        _hitKeyPressedTime = 0f;
        _isCharging = false;
        _currentSpeed = _movementSpeed;

        // Reseting smash and target states.
        _canSmash = false;
        //_cameraController.SetCanSmash(false);
        _ballDetectionArea.Ball.DestroyTarget();

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

        #region Shoot Direction
        
        Vector3 horizontalDirection;

        if (_shootDirectionController == ShootDirectionController.MOUSE)
        {
            // The hit direction is set according to the mouse position on the screen.
            if (Physics.Raycast(Camera.main.ScreenPointToRay(_shotDirection), out var hit, float.MaxValue, ~LayerMask.GetMask("Player")))
            {
                Vector3 position = transform.position;
                horizontalDirection = Vector3.Project(hit.point - position, Vector3.forward) + Vector3.Project(hit.point - position, Vector3.right);
            }
            else
            {
                horizontalDirection = Vector3.forward;
            }
        }
        else // player uses a controller
        {
            // The _shotDirection will have different values depending on the side and the orientation of the character
            // The global player directions depend on the side he is on and its forward movement depends on the game phase.
            Vector3 rightVector = GameManager.Instance.CameraManager.GetActiveCameraTransformBySide(IsInOriginalSide).right;
            Vector3 forwardVector = Vector3.Project(GameManager.Instance.CameraManager.GetActiveCameraTransformBySide(IsInOriginalSide).forward, Vector3.forward);

            Vector3 shotForwardDir = rightVector.normalized * _shotDirection.x + forwardVector.normalized * _shotDirection.y;
            
            // EXPLANATION
            // if player's joystick is in neutral position, we still need a direction and this is Vector3.forward
            // if player's joystick is not in neutral position, _shotDirection will take values in x and y, HOWEVER the direction of the shot is in x and z in the game.
            // So we need to adapt it by doing do following "new Vector3(_shotDirection.x, 0, _shotDirection.y)"
            horizontalDirection = shotForwardDir == Vector3.zero ? Vector3.forward : shotForwardDir;
        }
        
        #endregion

        // Initialization of the correct ball physic material.
        _ballDetectionArea.Ball.InitializePhysicsMaterial(hitType == HitType.Drop ? NamedPhysicMaterials.GetPhysicMaterialByName(_possiblePhysicMaterials, "Drop") :
            NamedPhysicMaterials.GetPhysicMaterialByName(_possiblePhysicMaterials, "Normal"));

        // Initialization of the other ball physic parameters.
        _ballDetectionArea.Ball.InitializeActionParameters(NamedActions.GetActionParametersByName(_possibleActions, hitType.ToString()));

        // Applying a specific force in a specific direction and with a specific rising force factor.
        // If the player is doing a lob, there is no need to multiply the rising force of the ball by a factor.
        _ballDetectionArea.Ball.ApplyForce(hitForce, _ballDetectionArea.GetRisingForceFactor(hitType), horizontalDirection.normalized, this);
    }

    public void AimShot(InputAction.CallbackContext context)
    {
        _shotDirection = context.ReadValue<Vector2>();
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
            _currentSpeed = _chargingMoveSpeed;
        }
    }

    public void Flat(InputAction.CallbackContext context)
    {
        if (context.performed && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftShift))
        {
            Shoot(HitType.Flat);
        }
    }

    public void TopSpin(InputAction.CallbackContext context)
    {
        if (context.performed && !Input.GetKey(KeyCode.LeftControl)) 
        {
            Shoot(HitType.TopSpin);
        }
    }

    public void Drop(InputAction.CallbackContext context)
    {
        if (context.performed && PlayerState != PlayerStates.SERVE)
        {
            Shoot(HitType.Drop);
        }
    }

    public void Slice(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Shoot(HitType.Slice);
        }
    }

    public void Lob(InputAction.CallbackContext context)
    {
        if (context.performed && PlayerState != PlayerStates.SERVE)
        {
            Shoot(HitType.Lob);
        }
    }

    public void SlowTime(InputAction.CallbackContext context)
    {
        if (context.performed && PlayerState != PlayerStates.SERVE && _ballInstance.LastPlayerToApplyForce != this)
        {
            Time.timeScale = _actionParameters.SlowTimeScaleFactor;
            _currentSpeed = _movementSpeed / Time.timeScale;
        }
        else if (context.canceled)
        {
            Time.timeScale = 1f;
            _currentSpeed = _movementSpeed;
        }
    }

    public void TechnicalShot(InputAction.CallbackContext context)
    {
        if (context.performed && PlayerState != PlayerStates.SERVE && _ballInstance.LastPlayerToApplyForce != this) 
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

            Vector3 forwardVector = Vector3.Project(GameManager.Instance.CameraManager.GetActiveCameraTransformBySide(IsInOriginalSide).forward, Vector3.forward).normalized;
            Vector3 rightVector = Vector3.Project(GameManager.Instance.CameraManager.GetActiveCameraTransformBySide(IsInOriginalSide).right, Vector3.right).normalized;
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
    }

    public void ServiceThrow(InputAction.CallbackContext context)
    {
        ThrowBall();
    }

    public void PrepareSmash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_playerCameraController.IsFirstPersonView)
                return;
            if (GameManager.Instance.GameState != GameState.PLAYING)
                return;
            if (!_canSmash)
                return;
            
            _playerCameraController.ToggleFirstPersonView();
            
            GameManager.Instance.CameraManager.ToggleGameCamerasForSmash();
            
            _ballInstance.gameObject.transform.rotation = Quaternion.identity;
        }
    }

    public void Smash(InputAction.CallbackContext context)
    {
        if (!_playerCameraController.IsFirstPersonView)
            return;
        
        if (context.performed)
        {
            GameManager.Instance.CameraManager.ToggleGameCamerasForSmash();
            
            _canSmash = false;

            _ballInstance.DestroyTarget();
            _ballInstance.Rb.isKinematic = false;
            _ballInstance.InitializePhysicsMaterial(NamedPhysicMaterials.GetPhysicMaterialByName(_possiblePhysicMaterials, "Normal"));
            _ballInstance.InitializeActionParameters(NamedActions.GetActionParametersByName(_possibleActions, "Smash"));

            Vector3 playerCameraTransformForward = _playerCameraController.FirstPersonCamera.transform.forward;
            
            _ballInstance.ApplyForce(_maximumShotForce, 0f, playerCameraTransformForward.normalized, this);
            
            _playerCameraController.ToggleFirstPersonView();
            
            #region Animations
            _playerAnimator.SmashAnimation();
            _isSmashing = true; 
            #endregion
        }
    }

    #endregion
    
    #region ANIMATIONS

    public void ShootingAnimationEnd()
    {
        _isShooting = false;
    }

    public void SmashAnimationEnd()
    {
        _isSmashing = false;
    }
    
    #endregion
}