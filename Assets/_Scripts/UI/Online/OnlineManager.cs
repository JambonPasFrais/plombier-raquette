using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.UI;


public class OnlineManager : MonoBehaviourPunCallbacks
{
	#region PRIVATE FIELDS

	private static OnlineManager _instance;

    [Header("Buttons Instances")]
    // Online Button from Main Menu
	[SerializeField] private Button _connectButton;
    // Play Button from Online Character Selection Menu
    [SerializeField] private Button _playButton;
    // Start Button from Online Room Menu
    [SerializeField] private Button _startButton;
    // Ready Button from Online Room Menu
	[SerializeField] private Button _readyButton;

    [Header("Other Menus Reference")]
    // Main Menu Reference
	[SerializeField] private GameObject _mainMenu;
    // Online Character Selection Menu
    [SerializeField] private GameObject _characterSelection;
    // Online Room Menu Reference
    [SerializeField] private GameObject _roomPanel;

    [Header("Online Room Menu References")]
    // Players Container from Online Room Menu
    [SerializeField] private GameObject _playersContainer;

    [Header("Prefabs")]
    // My Player Prefab
    [SerializeField] private GameObject _playerObject;
    // Other Player Prefab
    [SerializeField] private GameObject _otherPlayerObject;

    [Header("Character Data List")]
    // List of all the possible CharacterData
    [SerializeField] private List<CharacterData> _charDataList;

    [Header("Online Room Player Showrooms")]
    // References to player's visuals
    [SerializeField] private List<PlayerShowroom> _playerShowrooms = new List<PlayerShowroom>();
	

	private Dictionary<string, CharacterData> _charDataDic = new Dictionary<string, CharacterData>();

    private MyPlayerCard _localPlayerCard;
    private Dictionary<string, bool> _inRoomPlayersReadyState = new Dictionary<string, bool>();
    private int _readyPlayersCount;

    private byte _maxPlayersPerRoom = 2;

    private bool _isConnecting;
    private bool _connected = false;

	#endregion

	#region SETTERS & GETTERS

	public byte MaxPlayersPerRoom 
    { 
        get => _maxPlayersPerRoom; 
        set => _maxPlayersPerRoom = value; 
    }
    public static OnlineManager Instance => _instance;
    public Button ReadyButton => _readyButton;

	#endregion

	#region UNITY METHODS

	private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        foreach (CharacterData cd in _charDataList)
        {
            _charDataDic.Add(cd.Name, cd);
        }

        _startButton.interactable = false;
    }

    private void Update()
    {
        _playButton.interactable = _connected;
    }

	#endregion

	private IEnumerator Connect()
    {
        _isConnecting = true;

        _connectButton.interactable = false;

        if (PhotonNetwork.IsConnected)
        {
            yield return null;
        }
        else
        {
            PhotonNetwork.LocalPlayer.NickName = "Player " + UnityEngine.Random.Range(1, 21); // Pourquoi 21 max ?
            PhotonNetwork.ConnectUsingSettings();
        }

        yield return null;
    }

    public override void OnConnectedToMaster()
    {
        if (_isConnecting)
        {
            _connected = true;
            _isConnecting = false;
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = this._maxPlayersPerRoom });
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        _isConnecting = false;
        _connectButton.interactable = true;
    }

    public override void OnJoinedRoom()
    {
        ChangeActivePanel(_roomPanel.name);

        if (!PhotonNetwork.IsMasterClient)
        {
            GetOtherPlayersInRoomInformation();
        }

        GameObject myPlayerPreview = Instantiate(_playerObject, _playersContainer.transform);
        _localPlayerCard = myPlayerPreview.GetComponent<MyPlayerCard>();
        _localPlayerCard.Initialize(PhotonNetwork.LocalPlayer.NickName, GameParameters.Instance.GetCharactersPlayers());

        if (PhotonNetwork.IsMasterClient)
        {
            _startButton.gameObject.SetActive(true);
        }
        else
        {
            _startButton.gameObject.SetActive(false);
        }
    }

    public void OnStartButtonClicked()
    {
        PhotonNetwork.LoadLevel("Game");
    }

    public void ReadyButtonClicked()
    {
        photonView.RPC("PlayerClickedOnReadyButton", RpcTarget.AllViaServer, PhotonNetwork.LocalPlayer, _localPlayerCard.IsReady);
    }

    public void OnOnlineButtonClicked()
    {
        StartCoroutine(Connect());
        ChangeActivePanel(_characterSelection.name);
    }

    public void OnPlayButtonClicked()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnBackButtonClicked()
    {
        PhotonNetwork.Disconnect();
    }

    public void OnLeaveButtonClicked()
    {
        PhotonNetwork.LeaveRoom();

        for (int i = 0; i < _playersContainer.transform.childCount; i++)
        {
            Destroy(_playersContainer.transform.GetChild(i).gameObject);
        }

        ChangeActivePanel(_characterSelection.name);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        for (int i = 0; i < _playersContainer.transform.childCount; i++)
        {
            PlayerCard player = _playersContainer.transform.GetChild(i).gameObject.GetComponent<PlayerCard>();

            if (player.PlayerName == otherPlayer.NickName)
            {
                Destroy(player.gameObject);
                break;
            }
        }
    }

    public void ChangeActivePanel(string menuName)
    {
        _mainMenu.SetActive(menuName.Equals(_mainMenu.name));
        _characterSelection.SetActive(menuName.Equals(_characterSelection.name));
        _roomPanel.SetActive(menuName.Equals(_roomPanel.name));
    }

    private void GetOtherPlayersInRoomInformation()
    {
        photonView.RPC("InstantiateOtherPlayerCard", RpcTarget.Others, GameParameters.Instance.GetCharactersPlayers().Name, PhotonNetwork.LocalPlayer.NickName);
    }

    [PunRPC]
    private void PlayerClickedOnReadyButton(Photon.Realtime.Player player, bool isReady)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            _inRoomPlayersReadyState[player.NickName] = isReady;

            if (_inRoomPlayersReadyState[player.NickName])
            {
                _readyPlayersCount++;
            }
            else
            {
                _readyPlayersCount--;
            }

            if (_readyPlayersCount == _maxPlayersPerRoom)
            {
                _startButton.interactable = true;
            }
            else
            {
                _startButton.interactable = false;
            }
        }

        //if (PhotonNetwork.LocalPlayer != player)
        //{
        //    for (int i = 1; i < PlayerPreviewsContainer.transform.childCount; i++)
        //    {
        //        OtherPlayerPreview otherPlayerPreview = PlayerPreviewsContainer.transform.GetChild(i).gameObject.GetComponent<OtherPlayerPreview>();

        //        if (otherPlayerPreview.PlayerName == player.NickName)
        //        {
        //            otherPlayerPreview.ReadyStateChanged();
        //            break;
        //        }
        //    }
        //}
    }

    [PunRPC]
    private void InstantiateOtherPlayerCard(string otherPlayerCharacterData, string nickname)
    {
        GameObject otherPlayerPreview = Instantiate(_otherPlayerObject, _playersContainer.transform);
        otherPlayerPreview.GetComponent<PlayerCard>().Initialize(nickname, _charDataDic[otherPlayerCharacterData]);
        string characterData = GameParameters.Instance.GetCharactersPlayers().Name;
        photonView.RPC("InstantiatePresentPlayers", RpcTarget.Others, characterData, nickname, PhotonNetwork.LocalPlayer.NickName);
    }

    [PunRPC]
    private void InstantiatePresentPlayers(string otherPlayerCharacterData, string nickname, string senderNickname)
    {
        if (PhotonNetwork.LocalPlayer.NickName == nickname)
        {
            GameObject otherPlayerPreview = Instantiate(_otherPlayerObject, _playersContainer.transform);
            otherPlayerPreview.GetComponent<PlayerCard>().Initialize(senderNickname, _charDataDic[otherPlayerCharacterData]);
        }
    }
}
