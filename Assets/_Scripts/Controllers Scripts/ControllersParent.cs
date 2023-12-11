using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class ControllersParent : Agent
{
    #region PUBLIC FIELDS

    public bool IsServing;
    public PlayerStates PlayerState;
    public int ServicesCount;

    #endregion

    #region PRIVATE FIELDS

    [SerializeField] protected ActionParameters _actionParameters;
    [SerializeField] protected Teams _playerTeam;
    [SerializeField] protected BallServiceDetection _ballServiceDetectionArea;
    [SerializeField] protected Transform _serviceBallInitializationPoint;

    [Header("Force Clamping")]
    [SerializeField] protected float _maximumDistanceToNet;
    [SerializeField] protected float _forceMinimumClampFactor;

    protected float _hitKeyPressedTime;
    protected bool _isCharging;

    #endregion

    #region GETTERS

    public Teams PlayerTeam { get { return _playerTeam; } }
    public ActionParameters ActionParameters { get { return _actionParameters; } }
    public BallServiceDetection BallServiceDetectionArea { get { return _ballServiceDetectionArea; } }
    public Transform ServiceBallInitializationPoint { get { return _serviceBallInitializationPoint; } }

    #endregion

    public float CalculateActualForce(float hitForce)
    {
        float actualDistanceToNet = Vector3.Project(GameManager.Instance.Net.transform.position - gameObject.transform.position, Vector3.forward).magnitude;
        float clampedDistanceToNet = Mathf.Clamp(actualDistanceToNet, 0f, _maximumDistanceToNet);
        float forceFactor = (clampedDistanceToNet / _maximumDistanceToNet) * (1 - _forceMinimumClampFactor) + _forceMinimumClampFactor;
        return forceFactor * hitForce;
    }

    public void ResetAtService()
    {
        PlayerState = PlayerStates.IDLE;
        ServicesCount = 0;
        ResetLoadedShotVariables();

        if (_ballServiceDetectionArea != null)
            _ballServiceDetectionArea.gameObject.SetActive(true);
    }

    public void ResetLoadedShotVariables()
    {
        _hitKeyPressedTime = 0;
        _isCharging = false;
    }
}
