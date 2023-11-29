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
        /*if (GameManager.Instance.CurrentState == GameState.PLAYING)
        {
            // If the ball enters in collision with a part of the court ground, its number of rebounds is iterated.
            if(collision.gameObject.TryGetComponent<CourtDetections>(out CourtDetections courtGroundPart))
            {
                Rebound();
                Debug.Log("Court part detected, rebound realised");
            }

            // If the ball enters in collision with anything on the hitting player's side of the field excepting the net on the first rebound, it is a fault.
            if (!_isOnOtherSide && _reboundsCount < 2 && !collision.gameObject.TryGetComponent<Net>(out Net net))
            {
                Teams otherTeam = (Teams)(Enum.GetValues(typeof(Teams)).GetValue(((int)_lastPlayerToApplyForce.PlayerTeam + 1) % Enum.GetValues(typeof(Teams)).Length));
                GameManager.Instance.ScoreManager.AddPoint(otherTeam);
                ResetBallFunction();
                Debug.Log($"Collision with {collision.gameObject.name} on the hitting player side on the first rebound");
            }
            // If the ball enters in collision with the other player than the hitting player, it counts for a point for the hitting player.
            else if (collision.gameObject.GetComponent<PlayerController>() && _lastPlayerToApplyForce != collision.gameObject.GetComponent<ControllersParent>())
            {
                GameManager.Instance.ScoreManager.AddPoint(_lastPlayerToApplyForce.PlayerTeam);
                ResetBallFunction();
                Debug.Log("Collision with the other player");
            }
            else if (collision.gameObject.TryGetComponent<CourtDetections>(out CourtDetections detection)) 
            {
                if (_reboundsCount == 1)
                {
                    Vector3 ballToNetVector = GameManager.Instance.Net.transform.position - collision.contacts[0].point;
                    Vector3 horizontalBallToNetVector = Vector3.Project(ballToNetVector, Vector3.right) + Vector3.Project(ballToNetVector, Vector3.forward);
                    Vector3 horizontalCameraForwardVector = Vector3.Project(GameManager.Instance.SideManager.ActiveCameraTransform.forward, Vector3.forward);
                    bool isReboundOnHittingPlayerSide = Vector3.Dot(horizontalBallToNetVector, horizontalCameraForwardVector) > 0;

                    // If the ball enters in collision with a forbidden part of the other side of the field or on the hitting player field after rebounding on the net on the first rebound, then it is fault.
                    if (detection.IsFault || isReboundOnHittingPlayerSide)
                    {
                        Teams otherTeam = (Teams)(Enum.GetValues(typeof(Teams)).GetValue(((int)_lastPlayerToApplyForce.PlayerTeam + 1) % Enum.GetValues(typeof(Teams)).Length));
                        GameManager.Instance.ScoreManager.AddPoint(otherTeam);
                        ResetBallFunction();
                        Debug.Log("Collision with a fault part of the ground and/or on the hitting player side on the first rebound");
                    }
                }
                // If the ball enters in collision with the ground a second time, it is point for the hitting player.
                else if (_reboundsCount == 2)
                {
                    GameManager.Instance.ScoreManager.AddPoint(_lastPlayerToApplyForce.PlayerTeam);
                    ResetBallFunction();
                    Debug.Log("Collision with the ground for the second time");
                }
            }
        }*/
    }

    private void OnTriggerEnter(Collider other)
    {
        // If the ball enters in the net collider volume, then the ball passed on the other side.
        if (other.gameObject.GetComponent<NetDetection>())
            _isOnOtherSide = true;

        // If the ball enters the ball service detection during the service, then the player didn't serve yet and just threw the ball in the air.
        if (GameManager.Instance.GameState == GameState.SERVICE && other.gameObject.TryGetComponent<BallServiceDetection>(out BallServiceDetection ballServiceDetection))
        {
            _rigidBody.velocity = Vector3.zero;
            ballServiceDetection.Player.PlayerState = PlayerStates.IDLE;
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
        _isOnOtherSide = false;
        GameManager.Instance.EndOfPoint();
        _lastPlayerToApplyForce = null;
        _rigidBody.velocity = Vector3.zero;
    }

/*    public IEnumerator ReinitializeBall()
    {
        enabled = false;
        _reboundsCount = 0;
        GameManager.Instance.EndOfPoint();

        yield return new WaitForSeconds(.2f);

        _rigidBody.velocity = Vector3.zero;
        gameObject.SetActive(false);
        _lastPlayerToApplyForce = null;
        enabled = true;
    }*/
}
