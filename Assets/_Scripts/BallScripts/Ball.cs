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
    private float _risingForceFactor;
    private int _reboundsCount;
    private Coroutine _currentMovementCoroutine;
    private Coroutine _currentCurvingEffectCoroutine;

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
        _lastPlayerToApplyForce = null;
        _rigidBody.velocity = Vector3.zero;
        /*gameObject.SetActive(false);*/
        _lastPlayerToApplyForce = null;
    }

    public void InitializePhysicsMaterial(PhysicMaterial physicMaterial)
    {
        if (gameObject.GetComponent<SphereCollider>().material != physicMaterial)
        {
            gameObject.GetComponent<SphereCollider>().sharedMaterial = physicMaterial;
        }
    }

    public void InitializeActionParameters(ActionParameters actionParameters)
    {
        _actionParameters = actionParameters;
    }

    public void ApplyForce(float force, float risingForceFactor, Vector3 normalizedHorizontalDirection, ControllersParent playerToApplyForce)
    {
        _rigidBody.velocity = Vector3.zero;
        
        if (_currentMovementCoroutine != null)
        {
            StopCoroutine(_currentMovementCoroutine);
        }

        if (_currentCurvingEffectCoroutine != null)
        {
            StopCoroutine(_currentCurvingEffectCoroutine);
        }
        
        _risingForceFactor = risingForceFactor;
        Vector3 curvingDirection = Vector3.Project(playerToApplyForce.gameObject.transform.position - transform.position, Vector3.right);

        _currentMovementCoroutine = StartCoroutine(BallMovement(force, normalizedHorizontalDirection, curvingDirection));

        _lastPlayerToApplyForce = playerToApplyForce;
    }

    private IEnumerator BallMovement(float force, Vector3 normalizedDirection, Vector3 curvingDirection)
    {
        _reboundsCount = 0;

        _rigidBody.AddForce(normalizedDirection * force * _actionParameters.ShotForceFactor);
        _rigidBody.AddForce(Vector3.up * _actionParameters.RisingForce * _risingForceFactor);

        _currentCurvingEffectCoroutine = StartCoroutine(CurvingEffect(curvingDirection));

        yield return new WaitForSeconds(_actionParameters.TimeBeforeGoingDown);

        float countdown = 0;
        while (countdown < _actionParameters.DecreasingForcePhaseTime)
        {
            _rigidBody.AddForce(-Vector3.up * _actionParameters.DecreasingForce);

            yield return new WaitForSeconds(Time.deltaTime);

            countdown += Time.deltaTime;
        }
    }

    private IEnumerator CurvingEffect(Vector3 curvingDirection)
    {
        float afterReboundCurvingEffectTime = 0f;

        while (true)
        {
            if (_reboundsCount == 0)
            {
                _rigidBody.AddForce(curvingDirection.normalized * _actionParameters.InAirCurvingForce);
            }
            else
            {
                _rigidBody.AddForce(curvingDirection.normalized * _actionParameters.AfterReboudCurvingForce);
                afterReboundCurvingEffectTime += Time.deltaTime;

                if (afterReboundCurvingEffectTime >= _actionParameters.AfterReboudCurvingEffectDuration)
                {
                    break;
                }
            }

            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    public void Rebound()
    {
        _reboundsCount++;

        Vector3 direction = Vector3.Project(_rigidBody.velocity, Vector3.forward) + Vector3.Project(_rigidBody.velocity, Vector3.right);
        _rigidBody.AddForce(direction.normalized * (_actionParameters.AddedForceInSameDirection / _reboundsCount));
    }

    #endregion
}
