using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AgentTrainingManager : MonoBehaviour
{
    #region PUBLIC FIELDS

    public GameState GameState;

    [Header("Ball Management")]
    public GameObject BallPrefab;

    #endregion

    #region PRIVATE FIELDS

    [Header("Side Management")]
    [SerializeField] private Transform _servicePointsFirstSideParent;
    [SerializeField] private Transform _servicePointsSecondSideParent;
    private Dictionary<string, Transform> _servicePointsFirstSide = new Dictionary<string, Transform>();
    private Dictionary<string, Transform> _servicePointsSecondSide = new Dictionary<string, Transform>();

    [Header("Service Management")]
    [SerializeField] private List<GameObject> _lockServiceMovementColliders; 
    [SerializeField] private bool _serveRight = false;
    [SerializeField] private int _consecutiveServicesCount;
    private int _globalGamesCount;

    [Header("Environment Objects")]
    [SerializeField] private GameObject _net;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private List<ControllersParent> _controllers;
    [SerializeField] private FieldBorderPointsContainer[] _borderPointsContainers;
    [SerializeField] private float _leftFaultLineXFromFirstSide;

    private Dictionary<ControllersParent, Teams> _teamControllersAssociated;
    private Dictionary<Teams, FieldBorderPointsContainer> _fieldBorderPointsByTeam;
    private Dictionary<Teams, float[]> _faultLinesXByTeam;

    private GameObject _ballInstance;
    private int _serverIndex;
    private Transform _serviceBallInitializationPoint;
    private int _currentPointsCount;

    #endregion

    #region GETTERS

    public GameObject Net { get { return _net; } }
    public GameObject BallInstance { get { return _ballInstance; } }
    public Transform CameraTransform { get { return _cameraTransform; } }
    public List<ControllersParent> Controllers { get { return _controllers; } }
    public Transform ServiceBallInitializationPoint { get { return _serviceBallInitializationPoint; } }
    public FieldBorderPointsContainer[] BorderPointsContainers { get { return _borderPointsContainers; } }
    public int ServerIndex { get { return _serverIndex; } }
    public bool ServeRight { get { return _serveRight; } }
    public Dictionary<Teams, float[]> FaultLinesXByTeam { get { return _faultLinesXByTeam; } }

    #endregion

    #region UNITY METHODS

    private void Awake()
    {
        for (int i = 0; i < _servicePointsFirstSideParent.childCount; i++)
        {
            _servicePointsFirstSide.Add(_servicePointsFirstSideParent.GetChild(i).name, _servicePointsFirstSideParent.GetChild(i));
            _servicePointsSecondSide.Add(_servicePointsSecondSideParent.GetChild(i).name, _servicePointsSecondSideParent.GetChild(i));
        }

        _serveRight = true;
        _globalGamesCount = 0;
        _currentPointsCount = 0;

        _ballInstance = Instantiate(BallPrefab);
        _ballInstance.GetComponent<AIBall>().InitializeVariables(this);
    }

    void Start()
    {
        InitializeGameVariables();

        _teamControllersAssociated = new Dictionary<ControllersParent, Teams>();

        int i = 0;
        foreach (ControllersParent controller in _controllers)
        {
            Teams team = (Teams)Enum.GetValues(typeof(Teams)).GetValue(i);
            _teamControllersAssociated.Add(controller, team);
            i++;
        }

        _fieldBorderPointsByTeam = new Dictionary<Teams, FieldBorderPointsContainer>();
        foreach (FieldBorderPointsContainer borderPointsContainer in _borderPointsContainers)
        {
            _fieldBorderPointsByTeam.Add(borderPointsContainer.Team, borderPointsContainer);
        }

        _faultLinesXByTeam = new Dictionary<Teams, float[]>()
        {
            {Teams.TEAM1, new float[]{ _leftFaultLineXFromFirstSide, -_leftFaultLineXFromFirstSide } },
            {Teams.TEAM2, new float[]{ -_leftFaultLineXFromFirstSide, _leftFaultLineXFromFirstSide } }
        };
    }

    #endregion

    private void InitializeGameVariables()
    {
        GameState = GameState.SERVICE;

        foreach (ControllersParent controller in _controllers)
        {
            controller.PlayerState = PlayerStates.IDLE;
        }

        _serverIndex = 0;
        _controllers[_serverIndex].IsServing = true;

        PlacingPlayers();

        _ballInstance.GetComponent<AIBall>().ResetBall();
    }

    public void ChangeServingSide()
    {
        _serveRight = !_serveRight;
    }

    /// <summary>
    /// Alternates the player's fields and set the players, the cameras and the bot targets to the correct positions for a 1v1 match.
    /// </summary>
    /// <param name="players"></param>
    /// <param name="serveRight"></param>
    /// <param name="originalSides"></param>
    public void InitializePlayersPosition()
    {
        string side = _serveRight ? "Right" : "Left";

        _controllers[0].transform.position = _servicePointsFirstSide[side].position;
        _controllers[0].transform.rotation = _servicePointsFirstSide[side].rotation;
        _controllers[1].transform.position = _servicePointsSecondSide[side].position;
        _controllers[1].transform.rotation = _servicePointsSecondSide[side].rotation;
    }

    /// <summary>
    /// Activates the colliders of a specific side of the field.
    /// </summary>
    /// <param name="side"></param>
    public void EnableLockServiceColliders()
    {
        int sideIndex = (_globalGamesCount % 2);
        _lockServiceMovementColliders[sideIndex].SetActive(true);
    }

    public void PlacingPlayers()
    {
        InitializePlayersPosition();
        EnableLockServiceColliders();
    }

    public void DisableLockServiceColliders()
    {
        foreach (var item in _lockServiceMovementColliders)
            item.SetActive(false);
    }

    public Teams? GetPlayerTeam(ControllersParent currentPlayer)
    {
        foreach (KeyValuePair<ControllersParent, Teams> kvp in _teamControllersAssociated)
        {
            if (kvp.Key == currentPlayer)
            {
                return kvp.Value;
            }
        }

        return null;
    }

    /// <summary>
    /// Calculates the distance between the player and the field limit point in the wanted direction.
    /// </summary>
    /// <param name="playerController"></param>
    /// <param name="movementDirection"></param>
    /// <returns></returns>
    public float GetDistanceToBorderByDirection(ControllersParent playerController, Vector3 movementDirection, Vector3 currentForwardVector, Vector3 currentRightVector)
    {
        Teams playerTeam = (Teams)GetPlayerTeam(playerController);
        Vector3 playerPosition = playerController.gameObject.transform.position;
        FieldBorderPointsContainer borderPointsContainer = _fieldBorderPointsByTeam[playerTeam];

        if (movementDirection == currentForwardVector)
        {
            return Mathf.Abs(borderPointsContainer.FrontPointTransform.position.z - playerPosition.z);
        }
        else if (movementDirection == -currentForwardVector)
        {
            return Mathf.Abs(borderPointsContainer.BackPointTransform.position.z - playerPosition.z);
        }
        else if (movementDirection == currentRightVector)
        {
            return Mathf.Abs(borderPointsContainer.RightPointTransform.position.x - playerPosition.x);
        }
        else if (movementDirection == -currentRightVector)
        {
            return Mathf.Abs(borderPointsContainer.LeftPointTransform.position.x - playerPosition.x);
        }

        return 0f;
    }

    public void EndOfPoint()
    {
        GameState = GameState.ENDPOINT;

        foreach (var player in _controllers)
        {
            player.ResetAtService();
        }

        _currentPointsCount++;
        NewGameVerification();
    }

    private void NewGameVerification()
    {
        if (_currentPointsCount == 5)
        {
            _currentPointsCount = 0;
            _globalGamesCount++;
            ChangeServer();
        }

        ChangeServingSide();
        PlacingPlayers();
    }

    public void ChangeServer()
    {
        int newServerIndex = (_serverIndex + 1) % _controllers.Count;
        _controllers[_serverIndex].IsServing = false;
        _controllers[newServerIndex].IsServing = true;
        _serverIndex = newServerIndex;
    }

    public void BallServiceInitialization()
    {
        _controllers[_serverIndex].PlayerState = PlayerStates.SERVE;
        _serviceBallInitializationPoint = _controllers[_serverIndex].ServiceBallInitializationPoint;
        _ballInstance.transform.position = _serviceBallInitializationPoint.position;
    }

    public void DesactivateAllServiceDetectionVolumes()
    {
        foreach (ControllersParent controller in _controllers)
        {
            controller.BallServiceDetectionArea.gameObject.SetActive(false);
        }
    }
}
