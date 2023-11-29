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

    [SerializeField] protected Teams _playerTeam;
    [SerializeField] protected BallServiceDetection _ballServiceDetectionArea;

    #endregion

    #region GETTERS & SETTERS

    public Teams PlayerTeam { get { return _playerTeam; } }

    #endregion

    public void ResetAtService()
    {
        PlayerState = PlayerStates.IDLE;

        if (_ballServiceDetectionArea != null)
            _ballServiceDetectionArea.gameObject.SetActive(true);
    }
}
