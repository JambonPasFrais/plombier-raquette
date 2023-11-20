using System.Threading;
using UnityEngine;

public class PlayerController : ControllersParent
{
    #region PRIVATE FIELDS

    [Header("Components")]
    [SerializeField] private Rigidbody _rigidBody;
    [SerializeField] private BallDetection _ballDetectionArea;

    [Header("Movements and Hit Parameters")]
    [SerializeField] private float _movementSpeed;
    [SerializeField] protected float _minimumHitForce;
    [SerializeField] protected float _maximumHitForce;
    [SerializeField] private float _minimumHitKeyPressTimeToIncrementForce;
    [SerializeField] private float _maximumHitKeyPressTime;

    private Vector3 _movementVector;
    private float _verticalInput;
    private float _horizontalInput;
    private float _hitKeyPressedTime;
    private float _hitForce;

    #endregion

    #region UNITY METHODS

    private void Start()
    {
        _hitKeyPressedTime = 0f;
    }

    void Update()
    {
        _movementVector = Vector3.zero;

        if ((_verticalInput = Input.GetAxis("Vertical")) != 0) 
        {
            // Set vertical input to zero if no vertical movement key is pressed anymore.
            _verticalInput = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow)) ? _verticalInput : 0;
            _movementVector += _verticalInput * Vector3.forward;
        }

        if ((_horizontalInput = Input.GetAxis("Horizontal")) != 0) 
        {
            // Set horizontal input to zero if no horizontal movement key is pressed anymore.
            _horizontalInput = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.LeftArrow)) ? _horizontalInput : 0;
            _movementVector += _horizontalInput * Vector3.right;
        }

        // The player pressed the hit key.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_hitKeyPressedTime < _maximumHitKeyPressTime)
            {
                _hitKeyPressedTime += Time.deltaTime;
            }
        }

        // The player wants to hit the ball with a specific force.
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (!_ballDetectionArea.IsBallInHitZone || _ballDetectionArea.Ball.LastPlayerToApplyForce == this)
            {
                _hitKeyPressedTime = 0f;
                return;
            }

            float hitKeyPressTime = Mathf.Clamp(_hitKeyPressedTime, _minimumHitKeyPressTimeToIncrementForce, _maximumHitKeyPressTime);
            _hitForce = _minimumHitForce + ((hitKeyPressTime - _minimumHitKeyPressTimeToIncrementForce) / (_maximumHitKeyPressTime - _minimumHitKeyPressTimeToIncrementForce)) * (_maximumHitForce - _minimumHitForce);
            
            _hitKeyPressedTime = 0f;

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

            _ballDetectionArea.Ball.ApplyForce(_hitForce, _ballDetectionArea.GetRisingForceFactor(), horizontalDirection.normalized, this);
        }
    }

    private void FixedUpdate()
    {
        _rigidBody.velocity = _movementVector.normalized * _movementSpeed;
    }

    #endregion
}
