using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllersParent : MonoBehaviour
{
    #region PRIVATE FIELDS

    [SerializeField] protected bool _isServing;
    [SerializeField] protected PlayerStates _currentState;
    [SerializeField] protected Teams _playerTeam;
    [SerializeField] protected BallServiceDetection _ballServiceDetectionArea;

    #endregion

    #region GETTERS & SETTERS

    public bool IsServing { set { _isServing = value; } }
    public PlayerStates CurrentState { set { _currentState = value; } }
    public Teams PlayerTeam { get { return _playerTeam; } }

    #endregion

    public void ResetAtService()
    {
        _currentState = PlayerStates.IDLE;

        if (_ballServiceDetectionArea != null)
            _ballServiceDetectionArea.gameObject.SetActive(true);
    }
}
