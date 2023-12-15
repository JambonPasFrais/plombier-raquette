using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BotBehavior : ControllersParent
{
    #region PRIVATE FIELDS

    [Header("Ball Physical Behavior Parameters")]
    [SerializeField] private List<NamedActions> _possibleActions;
    [SerializeField] private List<NamedPhysicMaterials> _possiblePhysicMaterials;

    [Header("Instances")]
    [SerializeField] private Transform[] _targets;
    [SerializeField] private Transform[] _firstSideTargetsPositions;
    [SerializeField] private Transform[] _secondSideTargetsPositions;
    [SerializeField] private BallDetection _ballDetectionArea;
    
    [Header("GD")]
    [SerializeField] private float _speed;

    [Header("Service Parameters")]
    [SerializeField] private float _timeBeforeThrowingBallDuringService;
    [SerializeField] private float _timeBeforeShootingBallDuringService;
    [SerializeField] private float _serviceForce;

    private Ball _ballInstance;
    private Vector3 _targetPosVector3;
    private Dictionary<string, Transform[]> _targetPositionsBySide;
    private Vector3 _serviceDirection;
    private Coroutine _botServiceCoroutine;

    #endregion

    public Vector3 TargetPosVector3 { set {  _targetPosVector3 = value; } }

    #region UNITY METHODS

    private void Awake()
    {
        _targetPositionsBySide = new Dictionary<string, Transform[]>()
        {
            { FieldSide.FIRSTSIDE.ToString(), _firstSideTargetsPositions },
            { FieldSide.SECONDSIDE.ToString(), _secondSideTargetsPositions }
        };
    }

    private void Start()
    {
        ServicesCount = 0;
        _targetPosVector3 = transform.position;
        _ballInstance = GameManager.Instance.BallInstance.GetComponent<Ball>();
    }

    private void Update()
    {
        if (GameManager.Instance.GameState != GameState.ENDPOINT && GameManager.Instance.GameState != GameState.ENDMATCH)
        {
            if (PlayerState != PlayerStates.SERVE)
            {
                MoveTowardsBallX();
            }

            if (_ballDetectionArea.IsBallInHitZone && _ballDetectionArea.Ball.LastPlayerToApplyForce != this && PlayerState != PlayerStates.SERVE)
            {
                HitBall();
            }
            else if (_ballDetectionArea.IsBallInHitZone && _ballDetectionArea.Ball.LastPlayerToApplyForce != this && _botServiceCoroutine == null)
            {
                _botServiceCoroutine = StartCoroutine(BotService());
            }
        }
    }

    #endregion

    #region MOVEMENT AND HITTING METHODS

    private Vector3 GetServiceTarget()
    {
        float maximumDistance = 0f;
        Transform correctTarget = null;
        foreach (Transform target in _targets)
        {
            float currentDistance = Vector3.Distance(transform.position, target.position);
            if (currentDistance > maximumDistance)
            {
                maximumDistance = currentDistance;
                correctTarget = target;
            }
        }

        return correctTarget.position;
    }

    private IEnumerator BotService()
    {
        yield return new WaitForSeconds(_timeBeforeThrowingBallDuringService);

        ThrowBall();

        yield return new WaitForSeconds(_timeBeforeShootingBallDuringService);

        Vector3 targetPosition = GetServiceTarget();
        Vector3 serviceDirection = targetPosition - transform.position;
        Vector3 horizontalServiceDirection = Vector3.Project(serviceDirection, Vector3.forward) + Vector3.Project(serviceDirection, Vector3.right);
        _serviceDirection = horizontalServiceDirection;
        HitBall();
        _botServiceCoroutine = null;
    }

    private void HitBall()
    {
        float force;
        Vector3 direction;

        if (PlayerState == PlayerStates.SERVE)
        {
            GameManager.Instance.DesactivateAllServiceDetectionVolumes();
            GameManager.Instance.ServiceManager.DisableLockServiceColliders();

            direction = _serviceDirection;
            force = _serviceForce;

            PlayerState = PlayerStates.PLAY;
        }
        else
        {
            Vector3 targetPoint = _targets[Random.Range(0, _targets.Length)].position;
            direction = Vector3.Project(targetPoint - _ballInstance.gameObject.transform.position, Vector3.forward) + Vector3.Project(targetPoint - _ballInstance.gameObject.transform.position, Vector3.right);
            force = Random.Range(_minimumShotForce, _maximumShotForce);

            if (PlayerState != PlayerStates.PLAY)
            {
                PlayerState = PlayerStates.PLAY;
            }
        }

        if (_ballDetectionArea.Ball.LastPlayerToApplyForce != null && GameManager.Instance.GameState == GameState.SERVICE)
            GameManager.Instance.GameState = GameState.PLAYING;

        _ballInstance.InitializePhysicsMaterial(NamedPhysicMaterials.GetPhysicMaterialByName(_possiblePhysicMaterials, "Normal"));
        _ballInstance.InitializeActionParameters(NamedActions.GetActionParametersByName(_possibleActions, HitType.Flat.ToString()));
        _ballInstance.ApplyForce(force, _ballDetectionArea.GetRisingForceFactor(HitType.Flat), direction.normalized, this);
    }

    private void MoveTowardsBallX()
    {
        _targetPosVector3.x = _ballInstance.gameObject.transform.position.x;
        transform.position = Vector3.MoveTowards(transform.position, _targetPosVector3, _speed * Time.deltaTime);
    }

    public void SetTargetsSide(string sideName)
    {
        for(int i = 0; i < _targets.Length; i++)
        {
            Transform target = _targets[i];
            target.position = _targetPositionsBySide[sideName][i].position;
        }
    }
    
    #endregion
}

