using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : ControllersParent
{
    #region PRIVATE FIELDS

    [SerializeField] private List<NamedActions> _possibleActions;

    [Header("Components")]
    [SerializeField] private Rigidbody _rigidBody;
    [SerializeField] private BallDetection _ballDetectionArea;

    [Header("Movements and Hit Parameters")]
    [SerializeField] private float _movementSpeed;
    [SerializeField] protected float _minimumHitForce;
    [SerializeField] protected float _maximumHitForce;
    [SerializeField] private float _minimumHitKeyPressTimeToIncrementForce;
    [SerializeField] private float _maximumHitKeyPressTime;

    private Vector2 _movementVector;
    private float _hitKeyPressedTime;
    private float _hitForce;
    private bool _isCharging;

    #endregion

    #region UNITY METHODS

    private void Start()
    {
        _hitKeyPressedTime = 0f;
        _isCharging = false;
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
        // The player moves according to the movement inputs.
        _rigidBody.velocity = (new Vector3(_movementVector.x, 0, _movementVector.y)).normalized * _movementSpeed + new Vector3(0, _rigidBody.velocity.y, 0);
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
        _hitForce = _minimumHitForce + ((hitKeyPressTime - _minimumHitKeyPressTimeToIncrementForce) / (_maximumHitKeyPressTime - _minimumHitKeyPressTimeToIncrementForce)) * (_maximumHitForce - _minimumHitForce);

        _hitKeyPressedTime = 0f;
        _isCharging = false;

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

        _ballDetectionArea.Ball.InitializeActionParameters(NamedActions.GetActionParametersByName(_possibleActions, hitType.ToString()));
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

    public void Lob(InputAction.CallbackContext context)
    {
/*        if (context.performed)
        {
            if (CanShoot)
            {
                CanShoot = false;
                Shoot(HitType.Lob);
            }
            else if (CanShoot == false)
            {
                chargeForce = 1;
                isCharging = false;
            }
            if (ball == null)
            {
                CanShoot = false;
                chargeForce = 1;
            }
        }*/
    }

    public void Slice(InputAction.CallbackContext context)
    {
/*        if (context.performed)
        {
            if (CanShoot)
            {
                CanShoot = false;
                Shoot(HitType.Slice);
            }
            else if (CanShoot == false)  
            {
                chargeForce = 1;
                isCharging = false;
            }
            if (CanShoot)
            {
                CanShoot = false;
                Shoot(HitType.Slice);
            }
        }*/
    }

    public void TopSpin(InputAction.CallbackContext context)
    {
/*        if (context.performed)
        {
            if (ball != null)
            {
                CanShoot = false;
                Shoot(HitType.TopSpin);
            }
            else if (CanShoot == false)
            {
                chargeForce = 1;
                isCharging = false;
            }
            if (CanShoot)
            {
                CanShoot = false;
                Shoot(HitType.TopSpin);
            }
        }*/
    }

    public void Drop(InputAction.CallbackContext context)
    {
/*        if (context.performed)
        {
            if (CanShoot)
            {
                CanShoot = false;
                Shoot(HitType.Drop);
            }
            else if (CanShoot == false)
            {
                chargeForce = 1;
                isCharging = false;
            }
        }*/
    }

    public void Flat(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Shoot(HitType.Flat);
        }
    }

    public void ServeThrow(InputAction.CallbackContext context)
    {

    }

    #endregion
}