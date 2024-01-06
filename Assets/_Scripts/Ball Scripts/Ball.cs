using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Ball : MonoBehaviour
{
    #region PRIVATE FIELDS

    [Header("Target Parameters")]
    [SerializeField] private GameObject _targetPrefab;
    [SerializeField] private float _raycastLength;
    [SerializeField] private float _horizontalOffset;
    [SerializeField] private float _verticalOffsetFromGround;

    [Header("Components")]
    [SerializeField] private Rigidbody _rigidBody;

    [Header("Observed Variables")]
    [SerializeField] private ShotParameters _shotParameters;
    [SerializeField] private ControllersParent _lastPlayerToApplyForce;
    [SerializeField] private int _reboundsCount;
    [SerializeField] private GameObject _targetInstance;

    private float _risingForceFactor;
    private Coroutine _currentMovementCoroutine;
    private Coroutine _currentCurvingEffectCoroutine;
    private SphereCollider _sphereCollider;

    #endregion

    #region GETTERS

    public int ReboundsCount { get { return _reboundsCount; } }
    public ControllersParent LastPlayerToApplyForce { get { return _lastPlayerToApplyForce; } }

    public Rigidbody Rb => _rigidBody;

    #endregion

    #region UNITY METHODS

    private void Start()
    {
        _reboundsCount = 0;
        _sphereCollider = GetComponent<SphereCollider>();
    }

    private void Update()
    {
        if (transform.position.y < -1)
        {
            GameManager.Instance.EndOfPoint();
            ResetBall();
        }

        if (_rigidBody.isKinematic && GameManager.Instance.GameState == GameState.SERVICE)
        {
            transform.position = GameManager.Instance.ServiceBallInitializationPoint.position;
        }
        else if (!_rigidBody.isKinematic) 
        {
            DrawTarget();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.GetComponent<ControllersParent>() && !_rigidBody.isKinematic)
        {
            GameManager.Instance.EndOfPoint();
            GameManager.Instance.ScoreManager.AddPoint(_lastPlayerToApplyForce.PlayerTeam);
            ResetBall();
        }
    }

    #endregion

    #region PHYSICS BEHAVIOR METHODS

    public void InitializePhysicsMaterial(PhysicMaterial physicMaterial)
    {
        if (_sphereCollider.material != physicMaterial)
        {
            _sphereCollider.sharedMaterial = physicMaterial;
        }
    }

    public void InitializeActionParameters(ShotParameters shotParameters)
    {
        _shotParameters = shotParameters;
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
        float actualHorizontalForce = _shotParameters.ShotForceFactor * force;

        Vector3 curvingDirection = Vector3.Project(playerToApplyForce.gameObject.transform.position - transform.position, Vector3.right);
        Vector3 actualHorizontalDirection;
        if (playerToApplyForce is PlayerController) 
        {
            actualHorizontalDirection = playerToApplyForce.CalculateActualShootingDirection(normalizedHorizontalDirection, _shotParameters.ForceToDistanceFactor, actualHorizontalForce);
        }
        else
        {
            actualHorizontalDirection = normalizedHorizontalDirection;
        }

        _currentMovementCoroutine = StartCoroutine(BallMovement(actualHorizontalForce, actualHorizontalDirection.normalized, curvingDirection));

        _lastPlayerToApplyForce = playerToApplyForce;
    }

    private IEnumerator BallMovement(float actualHorizontalForce, Vector3 actualNormalizedHorizontalDirection, Vector3 curvingDirection)
    {
        _reboundsCount = 0;

        //Debug.Log($"Direction {actualNormalizedHorizontalDirection} - Force {actualHorizontalForce}");

        _rigidBody.AddForce(actualNormalizedHorizontalDirection * actualHorizontalForce);
        _rigidBody.AddForce(Vector3.up * _shotParameters.RisingForce * _risingForceFactor);

        _currentCurvingEffectCoroutine = StartCoroutine(CurvingEffect(curvingDirection));

        yield return new WaitForSeconds(_shotParameters.TimeBeforeGoingDown);

        float countdown = 0;
        while (countdown < _shotParameters.DecreasingForcePhaseTime)
        {
            _rigidBody.AddForce(-Vector3.up * _shotParameters.DecreasingForce);

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
                _rigidBody.AddForce(curvingDirection.normalized * _shotParameters.InAirCurvingForce);
            }
            else
            {
                _rigidBody.AddForce(curvingDirection.normalized * _shotParameters.AfterReboudCurvingForce);
                afterReboundCurvingEffectTime += Time.deltaTime;

                if (afterReboundCurvingEffectTime >= _shotParameters.AfterReboudCurvingEffectDuration)
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
        _rigidBody.AddForce(direction.normalized * (_shotParameters.AddedForceInSameDirection / _reboundsCount));
    }

    #endregion

    public void ResetBall()
    {
        if (_currentMovementCoroutine != null)
        {
            StopCoroutine(_currentMovementCoroutine);
        }
        if (_currentCurvingEffectCoroutine != null)
        {
            StopCoroutine(_currentCurvingEffectCoroutine);
        }

        _reboundsCount = 0;
        _lastPlayerToApplyForce = null;
        _rigidBody.velocity = Vector3.zero;
        _rigidBody.isKinematic = true;
        DestroyTarget();

        GameManager.Instance.GameState = GameState.SERVICE;
        GameManager.Instance.BallServiceInitialization();
    }

    #region SMASH TARGET MANAGEMENT

    private void DrawTarget()
    {
        if (_lastPlayerToApplyForce != null)
        {
            Ray ray = new Ray(transform.position, Vector3.down);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, _raycastLength) && hit.collider.gameObject.TryGetComponent<FieldGroundPart>(out FieldGroundPart fieldGroundPart))
            {
                if (fieldGroundPart.OwnerPlayer != _lastPlayerToApplyForce)
                {
                    Vector3 horizontalBallDirection = Vector3.Project(_rigidBody.velocity, Vector3.forward) + Vector3.Project(_rigidBody.velocity, Vector3.right);
                    Vector3 targetPosition = hit.point + Vector3.up * _verticalOffsetFromGround + horizontalBallDirection.normalized * _horizontalOffset;

                    if (_targetInstance == null)
                    {
                        InstantiateTarget(targetPosition);
                    }
                    else
                    {
                        MoveTarget(targetPosition);
                    }
                }
            }
            else
            {
                DestroyTarget();
            }
        }
    }

    private void MoveTarget(Vector3 position)
    {
        _targetInstance.transform.position = position;
    }

    public void DestroyTarget()
    {
        if (_targetInstance != null)
        {
            Destroy(_targetInstance);
            _targetInstance = null;
        }
    }

    private void InstantiateTarget(Vector3 position)
    {
        if (_targetPrefab != null)
        {
            if (_targetInstance != null)
            {
                Destroy(_targetInstance);
            }

            _targetInstance = Instantiate(_targetPrefab, position, Quaternion.Euler(90, 0, 0));
        }
    }

    #endregion
}


