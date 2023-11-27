using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class Ball : MonoBehaviour
{
    #region PRIVATE FIELDS

    [SerializeField] private bool _isOnOtherSide;

    [SerializeField] private ShotParameters _actionParameters;

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
        _isOnOtherSide = false;
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
        if (GameManager.Instance.CurrentState == GameState.SERVICE)
        {
            if (collision.gameObject.TryGetComponent<BallServiceDetection>(out BallServiceDetection ballServiceDetection))
            {
                gameObject.SetActive(false);
                _rigidBody.velocity = Vector3.zero;
                ballServiceDetection.Player.CurrentState = PlayerStates.IDLE;
            }
            else if (!collision.gameObject.GetComponent<ServiceBoxDetection>() && _reboundsCount == 0)
            {
                Debug.Log("Service Faux... Deuxième Service !");
            }
        }

        if (!_isOnOtherSide && GameManager.Instance.CurrentState == GameState.PLAYING)
        {
            Teams otherTeam = (Teams)(Enum.GetValues(typeof(Teams)).GetValue(((int)_lastPlayerToApplyForce.PlayerTeam + 1) % Enum.GetValues(typeof(Teams)).Length));
            ScoreManager.Instance.AddPoint(otherTeam);
            /*StartCoroutine(ReinitializeBall());*/
            ResetBallFunction();
        }
        else if (GameManager.Instance.CurrentState == GameState.PLAYING)
        {
            if (collision.gameObject.GetComponent<PlayerController>() && _lastPlayerToApplyForce != collision.gameObject.GetComponent<ControllersParent>())
            {
                ScoreManager.Instance.AddPoint(_lastPlayerToApplyForce.PlayerTeam);
                /*StartCoroutine(ReinitializeBall());*/
                ResetBallFunction();
            }

            else if (collision.gameObject.TryGetComponent<CourtDetections>(out CourtDetections detection))
            {
                Debug.Log(collision.gameObject.name);

                if (_reboundsCount == 1)
                {
                    ScoreManager.Instance.AddPoint(_lastPlayerToApplyForce.PlayerTeam);
                    /*StartCoroutine(ReinitializeBall());*/
                    ResetBallFunction();
                }

                else
                {
                    if (detection.IsFault)
                    {
                        Teams otherTeam = (Teams)(Enum.GetValues(typeof(Teams)).GetValue(((int)_lastPlayerToApplyForce.PlayerTeam + 1) % Enum.GetValues(typeof(Teams)).Length));
                        ScoreManager.Instance.AddPoint(otherTeam);
                        /*StartCoroutine(ReinitializeBall());*/
                        ResetBallFunction();
                    }

                    else
                    {
                        _reboundsCount++;
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<NetDetection>())
            _isOnOtherSide = true;
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

    public void InitializeActionParameters(ShotParameters actionParameters)
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

    public void ResetBallFunction()
    {
        _reboundsCount = 0;
        GameManager.Instance.EndOfPoint();
        _lastPlayerToApplyForce = null;
        _rigidBody.velocity = Vector3.zero;
    }

    public IEnumerator ReinitializeBall()
    {
        enabled = false;
        _reboundsCount = 0;
        GameManager.Instance.EndOfPoint();

        yield return new WaitForSeconds(.2f);

        _rigidBody.velocity = Vector3.zero;
        /*gameObject.SetActive(false);*/
        _lastPlayerToApplyForce = null;
        enabled = true;
    }
}
