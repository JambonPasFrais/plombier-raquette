using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Ball : MonoBehaviour
{
    #region PRIVATE FIELDS

    [SerializeField] private ShotParameters _shotParameters;

    [Header("Components")]
    [SerializeField] private Rigidbody _rigidBody;

    private ControllersParent _lastPlayerToApplyForce;
    private float _risingForceFactor;
    private int _reboundsCount;
    private Coroutine _currentMovementCoroutine;
    private Coroutine _currentCurvingEffectCoroutine;

    private float _staticTime;

    #endregion

    #region ACCESSORS

    public int ReboundsCount { get { return _reboundsCount; } }
    public ControllersParent LastPlayerToApplyForce { get { return _lastPlayerToApplyForce; } }

    #endregion

    #region UNITY METHODS

    private void Start()
    {
        _reboundsCount = 0;
        _staticTime = 0f;
    }

    private void Update()
    {
        if (transform.position.y < -1)
        {
            GameManager.Instance.EndOfPoint();
            ResetBall();
        }

        if (_rigidBody.isKinematic)
        {
            transform.position = GameManager.Instance.ServiceBallInitializationPoint.position;
        }

        // If the ball is static during at least 5 seconds, the ball and the players position and state are reset for the service.
        if (_rigidBody.velocity.magnitude == 0f)
        {
            if (_staticTime >= 5f)
            {
                GameManager.Instance.EndOfPoint();
                GameManager.Instance.SideManager.SetSidesInSimpleMatch(GameManager.Instance.Controllers, GameManager.Instance.ServiceManager.ServeRight,
                    !GameManager.Instance.ServiceManager.ChangeSides);
                GameManager.Instance.ServiceManager.EnableLockServiceColliders();
                ResetBall();
                ControllersParent controller;
                if ((controller = GameManager.Instance.Controllers[GameManager.Instance.ServerIndex]) is BotBehavior)
                {
                    ((BotBehavior)controller).CallingBotServiceMethod();
                }
                _staticTime = 0f;
            }
            else
            {
                _staticTime += Time.deltaTime;
            }
        }
        else if (_staticTime > 0f)
        {
            _staticTime = 0f;
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
        if (gameObject.GetComponent<SphereCollider>().material != physicMaterial)
        {
            gameObject.GetComponent<SphereCollider>().sharedMaterial = physicMaterial;
        }
    }

    public void InitializeActionParameters(ShotParameters shotParameters)
    {
        _shotParameters = shotParameters;
    }

    public void ApplyForce(float force, float risingForceFactor, Vector3 normalizedHorizontalDirection, ControllersParent playerToApplyForce)
    {
        // If the ball touches the service collider before it serves, the bot can re serve.
        // Happens during AI training for some reason.
        if(_rigidBody.isKinematic && playerToApplyForce is BotBehavior)
        {
            BotBehavior bot = (BotBehavior)playerToApplyForce;
            bot.CallingBotServiceMethod();
        }

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
        if(playerToApplyForce is PlayerController)
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

        if (_reboundsCount == 1)
        {
            Vector3 horizontalDistanceVector = Vector3.Project(transform.position - _lastPlayerToApplyForce.transform.position, Vector3.forward) +
                Vector3.Project(transform.position - _lastPlayerToApplyForce.transform.position, Vector3.right);
            Debug.Log($"Real distance travelled until first rebound : {horizontalDistanceVector.magnitude}");
        }

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

        GameManager.Instance.GameState = GameState.SERVICE;
        GameManager.Instance.BallServiceInitialization();
    }
}
