//using Photon.Pun;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using Unity.VisualScripting;
//using UnityEngine;
//using UnityEngine.InputSystem;
//using UnityEngine.Serialization;
//using Photon.Realtime;

//public class OnlineGameManager : MonoBehaviourPunCallbacks
//{
//    #region PUBLIC FIELDS

//    public static OnlineGameManager Instance;

//    public GameState GameState;

//    [Header("Manager Instances")]
//    public SideManager SideManager;
//    public ServiceManager ServiceManager;
//    public ScoreManager ScoreManager;

//    [Header("Ball Management")]
//    public Transform BallInitializationTransform;
//    public GameObject BallPrefab;

//    [HideInInspector] public bool ServiceOnOriginalSide;

//    #endregion

//    #region PRIVATE FIELDS

//    [Header("Environment Objects")]
//    [SerializeField] private GameObject _net;
//    [SerializeField] private List<ControllersParent> _controllers;
//    [SerializeField] private FieldBorderPointsContainer[] _borderPointsContainers;
//    [SerializeField] private GameObject _playerPrefab;
//    [SerializeField] private GameObject[] _cameras;
//    [SerializeField] private Transform[] _spawnPointsTransforms;


//    private Dictionary<ControllersParent, Teams> _teamControllersAssociated;
//    private Dictionary<Teams, FieldBorderPointsContainer> _fieldBorderPointsByTeam;

//    private GameObject _ballInstance;
//    private int _serverIndex;

//    #endregion

//    #region GETTERS

//    public GameObject BallInstance { get { return _ballInstance; } }
//    public GameObject Net { get { return _net; } }
//    public List<ControllersParent> Controllers { get { return _controllers; } }

//    #endregion

//    #region UNITY METHODS

//    private void Awake()
//    {
//        if (Instance == null)
//        {
//            Instance = this;
//        }
//        InstantiatePlayer();
//        if (PhotonNetwork.IsMasterClient == false)
//        {
//            photonView.RPC("SendConnectedToMaster", RpcTarget.MasterClient);
//        }
//        _ballInstance = Instantiate(BallPrefab);
//    }
//    private void Start()
//    {
//        GameState = GameState.BEFOREGAME;
//    }
//    private void Update()
//    {
//        if (GameState == GameState.BEFOREGAME && _controllers.Count == PhotonNetwork.CurrentRoom.PlayerCount)
//        {
//            ServiceOnOriginalSide = true;

//            GameState = GameState.SERVICE;
//            foreach (ControllersParent controller in _controllers)
//            {
//                controller.PlayerState = PlayerStates.IDLE;
//            }

//            _serverIndex = 0;
//            _controllers[_serverIndex].IsServing = true;
//            GameManager.Instance.SideManager.SetSidesInSimpleMatch(_controllers, true, ServiceOnOriginalSide);
//            GameManager.Instance.ServiceManager.SetServiceBoxCollider(false);
//            _ballInstance.GetComponent<Ball>().ResetBall();

//            _teamControllersAssociated = new Dictionary<ControllersParent, Teams>();

//            int i = 0;
//            foreach (ControllersParent controller in _controllers)
//            {
//                Teams team = (Teams)Enum.GetValues(typeof(Teams)).GetValue(i);
//                _teamControllersAssociated.Add(controller, team);
//                i++;
//            }

//            _fieldBorderPointsByTeam = new Dictionary<Teams, FieldBorderPointsContainer>();

//            foreach (FieldBorderPointsContainer borderPointsContainer in _borderPointsContainers)
//            {
//                _fieldBorderPointsByTeam.Add(borderPointsContainer.Team, borderPointsContainer);
//            }
//        }
//    }

//    #endregion

//    /*    private Teams? GetOtherPlayerTeam(ControllersParent currentPlayer)
//        {
//            foreach (KeyValuePair<ControllersParent, Teams> kvp in _teamControllersAssociated) 
//            {
//                if (kvp.Key != currentPlayer)
//                {
//                    return kvp.Value;
//                }
//            }

//            return null;
//        }*/

//    public Teams? GetPlayerTeam(ControllersParent currentPlayer)
//    {
//        foreach (KeyValuePair<ControllersParent, Teams> kvp in _teamControllersAssociated)
//        {
//            if (kvp.Key == currentPlayer)
//            {
//                return kvp.Value;
//            }
//        }

//        return null;
//    }

//    /// <summary>
//    /// Updates the field points ownership when the players change sides.
//    /// </summary>
//    public void ChangeFieldBorderPointsOwnership()
//    {
//        foreach (FieldBorderPointsContainer borderPointsContainer in _borderPointsContainers)
//        {
//            if (borderPointsContainer.Team == _teamControllersAssociated[_controllers[0]])
//            {
//                borderPointsContainer.Team = _teamControllersAssociated[_controllers[1]];
//            }
//            else
//            {
//                borderPointsContainer.Team = _teamControllersAssociated[_controllers[0]];
//            }

//            _fieldBorderPointsByTeam[borderPointsContainer.Team] = borderPointsContainer;
//        }
//    }

//    /// <summary>
//    /// Calculates the distance between the player and the field limit point in the wanted direction.
//    /// </summary>
//    /// <param name="playerController"></param>
//    /// <param name="movementDirection"></param>
//    /// <returns></returns>
//    public float GetDistanceToBorderByDirection(ControllersParent playerController, Vector3 movementDirection, Vector3 currentForwardVector, Vector3 currentRightVector)
//    {
//        Teams playerTeam = (Teams)GetPlayerTeam(playerController);
//        Vector3 playerPosition = playerController.gameObject.transform.position;
//        FieldBorderPointsContainer borderPointsContainer = _fieldBorderPointsByTeam[playerTeam];

//        if (movementDirection == currentForwardVector)
//        {
//            return Mathf.Abs(borderPointsContainer.FrontPointTransform.position.z - playerPosition.z);
//        }
//        else if (movementDirection == -currentForwardVector)
//        {
//            return Mathf.Abs(borderPointsContainer.BackPointTransform.position.z - playerPosition.z);
//        }
//        else if (movementDirection == currentRightVector)
//        {
//            return Mathf.Abs(borderPointsContainer.RightPointTransform.position.x - playerPosition.x);
//        }
//        else if (movementDirection == -currentRightVector)
//        {
//            return Mathf.Abs(borderPointsContainer.LeftPointTransform.position.x - playerPosition.x);
//        }

//        return 0f;
//    }

//    public void EndOfPoint()
//    {
//        GameState = GameState.ENDPOINT;

//        foreach (var player in _controllers)
//        {
//            player.ResetAtService();
//        }
//    }

//    public void EndOfGame()
//    {
//        Debug.Log("End of game !");
//    }

//    public void ChangeServer()
//    {
//        int newServerIndex = (_serverIndex + 1) % _controllers.Count;
//        _controllers[_serverIndex].IsServing = false;
//        _controllers[newServerIndex].IsServing = true;
//        _serverIndex = newServerIndex;
//    }

//    public void BallServiceInitialization()
//    {
//        _controllers[_serverIndex].PlayerState = PlayerStates.SERVE;
//        _ballInstance.transform.position = BallInitializationTransform.position;
//    }

//    public void ServiceThrow(InputAction.CallbackContext context)
//    {
//        Rigidbody ballRigidBody = _ballInstance.GetComponent<Rigidbody>();

//        if (_controllers[_serverIndex].PlayerState == PlayerStates.SERVE && _controllers[_serverIndex].IsServing && GameState == GameState.SERVICE && ballRigidBody.isKinematic)
//        {
//            ballRigidBody.isKinematic = false;
//            ballRigidBody.AddForce(Vector3.up * _controllers[_serverIndex].ActionParameters.ServiceThrowForce);
//        }
//    }

//    public void DesactivateAllServiceDetectionVolumes()
//    {
//        foreach (ControllersParent controller in _controllers)
//        {
//            controller.BallServiceDetectionArea.gameObject.SetActive(false);
//        }
//    }

//    private void InstantiatePlayer()
//    {
//        GameObject go = PhotonNetwork.Instantiate(this._playerPrefab.name, new Vector3(0, 0, 0), Quaternion.identity);
//        _controllers.Add(go.GetComponent<PlayerController>());
//    }
//    private void FindController()
//    {
//        ControllersParent[] controllers = FindObjectsOfType<ControllersParent>();
//        foreach (ControllersParent controller in controllers)
//        {
//            Debug.Log(controller.gameObject.name);
//            if (!_controllers.Contains(controller))
//            {

//                _controllers.Add(controller);
//            }
//        }
//    }
//    [PunRPC]
//    private void SendConnectedToMaster()
//    {
//        FindController();
//    }
//}