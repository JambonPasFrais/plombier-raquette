using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.UI;
using System;
using TMPro;
using Photon.Pun.Demo.PunBasics;
using UnityEditor;
using Newtonsoft.Json;


public class OnlineManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private Button _connectButton;
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _startButton;
    [SerializeField] private GameObject MainMenu;
    [SerializeField] private GameObject CharacterSelection;
    //[SerializeField] private GameObject RoomSelectionPanel;
    //[SerializeField] private GameObject RoomCreationPanel;
    [SerializeField] private GameObject RoomPanel;
    [SerializeField] private GameObject PlayersContainer;
    [SerializeField] private GameObject PlayerObject;
    [SerializeField] private GameObject OtherPlayerObject;
    [SerializeField] private List<CharacterData> CharDataList;
    [SerializeField] private Dictionary<string,CharacterData> CharDataDic = new Dictionary<string, CharacterData>();

    private MyPlayerCard _localPlayerCard;



    private byte _maxPlayersPerRoom = 2;

    bool _isConnecting;
    bool _Connected = false;

    public static OnlineManager Instance;

    public byte MaxPlayersPerRoom { get => _maxPlayersPerRoom; set => _maxPlayersPerRoom = value; }

    //string _gameVersion = "";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    private void Start()
    {
        foreach (CharacterData cd in CharDataList)
        {
            CharDataDic.Add(cd.Name, cd);
        }
    }
    private void Update()
    {
        _playButton.interactable = _Connected;
    }
    IEnumerator Connect()
    {
        _isConnecting = true;

        _connectButton.interactable = false;

        if (PhotonNetwork.IsConnected)
        {

            yield return null;
        }
        else
        {
            PhotonNetwork.LocalPlayer.NickName = "Player " + UnityEngine.Random.Range(0, 21);
            PhotonNetwork.ConnectUsingSettings();
        }
        yield return null;
    }


    public override void OnConnectedToMaster()
    {
        if (_isConnecting)
        {
            _Connected = true;
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
        ChangeActivePanel(RoomPanel.name);
        if (!PhotonNetwork.IsMasterClient)
        {
            GetOtherPlayersInRoomInformation();
        }
        
        GameObject myPlayerPreview = Instantiate(PlayerObject, PlayersContainer.transform);
        _localPlayerCard = myPlayerPreview.GetComponent<MyPlayerCard>();
        _localPlayerCard.Initialize(PhotonNetwork.LocalPlayer.NickName, GameParameters.Instance.GetCharactersPlayers()); ;
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

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        if (PhotonNetwork.IsMasterClient)
        {
            
        }
    }
    public void OnOnlineButtonClicked()
    {
        StartCoroutine(Connect());
        ChangeActivePanel(CharacterSelection.name);
    }
    public void OnPlayButtonClicked()
    {
        PhotonNetwork.JoinRandomRoom();
    }
    public void OnBackButtonClicked()
    {
       PhotonNetwork.Disconnect();
    }


    public void ChangeActivePanel(string menuName)
    {
        MainMenu.SetActive(menuName.Equals(MainMenu.name));
        CharacterSelection.SetActive(menuName.Equals(CharacterSelection.name));
        RoomPanel.SetActive(menuName.Equals(RoomPanel.name));
        //RoomSelectionPanel.SetActive(menuName.Equals(RoomSelectionPanel.name));
        //RoomCreationPanel.SetActive(menuName.Equals(RoomCreationPanel.name));
    }
    private void GetOtherPlayersInRoomInformation()
    {
        photonView.RPC("InstantiateOtherPlayerCard", RpcTarget.Others, GameParameters.Instance.GetCharactersPlayers().Name, PhotonNetwork.LocalPlayer.NickName);
    }
    [PunRPC]
     private void InstantiateOtherPlayerCard(string otherPlayerCharacterData, string nickname)
    {
        GameObject otherPlayerPreview = Instantiate(OtherPlayerObject, PlayersContainer.transform);
        otherPlayerPreview.GetComponent<PlayerCard>().Initialize(nickname, CharDataDic[otherPlayerCharacterData]);
        string characterData = GameParameters.Instance.GetCharactersPlayers().Name;
        photonView.RPC("InstantiatePresentPlayers", RpcTarget.Others, characterData, nickname, PhotonNetwork.LocalPlayer.NickName);
        
    } 
    
    [PunRPC]
     private void InstantiatePresentPlayers(string otherPlayerCharacterData, string nickname, string senderNickname)
    {
        if (PhotonNetwork.LocalPlayer.NickName == nickname)
        {
            GameObject otherPlayerPreview = Instantiate(OtherPlayerObject, PlayersContainer.transform);
            otherPlayerPreview.GetComponent<PlayerCard>().Initialize(senderNickname, CharDataDic[otherPlayerCharacterData]);
        }
    }
}
