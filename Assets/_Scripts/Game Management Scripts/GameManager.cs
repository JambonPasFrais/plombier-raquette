using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
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

    [Header("Ball Management")]
    public Transform BallInstantiationtransform;
    public GameObject BallPrefab;
    public GameObject OnlineBallPrefab;

    [HideInInspector] public bool ServiceOnOriginalSide;

    #endregion

    #region PRIVATE FIELDS

    [Header("Environment Objects")]
    [SerializeField] private GameObject _net;
    [SerializeField] private List<ControllersParent> _controllers;
    [SerializeField] private FieldBorderPointsContainer[] _borderPointsContainers;
    [SerializeField] private GameObject _playerPrefab;

    private Dictionary<ControllersParent, Teams> _teamControllersAssociated;
    private Dictionary<Teams, FieldBorderPointsContainer> _fieldBorderPointsByTeam;

    [SerializeField] private GameObject _ballInstance;
    private int _serverIndex;

    #endregion

    #region GETTERS

    public GameObject BallInstance { get { return _ballInstance; } }
    public GameObject Net { get { return _net; } }
    public List<ControllersParent> Controllers { get { return _controllers; } }

    public int ServerIndex { get => _serverIndex;}

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
            if (PhotonNetwork.IsMasterClient)
                _ballInstance = PhotonNetwork.Instantiate(OnlineBallPrefab.name, new Vector3(0, 256, 0), Quaternion.identity);
        }
        else
        {
            _ballInstance = Instantiate(BallPrefab);
        }
    }

    void Start()
    {


        if (PhotonNetwork.IsConnected == false)
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
        Debug.Log("End of game !");
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
        _ballInstance.transform.position = BallInstantiationtransform.position;
    }

    public void ServiceThrow(InputAction.CallbackContext context)
    {
        Rigidbody ballRigidBody = _ballInstance.GetComponent<Rigidbody>();

        if (_controllers[_serverIndex].PlayerState == PlayerStates.SERVE && _controllers[_serverIndex].IsServing && GameState == GameState.SERVICE && ballRigidBody.isKinematic)
        {
            ballRigidBody.isKinematic = false;
            ballRigidBody.AddForce(Vector3.up * _controllers[_serverIndex].ActionParameters.ServiceThrowForce);
        }
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
        GameObject go = PhotonNetwork.Instantiate(this._playerPrefab.name, new Vector3(0, 0, 0), Quaternion.identity);
        _controllers.Add(go.GetComponent<PlayerController>());
    }

    private void FindController()
    {
        ControllersParent[] controllers = FindObjectsOfType<ControllersParent>();
        foreach (ControllersParent controller in controllers)
        {
            if (!_controllers.Contains(controller))
            {
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
        BallInstantiationtransform = _controllers[_serverIndex].GetComponentInChildren<BallInitialisationPoint>().gameObject.transform;
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
    }
    [PunRPC]
    private void ShootOnline(string hitType, int index)
    {
        BallInstance.GetComponent<Ball>().InitializeActionParameters(NamedActions.GetActionParametersByName(_controllers[0].GetComponent<PlayerController>().PossibleActions, hitType));
        BallInstance.GetComponent<Ball>().InitializeLastPlayerToApplyForce(_controllers[index]);
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
