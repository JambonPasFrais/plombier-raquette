using ExitGames.Client.Photon.StructWrapping;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
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
    public GameObject BallPrefab;

    [HideInInspector] public bool ServiceOnOriginalSide;

    #endregion

    #region PRIVATE FIELDS

    [Header("Environment Objects")]
    [SerializeField] private GameObject _net;
    [SerializeField] private List<ControllersParent> _controllers;
    [SerializeField] private FieldBorderPointsContainer[] _borderPointsContainers;

    private Dictionary<ControllersParent, Teams> _teamControllersAssociated;
    private Dictionary<Teams, FieldBorderPointsContainer> _fieldBorderPointsByTeam;

    private GameObject _ballInstance;
    private int _serverIndex;
    private Transform _serviceBallInitializationPoint;

    #endregion

    #region GETTERS

    public GameObject BallInstance {  get { return _ballInstance; } }
    public GameObject Net {  get { return _net; } }
    public List<ControllersParent> Controllers {  get { return _controllers; } }
    public Transform ServiceBallInitializationPoint {  get { return _serviceBallInitializationPoint; } }
    public FieldBorderPointsContainer[] BorderPointsContainers {  get { return _borderPointsContainers; } }
    public int ServerIndex { get { return _serverIndex; } }

    #endregion

    #region UNITY METHODS

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        _ballInstance = Instantiate(BallPrefab);
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
    }

    #endregion

    private void InitializeGameVariables()
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
        else if(movementDirection == -currentForwardVector)
        {
            return Mathf.Abs(borderPointsContainer.BackPointTransform.position.z - playerPosition.z);
        }
        else if(movementDirection == currentRightVector)
        {
            return Mathf.Abs(borderPointsContainer.RightPointTransform.position.x - playerPosition.x);
        }
        else if(movementDirection == -currentRightVector)
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
        StartCoroutine(RestartGame());
    }

    private IEnumerator RestartGame()
    {
        yield return new WaitForSeconds(2f);

        foreach (var player in _controllers)
        {
            player.ResetAtService();
        }

        InitializeGameVariables();

        GameManager.Instance.ScoreManager.ResetScore();
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

    public void ServiceThrow(InputAction.CallbackContext context)
    {
        if(_controllers[_serverIndex] is AgentController)
        {
            AgentController agent = (AgentController)_controllers[_serverIndex];
            agent.ActionIndex = 1;
        }
        else
        {
            ThrowBall();
        }
    }

    public void ThrowBall()
    {
        Rigidbody ballRigidBody = _ballInstance.GetComponent<Rigidbody>();

        if (_controllers[_serverIndex].PlayerState == PlayerStates.SERVE && _controllers[_serverIndex].IsServing && GameState == GameState.SERVICE && ballRigidBody.isKinematic)
        {
            ballRigidBody.isKinematic = false;
            ballRigidBody.AddForce(Vector3.up * _controllers[_serverIndex].ActionParameters.ServiceThrowForce);
        }
    }

    public void DesactivateAllServiceDetectionVolumes()
    {
        foreach(ControllersParent controller in _controllers)
        {
            controller.BallServiceDetectionArea.gameObject.SetActive(false);
        }
    }
}
