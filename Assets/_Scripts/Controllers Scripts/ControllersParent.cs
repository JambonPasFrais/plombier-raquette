using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllersParent : MonoBehaviour
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

    [Header("Force Clamping")]
    [SerializeField] protected float _maximumDistanceToNet;
    [SerializeField] protected float _forceMinimumClampFactor;

    #endregion

    #region GETTERS

    public Teams PlayerTeam { get { return _playerTeam; } }
    public ActionParameters ActionParameters { get { return _actionParameters; } }
    public BallServiceDetection BallServiceDetectionArea { get { return _ballServiceDetectionArea; } }

    #endregion

    protected float CalculateActualForce(float hitForce)
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

        if (_ballServiceDetectionArea != null)
            _ballServiceDetectionArea.gameObject.SetActive(true);
    }
}
