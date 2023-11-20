using System.Collections;
using UnityEditor;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public bool PointNotFinished;

    #region PRIVATE FIELDS

    [SerializeField] private ActionParameters _actionParameters;

    [Header("Components")]
    [SerializeField] private Rigidbody _rigidBody;

    private ControllersParent _lastPlayerToApplyForce;
    private float _lastForceApplied;
    private Vector3 _lastNormalizedHorizontalDirection;
    private float _risingForceFactor;
    private int _reboundsCount;
    private Coroutine _currentCoroutineMovement;

    #endregion

    #region ACCESSORS

    public int ReboundsCount { get { return _reboundsCount; } }
    public ControllersParent LastPlayerToApplyForce { get { return _lastPlayerToApplyForce; } }

    #endregion

    #region UNITY METHODS

    private void Start()
    {
        _reboundsCount = 0;
        PointNotFinished = true;
    }

    private void Update()
    {
        if (transform.position.y < -1)
        {
            ResetBallFunction();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>())
        {
            ResetBallFunction();
        }
    }

    #endregion

    #region PHYSICS BEHAVIOR METHODS

    public void ResetBallFunction()
    {
        _rigidBody.velocity = Vector3.zero;
        gameObject.SetActive(false);
        _lastPlayerToApplyForce = null;
    }

    public void InitializeActionParameters(ActionParameters actionParameters)
    {
        _actionParameters = actionParameters;
    }

    public void ApplyForce(float force, float risingForceFactor, Vector3 normalizedHorizontalDirection, ControllersParent playerToApplyForce)
    {
        _rigidBody.velocity = Vector3.zero;
        
        if (_currentCoroutineMovement != null)
            StopCoroutine(_currentCoroutineMovement);
        
        _risingForceFactor = risingForceFactor;

        _currentCoroutineMovement = StartCoroutine(BallMovement(force, normalizedHorizontalDirection));

        _lastPlayerToApplyForce = playerToApplyForce;
        _lastForceApplied = force;
        _lastNormalizedHorizontalDirection = normalizedHorizontalDirection;
    }

    private IEnumerator BallMovement(float force, Vector3 normalizedDirection)
    {
        _reboundsCount = 0;

        _rigidBody.AddForce(normalizedDirection * force);
        _rigidBody.AddForce(Vector3.up * _actionParameters.RisingForce * _risingForceFactor);

        yield return new WaitForSeconds(_actionParameters.TimeBeforeGoingDown);

        float countdown = 0;
        while (countdown < _actionParameters.DecreasingForcePhaseTime)
        {
            _rigidBody.AddForce(-Vector3.up * _actionParameters.DecreasingForce);

            yield return new WaitForSeconds(Time.deltaTime);

            countdown += Time.deltaTime;
        }
    }

    #endregion
}
