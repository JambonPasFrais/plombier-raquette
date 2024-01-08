using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.UI;
using System.Linq;

public class OnlineManager : MonoBehaviourPunCallbacks
{
	#region PRIVATE FIELDS

	private static OnlineManager _instance;

    [Header("Buttons Instances")]
    // Online Button from Main Menu
	[SerializeField] private Button _connectButton;
    // Play Button from Online Character Selection Menu
    [SerializeField] private Button _playCharacterSelectionButton;
    // Start Button from Online Room Menu
    [SerializeField] private Button _playOnlineRoomButton;
    // Ready Button from Online Room Menu
	[SerializeField] private Button _readyButton;
    // Leave Button from Online Room Menu
    [SerializeField] private Button _leaveButton;

    [Header("Other Menus Reference")]
    // Main Menu Reference
	[SerializeField] private GameObject _mainMenu;
    // Online Character Selection Menu
    [SerializeField] private GameObject _characterSelection;
    // Online Room Menu Reference
    [SerializeField] private GameObject _roomPanel;

    [Header("Character Data List")]
    // List of all the possible CharacterData
    private List<CharacterData> _charDataList;

    [Header("Online Room Player Showrooms")]
    // References to player's visuals
    [SerializeField] private List<PlayerShowroom> _playerShowrooms = new List<PlayerShowroom>();
	
    // Dictionary containing every CharacterData keyed by Name
	private Dictionary<string, CharacterData> _charDataDic = new Dictionary<string, CharacterData>();

    private Dictionary<string, bool> _inRoomPlayersReadyState = new Dictionary<string, bool>();
    private int _readyPlayersCount;

    private byte _maxPlayersPerRoom = 2;

    private bool _isConnecting;

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
		_charDataList = new List<CharacterData>(MenuManager.Instance.Characters);

		foreach (CharacterData cd in _charDataList)
        {
            _charDataDic.Add(cd.Name, cd);
        }

        _playOnlineRoomButton.interactable = false;
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
			_playCharacterSelectionButton.interactable = true;
			_isConnecting = false;
        }

        ChangeActivePanel(CharacterSelection.name);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = this._maxPlayersPerRoom });
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        _isConnecting = false;
        _connectButton.interactable = true;
		_playCharacterSelectionButton.interactable = false;

	}

	public override void OnJoinedRoom()
    {
        ChangeActivePanel(_roomPanel.name);

        if (!PhotonNetwork.IsMasterClient)
        {
            GetOtherPlayersInRoomInformation();
        }

        _playerShowrooms[0].InitializeOnlineShowroom(GameParameters.Instance.GetCharactersPlayers(), PhotonNetwork.LocalPlayer.NickName);

        if (PhotonNetwork.IsMasterClient)
        {
            _playOnlineRoomButton.gameObject.SetActive(true);
            MenuManager.Instance.SetButtonNavigation(_readyButton, _playOnlineRoomButton, null, null, null);
            MenuManager.Instance.SetButtonNavigation(_playOnlineRoomButton, _leaveButton, _readyButton, null, null);
            MenuManager.Instance.SetButtonNavigation(_leaveButton, null, _playOnlineRoomButton, null, null);
        }
        else
        {
            _playOnlineRoomButton.gameObject.SetActive(false);
            MenuManager.Instance.SetButtonNavigation(_readyButton, _leaveButton, null, null, null);
            MenuManager.Instance.SetButtonNavigation(_leaveButton, null, _readyButton, null, null);
        }
    }

    public void OnStartButtonClicked()
    {
        PhotonNetwork.LoadLevel("OnlineScene");
    }

    public void OnReadyButtonClicked()
    {
        photonView.RPC("PlayerClickedOnReadyButton", RpcTarget.AllViaServer, PhotonNetwork.LocalPlayer);
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

        foreach (var showroom in _playerShowrooms)
        {
            showroom.ResetShowroom();
        }

        ChangeActivePanel(_characterSelection.name);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        _playerShowrooms[1].ResetShowroom();
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
    private void PlayerClickedOnReadyButton(Photon.Realtime.Player player)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if(!_inRoomPlayersReadyState.Keys.Contains(player.NickName))
                _inRoomPlayersReadyState[player.NickName] = true;

            else
                _inRoomPlayersReadyState[player.NickName] = !_inRoomPlayersReadyState[player.NickName];

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
                _playOnlineRoomButton.interactable = true;
            }
            else
            {
                _playOnlineRoomButton.interactable = false;
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
        _playerShowrooms[1].InitializeOnlineShowroom(_charDataDic[otherPlayerCharacterData], nickname);

		photonView.RPC("InstantiatePresentPlayers", RpcTarget.Others, GameParameters.Instance.GetCharactersPlayers().Name, nickname, PhotonNetwork.LocalPlayer.NickName);
    }

    [PunRPC]
    private void InstantiatePresentPlayers(string otherPlayerCharacterData, string nickname, string senderNickname)
    {
        if (PhotonNetwork.LocalPlayer.NickName == nickname)
        {
			_playerShowrooms[1].InitializeOnlineShowroom(_charDataDic[otherPlayerCharacterData], nickname);
		}
	}
}
