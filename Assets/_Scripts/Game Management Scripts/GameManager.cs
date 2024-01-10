using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Photon.Realtime;
using System.Linq;
using System.Globalization;

public class GameManager : MonoBehaviourPunCallbacks
{
    #region PUBLIC FIELDS

    public static GameManager Instance;

    public GameState GameState;

    [Header("Manager Instances")]
    public SideManager SideManager;
    public ServiceManager ServiceManager;
    public ScoreManager ScoreManager;
    public CameraManager CameraManager;

    [Header("Ball Management")]
    public GameObject BallPrefab;
    public GameObject OnlineBallPrefab;

    [SerializeField] private GameObject _smashTargetGo;

    [HideInInspector] public bool ServiceOnOriginalSide;

    #endregion

    #region PRIVATE FIELDS

    [Header("Environment Objects")]
    [SerializeField] private GameObject _net;
    [SerializeField] private List<ControllersParent> _controllers;
    [SerializeField] private FieldBorderPointsContainer[] _borderPointsContainers;
    [SerializeField] private float _leftFaultLineXFromFirstSide;
    [SerializeField] private GameObject _playerPrefab;

    [Header("Other Objects")]
    // Temporary variable.
    [SerializeField] private CharacterParameters _classicCharacterParameters;

    private Dictionary<ControllersParent, Teams> _teamControllersAssociated;
    private Dictionary<Teams, FieldBorderPointsContainer> _fieldBorderPointsByTeam;
    private Dictionary<Teams, float[]> _faultLinesXByTeam;

    private GameObject _ballInstance;
    private int _serverIndex;
    private Transform _serviceBallInitializationPoint;

    #endregion

    #region GETTERS

    public GameObject BallInstance { get { return _ballInstance; } }
    public GameObject Net { get { return _net; } }
    public List<ControllersParent> Controllers { get { return _controllers; } }
    public int ServerIndex { get { return _serverIndex; } }
    public Transform ServiceBallInitializationPoint { get { return _serviceBallInitializationPoint; } }
    public Dictionary<Teams, float[]> FaultLinesXByTeam { get { return _faultLinesXByTeam; } }

    public GameObject SmashTargetGo => _smashTargetGo;

    #endregion

    #region SETTERS

    public void AddControllers(ControllersParent controller)
    {
        _controllers.Add(controller);
    }

    #endregion

    #region UNITY METHODS

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        if (PhotonNetwork.IsConnected)
        {
            InstantiatePlayer();

            if (PhotonNetwork.IsMasterClient == false)
            {
                photonView.RPC("AskFindController", RpcTarget.MasterClient);
            }
        }

        _ballInstance = Instantiate(BallPrefab);
    }

    public void Init()
    {
        ServiceOnOriginalSide = true;

        GameState = GameState.SERVICE;
        foreach (ControllersParent controller in _controllers)
        {
            ServiceOnOriginalSide = true;

            _serverIndex = 0;
            _controllers[_serverIndex].IsServing = true;

            // Double
            if (_controllers.Count > 2)
            {
                CameraManager.InitSplitScreenCameras();
            }
            else if (GameParameters.Instance.LocalNbPlayers == _controllers.Count) // Simple with 2 locals
            {
                CameraManager.InitSplitScreenCameras();
            }
            else // Simple vs bot
            {
                CameraManager.InitSoloCamera(); 
            }

            SideManager.SetSides(_controllers, true, ServiceOnOriginalSide);

            ServiceManager.SetServiceBoxCollider(false);

            ScoreManager.InitGameLoop(GameParameters.Instance.CurrentGameMode.NbOfSets, GameParameters.Instance.CurrentGameMode.NbOfGames, false); //TODO : can't play just a tiebreak ?

            _ballInstance.GetComponent<Ball>().ResetBall();

            _serverIndex = 0;
            _controllers[_serverIndex].IsServing = true;
            GameManager.Instance.SideManager.SetSidesInSimpleMatch(_controllers, true, ServiceOnOriginalSide);
            GameManager.Instance.ServiceManager.SetServiceBoxCollider(false);
            _ballInstance.GetComponent<Ball>().ResetBall();
            _teamControllersAssociated = new Dictionary<ControllersParent, Teams>();

            int i = 0;

            //foreach (ControllersParent controller in _controllers)
            //{
                Teams team = (Teams)Enum.GetValues(typeof(Teams)).GetValue(i);
                _teamControllersAssociated.Add(controller, team);
                i++;

                if (i > 1)
                {
                    i %= 2;
                }
            //}

            _fieldBorderPointsByTeam = new Dictionary<Teams, FieldBorderPointsContainer>();

            foreach (FieldBorderPointsContainer borderPointsContainer in _borderPointsContainers)
            {
                _fieldBorderPointsByTeam.Add(borderPointsContainer.Team, borderPointsContainer);
            }
        }

        _faultLinesXByTeam = new Dictionary<Teams, float[]>()
        {
            {Teams.TEAM1, new float[]{ _leftFaultLineXFromFirstSide, -_leftFaultLineXFromFirstSide } },
            {Teams.TEAM2, new float[]{ -_leftFaultLineXFromFirstSide, _leftFaultLineXFromFirstSide } }
        };
    }

    private void Update()
    {
        if (PhotonNetwork.IsConnected && GameState == GameState.BEFOREGAME && _controllers.Count == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            ServiceOnOriginalSide = true;

            GameState = GameState.SERVICE;
            foreach (ControllersParent controller in _controllers)
            {
                controller.PlayerState = PlayerStates.IDLE;
            }

            _serverIndex = 0;
            _controllers[_serverIndex].IsServing = true;
            GameManager.Instance.SideManager.SetSidesInSimpleMatch(_controllers, true, ServiceOnOriginalSide);
            GameManager.Instance.ServiceManager.SetServiceBoxCollider(false);
            _ballInstance.GetComponent<Ball>().ResetBall();
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
        }
    }

    #endregion

    /*    private Teams? GetOtherPlayerTeam(ControllersParent currentPlayer)
        {
            foreach (KeyValuePair<ControllersParent, Teams> kvp in _teamControllersAssociated) 
            {
                if (kvp.Key != currentPlayer)
                {
                    return kvp.Value;
                }
            }

            return null;
        }*/

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
    /// Updates the field points ownership when the players change sides.
    /// </summary>
    public void ChangeFieldBorderPointsOwnership()
    {
        foreach (FieldBorderPointsContainer borderPointsContainer in _borderPointsContainers)
        {
            if (borderPointsContainer.Team == _teamControllersAssociated[_controllers[0]])
            {
                borderPointsContainer.Team = _teamControllersAssociated[_controllers[1]];
            }
            else
            {
                borderPointsContainer.Team = _teamControllersAssociated[_controllers[0]];
            }

            _fieldBorderPointsByTeam[borderPointsContainer.Team] = borderPointsContainer;
        }
    }

    public void ChangeFaultLinesXByTeamValues()
    {
        for (int i = 0; i < _faultLinesXByTeam.Count; i++)
        {
            Teams team = _faultLinesXByTeam.Keys.ToList()[i];
            float[] formerFaultLinesX = _faultLinesXByTeam[team];
            _faultLinesXByTeam[team] = new float[] { -formerFaultLinesX[0], -formerFaultLinesX[1] };
        }
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
    }

    public void EndOfGame()
    {
        //TODO : uncomment when finished
        ControllerManager.Instance.ChangeCtrlersActMapToMenu();
        SceneManager.LoadScene("Clean_UI_Final");
        //Debug.Log("End of game !");
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

    public void DeactivateAllServiceDetectionVolumes()
    {
        foreach (ControllersParent controller in _controllers)
        {
            controller.BallServiceDetectionArea.gameObject.SetActive(false);
        }
    }

    private void InstantiatePlayer()
    {
        GameObject go = PhotonNetwork.Instantiate(_playerPrefab.name, new Vector3(0, 0, 0), Quaternion.identity);
        _controllers.Add(go.GetComponent<PlayerController>());
    }

    private void FindController()
    {
        ControllersParent[] controllers = FindObjectsOfType<ControllersParent>();
        foreach (ControllersParent controller in controllers)
        {
            if (!_controllers.Contains(controller))
            {
                controller.gameObject.GetComponent<PlayerInput>().enabled = false;
                _controllers.Add(controller);
            }
        }

        if (PhotonNetwork.IsMasterClient && _controllers.Count == PhotonNetwork.CurrentRoom.PlayerCount && GameState == GameState.BEFOREGAME)
        {
            photonView.RPC("AskFindController", RpcTarget.Others);
        }

        if (PhotonNetwork.IsMasterClient == false)
        {
            _ballInstance = FindObjectOfType<Ball>().gameObject;
            _controllers.Reverse();
            photonView.RPC("StartGame", RpcTarget.All);
        }
    }

    private void StartOnlineGame()
    {
        ServiceOnOriginalSide = true;

        GameState = GameState.SERVICE;
        foreach (ControllersParent controller in _controllers)
        {
            controller.PlayerState = PlayerStates.IDLE;
        }

        _serverIndex = 0;
        _controllers[_serverIndex].IsServing = true;
        _serviceBallInitializationPoint = _controllers[_serverIndex].ServiceBallInitializationPoint;
        GameManager.Instance.SideManager.SetSideOnline(true, ServiceOnOriginalSide);
        //GameManager.Instance.ServiceManager.SetServiceOnline(false);
        GameManager.Instance.ServiceManager.SetServiceBoxCollider(false);
        _teamControllersAssociated = new Dictionary<ControllersParent, Teams>();
        _ballInstance.GetComponent<Ball>().ResetBall();
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
    }

    [PunRPC]
    private void AskFindController()
    {
        FindController();
    }

    [PunRPC]
    private void StartGame()
    {
        StartOnlineGame();
    }

    [PunRPC]
    private void Served()
    {
        BallInstance.GetComponent<Rigidbody>().isKinematic = false;
        BallInstance.GetComponent<Rigidbody>().AddForce(Vector3.up * GameManager.Instance.Controllers[GameManager.Instance.ServerIndex].ActionParameters.ServiceThrowForce);
    }

    [PunRPC]
    private void ShootOnline(float force, string hitType, float risingForceFactor, Vector3 normalizedHorizontalDirection, int controllerIndex)
    {
        BallInstance.GetComponent<Ball>().InitializeActionParameters(NamedActions.GetActionParametersByName(_controllers[0].GetComponent<PlayerController>().PossibleActions, hitType));
        BallInstance.GetComponent<Ball>().InitializePhysicsMaterial(hitType == "Drop" ? NamedPhysicMaterials.GetPhysicMaterialByName(_controllers[0].GetComponent<PlayerController>().PossiblePhysicMaterials, "Drop") :
            NamedPhysicMaterials.GetPhysicMaterialByName(_controllers[0].GetComponent<PlayerController>().PossiblePhysicMaterials, "Normal"));
        BallInstance.GetComponent<Ball>().ApplyForce(force, risingForceFactor, normalizedHorizontalDirection, Controllers[controllerIndex]);
    }

    [PunRPC]
    private void EndPoint(bool fault)
    {
        Teams team = BallInstance.GetComponent<Ball>().LastPlayerToApplyForce.PlayerTeam;
        if (fault)
        {
            BallInstance.GetComponent<Ball>().LastPlayerToApplyForce.ServicesCount = 0;
            team = (Teams)(Enum.GetValues(typeof(Teams)).GetValue(((int)BallInstance.GetComponent<Ball>().LastPlayerToApplyForce.PlayerTeam + 1) % Enum.GetValues(typeof(Teams)).Length));
        }
        GameManager.Instance.EndOfPoint();
        GameManager.Instance.ScoreManager.AddPoint(team);
        BallInstance.GetComponent<Ball>().ResetBall();
    }
}