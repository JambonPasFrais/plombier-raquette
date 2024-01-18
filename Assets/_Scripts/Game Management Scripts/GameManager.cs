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

public class GameManager : MonoBehaviour
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

    [SerializeField] private GameObject _smashTargetGo;

    [HideInInspector] public bool ServiceOnOriginalSide;

    #endregion

    #region PRIVATE FIELDS

    [Header("Environment Objects")]
    [SerializeField] private GameObject _net;
    [SerializeField] private List<ControllersParent> _controllers;
    [SerializeField] private FieldBorderPointsContainer[] _borderPointsContainers;
    [SerializeField] private float _leftFaultLineXFromFirstSide;
    [SerializeField] private SupporterManager _supporterManager;

	[Header("Canvas References")]
	[SerializeField] private GameObject _inGameUI;
	[SerializeField] private GameObject _endGameUI;

	private Dictionary<ControllersParent, Teams> _teamControllersAssociated;
    private Dictionary<Teams, FieldBorderPointsContainer> _fieldBorderPointsByTeam;
    private Dictionary<Teams, float[]> _faultLinesXByTeam;

    [SerializeField] private GameObject _ballInstance;
    private int _serverIndex;
    private Transform _serviceBallInitializationPoint;
    private System.Random random = new System.Random();
    private Coroutine _lastCoroutineStarted;

    #endregion

    #region GETTERS

    public GameObject BallInstance {  get { return _ballInstance; } }
    public GameObject Net {  get { return _net; } }
    public List<ControllersParent> Controllers {  get { return _controllers; } }
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

        _ballInstance = Instantiate(BallPrefab);
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

        // More than one player -> Initialize Split-Screen Cameras
        if (GameParameters.Instance.LocalNbPlayers > 1)
        {
            CameraManager.InitSplitScreenCameras();
        }
        // One local player -> init solo camera
        else
        {
            CameraManager.InitSoloCamera();
        }
        
        SideManager.SetSides(_controllers, true, ServiceOnOriginalSide);
        
        ServiceManager.SetServiceBoxCollider(false);

        bool isTieBreak = false;

        if (GameParameters.Instance.CurrentGameMode.NbOfGames == 1)
            isTieBreak = true;
        
        ScoreManager.InitGameLoop(GameParameters.Instance.CurrentGameMode.NbOfSets, GameParameters.Instance.CurrentGameMode.NbOfGames, isTieBreak); //TODO : can't play just a tiebreak ?
        
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
        _ballInstance.transform.position = _serviceBallInitializationPoint.position;
    }

    public void DesactivateAllServiceDetectionVolumes()
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
}
