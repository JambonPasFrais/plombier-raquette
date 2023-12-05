using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BotBehavior : ControllersParent
{
    #region Private Fields

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
    [SerializeField] private float _minimumHitForce;
    [SerializeField] private float _maximumHitForce;

    private Ball _ballInstance;
    private Vector3 _targetPosVector3;
    private Dictionary<string, Transform[]> _targetPositionsBySide;

    #endregion

    public Vector3 TargetPosVector3 { set {  _targetPosVector3 = value; } }

    #region Unity Methods

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
            MoveTowardsBallX();

            if (_ballDetectionArea.IsBallInHitZone && _ballDetectionArea.Ball.LastPlayerToApplyForce != this)
            {
                HitBall();
            }
        }
    }

    #endregion
    
    #region Personalised Methods

    private void HitBall()
    {
        Vector3 targetPoint = _targets[Random.Range(0, _targets.Length)].position;
        Vector3 direction = Vector3.Project(targetPoint - _ballInstance.gameObject.transform.position, Vector3.forward) + Vector3.Project(targetPoint - _ballInstance.gameObject.transform.position, Vector3.right);

        if (PlayerState != PlayerStates.PLAY)
        {
            if (PlayerState == PlayerStates.SERVE)
            {
                GameManager.Instance.DesactivateAllServiceDetectionVolumes();
                GameManager.Instance.ServiceManager.DisableLockServiceColliders();
            }

            PlayerState = PlayerStates.PLAY;
        }

        if (_ballDetectionArea.Ball.LastPlayerToApplyForce != null && GameManager.Instance.GameState == GameState.SERVICE)
            GameManager.Instance.GameState = GameState.PLAYING;

        _ballInstance.InitializePhysicsMaterial(NamedPhysicMaterials.GetPhysicMaterialByName(_possiblePhysicMaterials, "Normal"));
        _ballInstance.InitializeActionParameters(NamedActions.GetActionParametersByName(_possibleActions, HitType.Flat.ToString()));
        _ballInstance.ApplyForce(Random.Range(_minimumHitForce, _maximumHitForce), _ballDetectionArea.GetRisingForceFactor(), direction.normalized, this);
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

