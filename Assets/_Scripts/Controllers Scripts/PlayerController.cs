using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : ControllersParent
{
    #region PRIVATE FIELDS

    [SerializeField] private ActionParameters _actionParameters;

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


    private CameraController _cameraController;
    private Vector2 _movementVector;
    private float _currentSpeed;
    private float _hitKeyPressedTime;
    private float _hitForce;
    private bool _isCharging;

    #endregion

    #region UNITY METHODS

    private void Start()
    {
        _hitKeyPressedTime = 0f;
        _isCharging = false;
        _currentSpeed = _movementSpeed;
        _cameraController = GetComponent<CameraController>();
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
        if (!_cameraController.GetIsSmashing())
        {
            // The global player directions depend on the side he is on.
            Vector3 rightVector = GameManager.Instance.SideManager.ActiveCameraTransform.right;
            Vector3 forwardVector = Vector3.Project(GameManager.Instance.SideManager.ActiveCameraTransform.forward, Vector3.forward);
            Vector3 movementDirection = rightVector.normalized * _movementVector.x + forwardVector.normalized * _movementVector.y;

            // The player moves according to the movement inputs.
            /*_rigidBody.velocity = (new Vector3(_movementVector.x, 0, _movementVector.y)).normalized * _currentSpeed + new Vector3(0, _rigidBody.velocity.y, 0);*/
            _rigidBody.velocity = movementDirection.normalized * _currentSpeed + new Vector3(0, _rigidBody.velocity.y, 0);
        }
    }

    #endregion

    #region ACTION METHODS

    private void Shoot(HitType hitType)
    {
        if (!_ballDetectionArea.IsBallInHitZone || _ballDetectionArea.Ball.LastPlayerToApplyForce == this)
        {
            _hitKeyPressedTime = 0f;
            _isCharging = false;
            return;
        }

        float hitKeyPressTime = Mathf.Clamp(_hitKeyPressedTime, _minimumHitKeyPressTimeToIncrementForce, _maximumHitKeyPressTime);
        _hitForce = _minimumShotForce + ((hitKeyPressTime - _minimumHitKeyPressTimeToIncrementForce) / (_maximumHitKeyPressTime - _minimumHitKeyPressTimeToIncrementForce)) * (_maximumShotForce - _minimumShotForce);

        _hitKeyPressedTime = 0f;
        _isCharging = false;

        if (CurrentState == PlayerStates.SERVE)
        {
            CurrentState = PlayerStates.PLAY;
            _ballServiceDetectionArea.gameObject.SetActive(false);
            GameManager.Instance.ServiceManager.DisableLockServiceColliders();
            GameManager.Instance.CurrentState = GameState.PLAYING;
        }

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
        _ballDetectionArea.Ball.ApplyForce(_hitForce, _ballDetectionArea.GetRisingForceFactor(), horizontalDirection.normalized, this);
    }

    public void Move(InputAction.CallbackContext context)
    {
        _movementVector = context.ReadValue<Vector2>();
    }

    public void ChargeShot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _isCharging = true;
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
        if (context.performed)
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
        if (context.performed)
        {
            Shoot(HitType.Lob);
        }
    }

    public void SlowTime(InputAction.CallbackContext context)
    {
        if (context.performed)
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
        if (context.performed)
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

            Vector3 wantedDirection = forwardMovementFactor * Vector3.forward + rightMovementFactor * Vector3.right;
            float distanceToBorderInWantedDirection = GameManager.Instance.GetDistanceToBorderByDirection(this, wantedDirection);

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

    public void ServeThrow(InputAction.CallbackContext context)
    {

    }
    public void Smash()
    {

    }
    #endregion
}