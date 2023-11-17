using System.Collections;
using UnityEditor;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public bool PointNotFinished;

    #region PRIVATE FIELDS

    [Header("Components")]
    [SerializeField] private Rigidbody _rigidBody;

    [Header("Physics Parameters")]
    [SerializeField] private float _risingForce;
    [SerializeField] private float _timeBeforeGoingDown;
    [SerializeField] private float _decreasingForcePhaseTime;
    [SerializeField] private float _decreasingForce;
    [SerializeField] private float _reboundHorizontalDirectionFactor;
    [SerializeField] private float _reboundVerticalDirectionFactor;
    [SerializeField] private float _bouncingForce;
    [SerializeField] private float _clampMaximumVelocity;

    private ControllersParent _lastPlayerToApplyForce;
    private float _lastForceApplied;
    private Vector3 _lastNormalizedHorizontalDirection;
    private float _risingForceFactor;
    private int _reboundsCount;

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
        if (transform.position.y < 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>())
        {
            Destroy(gameObject);
        }
    }

    #endregion

    #region PHYSICS BEHAVIOR METHODS

    public void ApplyForce(float force, Vector3 normalizedHorizontalDirection, ControllersParent playerToApplyForce)
    {
        _rigidBody.velocity = Vector3.zero;
        StopCoroutine(BallMovement(_lastForceApplied, _lastNormalizedHorizontalDirection));
        _risingForceFactor = 1;

        if (playerToApplyForce != _lastPlayerToApplyForce)
        {
            StartCoroutine(BallMovement(force, normalizedHorizontalDirection));
        }

        _lastPlayerToApplyForce = playerToApplyForce;
        _lastForceApplied = force;
        _lastNormalizedHorizontalDirection = normalizedHorizontalDirection;
    }

    public void ApplyForce(float force, float risingForceFactor, Vector3 normalizedHorizontalDirection, ControllersParent playerToApplyForce)
    {
        _rigidBody.velocity = Vector3.zero;
        StopCoroutine(BallMovement(_lastForceApplied, _lastNormalizedHorizontalDirection));
        _risingForceFactor = risingForceFactor;

        if (playerToApplyForce != _lastPlayerToApplyForce)
        {
            StartCoroutine(BallMovement(force, normalizedHorizontalDirection));
        }

        _lastPlayerToApplyForce = playerToApplyForce;
        _lastForceApplied = force;
        _lastNormalizedHorizontalDirection = normalizedHorizontalDirection;
    }

    private IEnumerator BallMovement(float force, Vector3 normalizedDirection)
    {
        _reboundsCount = 0;

        _rigidBody.AddForce(normalizedDirection * force);
        _rigidBody.AddForce(Vector3.up * _risingForce * _risingForceFactor);

        yield return new WaitForSeconds(_timeBeforeGoingDown);

        float countdown = 0;
        while (countdown < _decreasingForcePhaseTime)
        {
            _rigidBody.AddForce(-Vector3.up * _decreasingForce);

            yield return new WaitForSeconds(Time.deltaTime);

            countdown += Time.deltaTime;
        }
    }

    public void Rebound()
    {
        StopCoroutine(BallMovement(_lastForceApplied, _lastNormalizedHorizontalDirection));

        _reboundsCount++;

        Vector3 reboundDirection = new Vector3(_rigidBody.velocity.x * _reboundHorizontalDirectionFactor, _reboundVerticalDirectionFactor * _rigidBody.velocity.magnitude, _rigidBody.velocity.z * _reboundHorizontalDirectionFactor);
        float weightedForce = _bouncingForce * (Mathf.Clamp(_rigidBody.velocity.magnitude, 0f, _clampMaximumVelocity) / _clampMaximumVelocity);
        _rigidBody.AddForce(weightedForce * reboundDirection.normalized);
    }

    #endregion
}
