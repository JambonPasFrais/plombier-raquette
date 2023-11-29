using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    #region PUBLIC FIELDS

    public static GameManager Instance;

    public GameState GameState;

    [Header("Manager Instances")]
    public SideManager SideManager;
    public ServiceManager ServiceManager;
    public ScoreManager ScoreManager;

    [Header("Ball Management")]
    public Transform BallInitializationTransform;
    public GameObject BallPrefab;

    [HideInInspector] public bool ServiceOnOriginalSide;
    [HideInInspector] public bool IsGameFinished;
    [HideInInspector] public bool ServeRight;

    #endregion

    #region PRIVATE FIELDS

    [Header("Environment Objects")]
    [SerializeField] private GameObject _net;
    [SerializeField] private List<ControllersParent> _controllers;
    [SerializeField] private FieldBorderPointsContainer[] _borderPointsContainers;

    private Dictionary<ControllersParent, Player> _playerControllersAssociated;
    private Dictionary<Player, int> _playersPoints;
    private Dictionary<Player, int> _playersGames;
    private Dictionary<string, FieldBorderPointsContainer> _fieldBorderPointsByPlayerName;

    private GameObject _ballInstance;
    private int _serverIndex;

    #endregion

    #region GETTERS

    public GameObject BallInstance {  get { return _ballInstance; } }
    public GameObject Net {  get { return _net; } }

    #endregion

    #region UNITY METHODS

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        _ballInstance = Instantiate(BallPrefab);
        _ballInstance.SetActive(false);
    }

    void Start()
    {
        ServiceOnOriginalSide = true;
        IsGameFinished = false;
        ServeRight = true;

        GameState = GameState.SERVICE;
        foreach(ControllersParent controller in _controllers)
        {
            controller.PlayerState = PlayerStates.IDLE;
        }

        _serverIndex = 0;
        _controllers[_serverIndex].IsServing = true;
        GameManager.Instance.SideManager.ChangeSidesInGameSimple(_controllers, ServeRight, ServiceOnOriginalSide);

        _playerControllersAssociated = new Dictionary<ControllersParent, Player>();
        _playersPoints = new Dictionary<Player, int>();
        _playersGames = new Dictionary<Player, int>();

        int i = 0;
        foreach (ControllersParent controller in _controllers)
        {
            Player newPlayer = new Player($"Player {i + 1}");
            _playerControllersAssociated.Add(controller, newPlayer);
            _playersPoints.Add(newPlayer, 0);
            _playersGames.Add(newPlayer, 0);
            i++;
        }

        _fieldBorderPointsByPlayerName = new Dictionary<string, FieldBorderPointsContainer>();

        foreach (FieldBorderPointsContainer borderPointsContainer in _borderPointsContainers)
        {
            _fieldBorderPointsByPlayerName.Add(borderPointsContainer.PlayerName, borderPointsContainer); 
        }
    }

    void Update()
    {
        // Ball instantiation.
        if (Input.GetKeyDown(KeyCode.C) && _controllers[_serverIndex].PlayerState == PlayerStates.IDLE && _controllers[_serverIndex].IsServing)
        {
            _ballInstance.GetComponent<Ball>().ResetBallFunction();

            _controllers[_serverIndex].PlayerState = PlayerStates.SERVE;
            GameState = GameState.SERVICE;
            _ballInstance.transform.position = BallInitializationTransform.position;
            _ballInstance.SetActive(true);
        }
    }

    #endregion

    private Player GetOtherPlayer(ControllersParent currentPlayer)
    {
        foreach (KeyValuePair<ControllersParent, Player> kvp in _playerControllersAssociated) 
        {
            if (kvp.Key != currentPlayer)
            {
                return kvp.Value;
            }
        }

        return null;
    }

    public string GetPlayerName(ControllersParent currentPlayer)
    {
        foreach (KeyValuePair<ControllersParent, Player> kvp in _playerControllersAssociated)
        {
            if (kvp.Key == currentPlayer)
            {
                return kvp.Value.Name;
            }
        }

        return null;
    }

    public float GetDistanceToBorderByDirection(ControllersParent playerController, Vector3 movementDirection)
    {
        string playerName = GetPlayerName(playerController);
        Vector3 playerPosition = playerController.gameObject.transform.position;
        FieldBorderPointsContainer borderPointsContainer = _fieldBorderPointsByPlayerName[playerName];

        if (movementDirection == Vector3.forward) 
        {
            return Mathf.Abs(borderPointsContainer.FrontPointTransform.position.z - playerPosition.z);
        }
        else if(movementDirection == -Vector3.forward)
        {
            return Mathf.Abs(borderPointsContainer.BackPointTransform.position.z - playerPosition.z);
        }
        else if(movementDirection == Vector3.right)
        {
            return Mathf.Abs(borderPointsContainer.RightPointTransform.position.x - playerPosition.x);
        }
        else if(movementDirection == -Vector3.right)
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
    }

    public void ChangeServer()
    {
        int newServer = (_serverIndex + 1) % _controllers.Count;
        _controllers[_serverIndex].IsServing = false;
        _controllers[newServer].IsServing = true;
        _serverIndex = newServer;
    }
}
