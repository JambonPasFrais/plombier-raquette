using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllersParent : MonoBehaviour
{
    #region PUBLIC FIELDS

    public bool IsServing;
    public PlayerStates PlayerState;

    #endregion

    #region PRIVATE FIELDS

    [SerializeField] protected ActionParameters _actionParameters;
    [SerializeField] protected Teams _playerTeam;
    [SerializeField] protected BallServiceDetection _ballServiceDetectionArea;

    #endregion

    #region GETTERS

    public Teams PlayerTeam { get { return _playerTeam; } }
    public ActionParameters ActionParameters { get { return _actionParameters; } }
    public BallServiceDetection BallServiceDetectionArea { get { return _ballServiceDetectionArea; } }

    #endregion

    public void ResetAtService()
    {
        PlayerState = PlayerStates.IDLE;

        if (_ballServiceDetectionArea != null)
            _ballServiceDetectionArea.gameObject.SetActive(true);
    }
}
