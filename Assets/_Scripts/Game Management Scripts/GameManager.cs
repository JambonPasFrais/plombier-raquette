using Photon.Pun;
using Photon.Realtime;
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

    [HideInInspector] public bool ServiceOnOriginalSide;

    #endregion

    #region PRIVATE FIELDS

    [Header("Loading Screen")]
    [SerializeField] private GameObject _loadingScreen;
    [SerializeField] private TMPro.TextMeshProUGUI _animatedLoadingText;
    [SerializeField] private float _pointsAppearancePeriod;

    [Header("Smash Target")]
    [SerializeField] private GameObject _smashTargetGo;

    [Header("Environment Objects")]
    [SerializeField] private GameObject _net;
    [SerializeField] private List<ControllersParent> _controllers;
    [SerializeField] private FieldBorderPointsContainer[] _borderPointsContainers;
    [SerializeField] private float _leftFaultLineXFromFirstSide;
    [SerializeField] private SupporterManager _supporterManager;
    [SerializeField] private GameObject _onlinePlayerPrefab;

	[Header("Canvas References")]
    [SerializeField] private GameObject _inGameUI;
	[SerializeField] private GameObject _endGameUI;

	private Dictionary<ControllersParent, Teams> _teamControllersAssociated;
    private Dictionary<Teams, FieldBorderPointsContainer> _fieldBorderPointsByTeam;
    private Dictionary<Teams, float[]> _faultLinesXByTeam;

    /*[SerializeField] */private GameObject _ballInstance;
    private int _serverIndex;
    private Transform _serviceBallInitializationPoint;
    private System.Random random = new System.Random();
    private Coroutine _lastCoroutineStarted;

    private Coroutine _loadingScreenAnimatedTextCoroutine;

    #endregion

    #region GETTERS

    public GameObject BallInstance {  get { return _ballInstance; } }
    public GameObject Net {  get { return _net; } }
    public List<ControllersParent> Controllers {  get { return _controllers; } }
    public int ServerIndex { get { return _serverIndex; } }
    public Transform ServiceBallInitializationPoint { get { return _serviceBallInitializationPoint; } }
    public Dictionary<Teams, float[]> FaultLinesXByTeam { get { return _faultLinesXByTeam; } }
    public FieldBorderPointsContainer[] BorderPointsContainers { get { return _borderPointsContainers; } }

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

        GameState = GameState.BEFOREGAME;

        if (PhotonNetwork.IsConnected)
        {
            _loadingScreen.SetActive(true);
            _inGameUI.SetActive(false);
        }

        if (PhotonNetwork.IsConnected)
        {
            InstantiatePlayer(PhotonNetwork.IsMasterClient);

            if (PhotonNetwork.IsMasterClient)
            {
                _ballInstance = PhotonNetwork.Instantiate(OnlineBallPrefab.name, new Vector3(0, 100, 0), Quaternion.identity);
                _ballInstance.GetComponent<Rigidbody>().isKinematic = true;
                ((PlayerController)_controllers[0]).BallInstance = _ballInstance.GetComponent<Ball>();
                ((PlayerController)_controllers[0]).PlayerCameraController.BallInstance = _ballInstance.GetComponent<Ball>();
            }
            else
            {
                photonView.RPC("AskFindController", RpcTarget.MasterClient);
            }
        }
        else
        {
            _ballInstance = Instantiate(BallPrefab);
        }
    }

    private void Update()
    {
        if (PhotonNetwork.IsConnected && _loadingScreenAnimatedTextCoroutine == null && GameState == GameState.BEFOREGAME)  
        {
            _loadingScreenAnimatedTextCoroutine = StartCoroutine(AnimateLoadingText());
        }
    }

    public void Init()
    {
        _endGameUI.SetActive(false);

        ServiceOnOriginalSide = true;

        GameState = GameState.SERVICE;
        foreach (ControllersParent controller in _controllers)
        {
            controller.PlayerState = PlayerStates.IDLE;
        }

        _serverIndex = 0;
        _controllers[_serverIndex].IsServing = true;

        // Double
        if (_controllers.Count > 2 )
        {
            CameraManager.InitSplitScreenCameras();
        }else if (GameParameters.Instance.LocalNbPlayers == _controllers.Count) // Simple with 2 locals
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

        _teamControllersAssociated = new Dictionary<ControllersParent, Teams>();

        int i = 0;
        foreach (ControllersParent controller in _controllers)
        {
            Teams team = (Teams)Enum.GetValues(typeof(Teams)).GetValue(i);
            _teamControllersAssociated.Add(controller, team);
            i++;

            if (i > 1)
            {
                i %= 2;
            }
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

        _lastCoroutineStarted = StartCoroutine(CharactersSoundsPlayer(random.Next(3, 12)));
	}

    #endregion

    private IEnumerator AnimateLoadingText()
    {
        while (_loadingScreen.activeSelf)
        {
            yield return new WaitForSeconds(_pointsAppearancePeriod);

            if (_animatedLoadingText.text == ". . . ")
            {
                _animatedLoadingText.text = "";
            }
            else
            {
                _animatedLoadingText.text += ". ";
            }
        }

        _loadingScreenAnimatedTextCoroutine = null;
    }

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
        for(int i = 0; i < _faultLinesXByTeam.Count; i++)
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
        if(movementDirection == -currentForwardVector)
        {
            return Mathf.Abs(borderPointsContainer.BackPointTransform.position.z - playerPosition.z);
        }
        if(movementDirection == currentRightVector)
        {
            return Mathf.Abs(borderPointsContainer.RightPointTransform.position.x - playerPosition.x);
        }
        if(movementDirection == -currentRightVector)
        {
            return Mathf.Abs(borderPointsContainer.LeftPointTransform.position.x - playerPosition.x);
        }

        return 0f;
    }

    public void EndOfPoint(Teams teamWhoWonPoint)
    {
        GameState = GameState.ENDPOINT;

        for(int i = 0; i < _controllers.Count; i++)
        {
            if (_controllers[i].PlayerTeam == teamWhoWonPoint)
            {
				_controllers[i].LaunchCelebration();
                GameParameters.Instance.PlayersCharacter[i].PlaySound("Happy");
			}
            else
            {
				_controllers[i].LaunchCelebration();
				GameParameters.Instance.PlayersCharacter[i].PlaySound("Sad");
			}

            _controllers[i].ResetAtService();
        }
        
        _supporterManager.AnimateSupportersAfterPoint();
    }

    public void EndOfPointWithoutPointWinner()
    {
        GameState = GameState.ENDPOINT;

        AudioManager.Instance.PlaySfx("EndPointCrowd");

        foreach (var player in _controllers)
        {
            player.ResetAtService();
        }
    }

    public void EndOfGame(int playerIndex)
    {
        //TODO : uncomment when finished
        StopCoroutine(_lastCoroutineStarted);
        ControllerManager.Instance.ChangeCtrlersActMapToMenu();
        _inGameUI.SetActive(false);
        _endGameUI.SetActive(true);
        CameraManager.EndGameCameraMode();
        _endGameUI.transform.GetChild(0).GetComponent<EndMatchUI>().Init(playerIndex);
        //SceneManager.LoadScene("Clean_UI_Final");
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

        if ((PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient) || !PhotonNetwork.IsConnected)
        {
            _ballInstance.transform.position = _serviceBallInitializationPoint.position;
        }

        if (_serverIndex == 0)
        {
            _ballInstance.GetPhotonView().RequestOwnership();
        }
    }

    public void DisableAllServiceDetectionVolumes()
    {
        foreach(ControllersParent controller in _controllers)
        {
            controller.BallServiceDetectionArea.gameObject.SetActive(false);
        }
    }

    private IEnumerator CharactersSoundsPlayer(int time)
    {
        yield return new WaitForSeconds(time);
        GameParameters.Instance.PlayersCharacter[random.Next(0, GameParameters.Instance.PlayersCharacter.Count())].PlaySound("VariousSounds");
		_lastCoroutineStarted = StartCoroutine(CharactersSoundsPlayer(random.Next(4, 12)));
    }

    private void InstantiatePlayer(bool isMasterClient)
    {
        GameObject playerObject = PhotonNetwork.Instantiate(_onlinePlayerPrefab.name, new Vector3(0, 0, 0), Quaternion.identity);
        PlayerController playerController = playerObject.GetComponent<PlayerController>();

        if (isMasterClient)
        {
            playerController.PlayerTeam = Teams.TEAM1;
        }
        else
        {
            playerController.PlayerTeam = Teams.TEAM2;
        }

        _controllers.Add(playerController);
    }

    private void FindController()
    {
        ControllersParent[] controllers = FindObjectsOfType<ControllersParent>();
        foreach (ControllersParent controller in controllers)
        {
            if (!_controllers.Contains(controller))
            {
                controller.PlayerTeam = _controllers[0].PlayerTeam == Teams.TEAM1 ? Teams.TEAM2 : Teams.TEAM1;
                controller.gameObject.GetComponent<PlayerInput>().enabled = false;
                _controllers.Add(controller);
            }
        }

        if (PhotonNetwork.IsMasterClient && _controllers.Count == PhotonNetwork.CurrentRoom.PlayerCount && GameState == GameState.BEFOREGAME)
        {
            photonView.RPC("AskFindController", RpcTarget.Others);
            ((PlayerController)_controllers[1]).BallInstance = _ballInstance.GetComponent<Ball>();
            ((PlayerController)_controllers[1]).PlayerCameraController.BallInstance = _ballInstance.GetComponent<Ball>();
        }
        else if (!PhotonNetwork.IsMasterClient)
        {
            _ballInstance = FindObjectOfType<Ball>().gameObject;
            _ballInstance.GetComponent<Rigidbody>().isKinematic = true;

            foreach (ControllersParent controller in _controllers)
            {
                ((PlayerController)controller).BallInstance = _ballInstance.GetComponent<Ball>();
                ((PlayerController)controller).PlayerCameraController.BallInstance = _ballInstance.GetComponent<Ball>();
            }

            photonView.RPC("StartGame", RpcTarget.AllViaServer);
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

        _serverIndex = PhotonNetwork.IsMasterClient ? 0 : 1;
        _controllers[_serverIndex].IsServing = true;
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

        _faultLinesXByTeam = new Dictionary<Teams, float[]>()
        {
            {Teams.TEAM1, new float[]{ _leftFaultLineXFromFirstSide, -_leftFaultLineXFromFirstSide } },
            {Teams.TEAM2, new float[]{ -_leftFaultLineXFromFirstSide, _leftFaultLineXFromFirstSide } }
        };

        GameManager.Instance.CameraManager.InitSoloCamera();
        GameManager.Instance.SideManager.SetSidesInOnlineMatch(true, ServiceOnOriginalSide, PhotonNetwork.IsMasterClient);
        GameManager.Instance.ServiceManager.SetServiceBoxCollider(false);

        if (PhotonNetwork.IsConnected)
        {
            _loadingScreen.SetActive(false);
            _inGameUI.SetActive(true);
        }
    }

    #region RPC METHODS

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
    public void BallThrown()
    {
        _ballInstance.GetComponent<Rigidbody>().isKinematic = false;

        if ((PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient) || !PhotonNetwork.IsConnected)
        {
            _ballInstance.GetComponent<Rigidbody>().AddForce(Vector3.up * GameManager.Instance.Controllers[GameManager.Instance.ServerIndex].ActionParameters.ServiceThrowForce);
        }
    }

    [PunRPC]
    private void ShootOnline(Vector3 ballPosition, float force, string hitType, float risingForceFactor, Vector3 normalizedHorizontalDirection, Player shootingPlayer)
    {
        PlayerController shootingPlayerController;

        if (Instance.Controllers[0].gameObject.GetPhotonView().Owner == shootingPlayer)
        {
            shootingPlayerController = (PlayerController)Instance.Controllers[0];
        }
        else
        {
            shootingPlayerController = (PlayerController)Instance.Controllers[1];
            shootingPlayerController.PlayerAndGameStatesUpdating();
        }

        _ballInstance.GetComponent<Ball>().InitializeActionParameters(NamedActions.GetActionParametersByName(_controllers[0].GetComponent<PlayerController>().PossibleActions, hitType));
        _ballInstance.GetComponent<Ball>().InitializePhysicsMaterial(hitType == "Drop" ? NamedPhysicMaterials.GetPhysicMaterialByName(_controllers[0].GetComponent<PlayerController>().PossiblePhysicMaterials, "Drop") :
            NamedPhysicMaterials.GetPhysicMaterialByName(_controllers[0].GetComponent<PlayerController>().PossiblePhysicMaterials, "Normal"));
        _ballInstance.GetComponent<Ball>().ApplyForce(force, risingForceFactor, normalizedHorizontalDirection, shootingPlayerController);
    }

    [PunRPC]
    public void SetSidesInOnlineMatch(bool serveRight, bool originalSides)
    {
        SideManager.SetSidesInOnlineMatch(serveRight, originalSides, PhotonNetwork.IsMasterClient);
    }

    [PunRPC]
    public void ServingPlayerResetAfterWrongFirstService()
    {
        Ball ball = _ballInstance.GetComponent<Ball>();

        ball.LastPlayerToApplyForce.ServicesCount++;
        ball.LastPlayerToApplyForce.BallServiceDetectionArea.gameObject.SetActive(true);
        ball.LastPlayerToApplyForce.ResetLoadedShotVariables();

        GameManager.Instance.ServiceManager.EnableLockServiceColliders();

        ball.ResetBall();
    }

    [PunRPC]
    private void EndPoint(Teams winningPointTeam)
    {
        GameManager.Instance.EndOfPoint(winningPointTeam);
        GameManager.Instance.ScoreManager.AddPoint(winningPointTeam);
        _ballInstance.GetComponent<Ball>().ResetBall();
    }

    [PunRPC]
    public void ResetBall()
    {
        _ballInstance.GetComponent<Ball>().ResetBall();
    }

    #region ONLINE SMASH MANAGEMENT

    [PunRPC]
    public void OnlineBallPositionSettingDuringSmash(Vector3 ballPosition)
    {
        _ballInstance.GetComponent<Rigidbody>().isKinematic = true;
        _ballInstance.transform.position = ballPosition;
        ((PlayerController)_controllers[0]).OtherPlayerIsSmashing = true;
    }

    [PunRPC]
    public void SmashShot()
    {
        _ballInstance.GetComponent<Rigidbody>().isKinematic = false;
        ((PlayerController)_controllers[0]).OtherPlayerIsSmashing = false;
    }

    #endregion

    #endregion
}
