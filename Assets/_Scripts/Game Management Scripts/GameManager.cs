using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    #region PUBLIC FIELDS

    public static GameManager Instance;

    public Transform BallInitializationTransform;
    public GameObject BallInstance;

    public GameState CurrentState;

    [HideInInspector] public bool HasChangeSidesSinceBeginning;
    [HideInInspector] public bool IsGameFinished;
    [HideInInspector] public bool ServeRight;

    #endregion

    #region PRIVATE FIELDS

    [SerializeField] private List<ControllersParent> _controllers;
    [SerializeField] private PlayerField[] _playerFields;

    private Dictionary<ControllersParent, Player> _playerControllersAssociated;
    private Dictionary<Player, int> _playersPoints;
    private Dictionary<Player, int> _playersGames;
    private Dictionary<string, PlayerField> _playerFieldsByPlayerName;

    private int _serverIndex;

    #endregion

    #region UNITY METHODS

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        HasChangeSidesSinceBeginning = false;
        IsGameFinished = false;
        ServeRight = true;

        _serverIndex = 0;

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

        _playerFieldsByPlayerName = new Dictionary<string, PlayerField>();

        foreach (PlayerField playerField in _playerFields)
        {
            _playerFieldsByPlayerName.Add(playerField.PlayerName, playerField);
        }
    }

    void Update()
    {
        // Ball instantiation.
        if (Input.GetKeyDown(KeyCode.C))
        {
            BallInstance.GetComponent<Ball>().ResetBallFunction();

            BallInstance.transform.position = BallInitializationTransform.position;
            BallInstance.SetActive(true);
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
        PlayerField playerField = _playerFieldsByPlayerName[playerName];

        if (movementDirection == Vector3.forward) 
        {
            return Mathf.Abs(playerField.FrontPointTransform.position.z - playerPosition.z);
        }
        else if(movementDirection == -Vector3.forward)
        {
            return Mathf.Abs(playerField.BackPointTransform.position.z - playerPosition.z);
        }
        else if(movementDirection == Vector3.right)
        {
            return Mathf.Abs(playerField.RightPointTransform.position.x - playerPosition.x);
        }
        else if(movementDirection == -Vector3.right)
        {
            return Mathf.Abs(playerField.LeftPointTransform.position.x - playerPosition.x);
        }

        return 0f;
    }

    public void EndOfPoint()
    {
        CurrentState = GameState.ENDPOINT;

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
