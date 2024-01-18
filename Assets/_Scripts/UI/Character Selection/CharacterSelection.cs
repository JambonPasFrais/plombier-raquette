using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CharacterSelection : MonoBehaviour
{
    #region Serialized Field
    
    [Header("Common Instances")]
    [SerializeField] private GameObject _aceItWindow;
    [SerializeField] private CharacterUI _characterUIPrefab;
    [SerializeField] private Transform _characterIconsContainerSolo;
    [SerializeField] private Transform _characterIconsContainerMulti;
    [SerializeField] private LayerMask _characterUILayerMask;
    [SerializeField] private Transform _characterModelsPool; //is equivalent to _charactersModelsContainer
    [SerializeField] private Button _returnButton;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _joinRoomButton;
	[SerializeField] private Button _aceItPlayButton;
    
    [Header("Window Types")]
    [SerializeField] private GameObject _matchSoloWindow;
    [SerializeField] private GameObject _matchSingleWindow;
    [SerializeField] private GameObject _matchDoubleWindow;
    
    [Header("Showrooms Types")] //TODO : rework PlayerShowroom
    [SerializeField] private List<PlayerShowroom> _characterShowroomsSolo = new List<PlayerShowroom>();
    [SerializeField] private List<PlayerShowroom> _characterShowroomsSingle = new List<PlayerShowroom>();
    [SerializeField] private List<PlayerShowroom> _characterShowroomsDouble = new List<PlayerShowroom>();

    #endregion
    
    #region Logic Variables
    
    // All Characters Data
    private List<CharacterData> _allCharactersData = new List<CharacterData>(); // Equivalent to _characters
	
    // All characters UI
    private List<CharacterUI> _allCharactersUi = new List<CharacterUI>(); // _charactersUI
	
    //TODO : remove [SerializedField]
    // Every character that are selected
    [SerializeField] private List<CharacterData> _selectedCharacters; // _playersCharacter
	
    // Keeps track of the selected characters per player index
    [SerializeField] private List<CharacterUI> _selectedCharactersUIs; 
    
    // Models sorted by names
    private Dictionary<string, GameObject> _characterModelsByName = new Dictionary<string, GameObject>();// _charactersModel
    
    // Keep track of current showroom
    private List<PlayerShowroom> _currentShowroomList = new List<PlayerShowroom>();
	
    // Will be used to save a singleton value
    private int _totalNbPlayers;

    // Here to manage the selection of a button
    private InputSystemUIInputModule _inputSystemUIInputModule;
    
    private ControllerSelectionMenu _controllerSelectionMenu;

    [SerializeField] private bool _isOnlineMode;
    
    #endregion
    
    #region UNITY FUNCTIONS
    
    private void Start()
    {
	    _inputSystemUIInputModule = MenuManager.Instance.CurrentEventSystem.GetComponent<InputSystemUIInputModule>();
    }
    
    #endregion

    #region LISTENERS

    public void OnCharacterSelectionMenuLoad(ControllerSelectionMenu controllerSelectionMenuInstance)
    {
	    MenuManager.Instance.PlaySound("ChooseYourCharacter");

	    _allCharactersData = MenuManager.Instance.Characters;
	    _characterModelsPool = MenuManager.Instance.CharactersModelsParent;
	    _characterModelsByName = MenuManager.Instance.CharactersModel;
	    _controllerSelectionMenu = controllerSelectionMenuInstance;
	    _aceItWindow.SetActive(false);
	    
	    ClearShowroomsCharEmblems();
	    
	    SetShowroomType();

	    _selectedCharacters = new List<CharacterData>(new CharacterData[_totalNbPlayers]);
	    _selectedCharactersUIs = new List<CharacterUI>(new CharacterUI[_totalNbPlayers]);
	    
	    SetPlayerInfos();
	    
	    SetIconsContainer();
	    
	    InitNavigationButtons();
	    
	    CreateCharacterIcons();
	    
	    SetModelRandomForBots();

		MenuManager.Instance.CurrentEventSystem.SetSelectedGameObject(null);
    }

    public void OnCharacterSelectionMenuLoad()
    {
        MenuManager.Instance.PlaySound("ChooseYourCharacter");

        _allCharactersData = MenuManager.Instance.Characters;
        _characterModelsPool = MenuManager.Instance.CharactersModelsParent;
		_characterModelsByName = MenuManager.Instance.CharactersModel;
        _aceItWindow.SetActive(false);

        ClearShowroomsCharEmblems();

        SetShowroomType();

        _selectedCharacters = new List<CharacterData>(new CharacterData[_totalNbPlayers]);
        _selectedCharactersUIs = new List<CharacterUI>(new CharacterUI[_totalNbPlayers]);

        SetPlayerInfos();

        SetIconsContainer();

        InitNavigationButtons();

        CreateCharacterIcons();

        SetModelRandomForBots();

        MenuManager.Instance.CurrentEventSystem.SetSelectedGameObject(null);
    }

    public void LoadFromOnlineMenu(bool isOnline)
	{
	    _isOnlineMode = isOnline;
	}

    public void OnMenuDisabled()
    {
	    ResetModelPool();
	    ResetCurrentShowroom();
	    ResetSelectedPlayers();
	    ResetCharacterUis();
    }
    
    public void OnPlay()
    {
		if (!PhotonNetwork.IsConnected)
		{
            TransformRandomSelectionInCharacter();

            GameParameters.Instance.SetCharactersPlayers(_selectedCharacters);

            _aceItWindow.SetActive(false);
        }
		else
		{
            if (_selectedCharacters[0] != null && _selectedCharacters[0].Name == "Random")
            {
                SetRandomCharacterForSpecifiedPlayer(0);
            }

            GameParameters.Instance.SetCharactersPlayers(_selectedCharacters);
        }
    }

    public void OnNext()
    {
	    GameParameters.Instance.SetCharactersPlayers(_selectedCharacters);
    }
    
    #endregion

    #region SHOWROOMS
    
    private void SetShowroomType()
    {
	    if (GameParameters.IsTournamentMode || IsOnlineMode())
		    SetSoloShowroom();
	    else if (!GameParameters.Instance.IsDouble)
		    SetSingleShowroom();
	    else
		    SetDoubleShowroom();
    }

    private void SetSoloShowroom()
    {
	    _matchSingleWindow.SetActive(false);
	    _matchDoubleWindow.SetActive(false);

	    _matchSoloWindow.SetActive(true);
	    
	    _totalNbPlayers = 1;
	    
	    _currentShowroomList = _characterShowroomsSolo;
    }

    private void SetSingleShowroom()
    {
	    _matchSoloWindow.SetActive(false);
	    _matchDoubleWindow.SetActive(false);
	    
	    _matchSingleWindow.SetActive(true);

	    _totalNbPlayers = 2;
	    
	    _currentShowroomList = _characterShowroomsSingle;
    }

    private void SetDoubleShowroom()
    {
	    _matchSoloWindow.SetActive(false);
	    _matchSingleWindow.SetActive(false);
	    
	    _matchDoubleWindow.SetActive(true);

	    _totalNbPlayers = 4;
	    
	    _currentShowroomList = _characterShowroomsDouble;
    }

    private void ClearShowroomsCharEmblems()
    {
	    foreach(var item in _characterShowroomsSolo)
	    {
		    item.CharacterEmblem.gameObject.SetActive(false);
	    }
	    
	    foreach(var item in _characterShowroomsSingle)
	    {
		    item.CharacterEmblem.gameObject.SetActive(false);
	    }
		
	    foreach(var item in _characterShowroomsDouble)
	    {
		    item.CharacterEmblem.gameObject.SetActive(false);
	    }
    }
    
    #endregion

    #region PLAYER INFOS
    private void SetPlayerInfos()
    {
	    switch (_totalNbPlayers)
	    {
		    case 1:
			    SetPlayerInfoSolo();
			    break;
		    case 2:
			    SetPlayerInfoSingle();
			    break;
		    case 4:
			    SetPlayerInfoDouble();
			    break;
	    }
    }

    private void SetPlayerInfoSolo()
    {
	    _characterShowroomsSolo[0].PlayerInfo.text = "P1";
    }
    
    private void SetPlayerInfoSingle()
    {
	    _characterShowroomsSingle[0].PlayerInfo.text = "P1";
	    
	    if (GameParameters.Instance.LocalNbPlayers == 1)
		    _characterShowroomsSingle[1].PlayerInfo.text = "COM";
	    else
		    _characterShowroomsSingle[1].PlayerInfo.text = "P2";
    }

    private void SetPlayerInfoDouble()
    {
	    _characterShowroomsDouble[0].PlayerInfo.text = "P1";

	    for(int i = 1; i < 4; i++)
	    {
		    if (i < GameParameters.Instance.LocalNbPlayers)
			    _characterShowroomsDouble[i].PlayerInfo.text = "P" + (i + 1).ToString();

		    else
			    _characterShowroomsDouble[i].PlayerInfo.text = "COM";
	    }
    }
    
    #endregion

    #region INITIALIZATION
    
    private void CreateCharacterIcons()
    {
	    foreach (var item in _allCharactersData)
	    {
		    CharacterUI charUI = Instantiate(_characterUIPrefab, IsSoloMode() ? _characterIconsContainerSolo : _characterIconsContainerMulti);

		    charUI.SetVisual(item);

		    charUI.Character.Init();

		    _allCharactersUi.Add(charUI);
	    }
    }

    private void SetModelRandomForBots()
    {
	    for (int i = _totalNbPlayers - 1; i > GameParameters.Instance.LocalNbPlayers - 1; i--)
	    {
		    // We select the "Random" Character UI so it's in Last
		    CharacterUI characterUI = _allCharactersUi.Last();
			
		    // Get the CharacterModel
		    if (!_characterModelsByName.TryGetValue(characterUI.Character.Name + i, out var characterModel))
			    return;
			
		    // Modify Showroom Infos
		    _currentShowroomList[i].CharacterName.text = characterUI.Character.Name;
		    _currentShowroomList[i].Background.color = characterUI.Character.CharacterPrimaryColor;
		    _currentShowroomList[i].NameBackground.color = characterUI.Character.CharacterSecondaryColor;

		    // Display Model from pool
		    characterModel.transform.SetParent(_currentShowroomList[i].ModelLocation);
		    characterModel.transform.localPosition = new Vector3(0f, 150f, 0f);
		    characterModel.transform.localRotation = Quaternion.Euler(new Vector3(characterUI.Character.Name == "Random" ? -90 : 0, 180, 0));
		    characterModel.SetActive(true);
		    
		    // Update logic Variables
		    _selectedCharactersUIs[i] = characterUI;
		    _selectedCharacters[i] = characterUI.Character;
	    }
    }

    private void SetIconsContainer()
    {
	    _characterIconsContainerSolo.gameObject.SetActive(IsSoloMode());
	    _characterIconsContainerMulti.gameObject.SetActive(!IsSoloMode());
    }

	// Become useless because the buttons doesn't exist anymore 
    private void InitNavigationButtons()
    {
	    _nextButton.gameObject.SetActive(IsSoloMode() && !IsOnlineMode());
		_nextButton.interactable = false;
	    _joinRoomButton.gameObject.SetActive(IsSoloMode() && IsOnlineMode());
		_joinRoomButton.interactable = false;
/*        _returnButton.gameObject.SetActive(IsSoloMode() && IsOnlineMode());
        _returnButton.interactable = true;*/
        _aceItPlayButton.gameObject.SetActive(false);
	}
    
    #endregion
    
    #region RESET
    
    public void ResetModelPool()
    {
	    foreach(var item in _characterModelsByName)
	    {
		    item.Value.transform.SetParent(_characterModelsPool);
		    item.Value.transform.localPosition = Vector3.zero;
		    item.Value.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
		    item.Value.gameObject.SetActive(false);
	    }
    }

    public void ResetCurrentShowroom()
    {
	    foreach (var item in _currentShowroomList)
	    {
		    item.Background.color = Color.white;
		    item.NameBackground.color = Color.black;
		    item.CharacterName.text = "";
		    item.CharacterEmblem.sprite = null;
		    item.CharacterEmblem.gameObject.SetActive(false);
	    }
    }

    public void ResetSelectedPlayers()
    {
	    for (int i = 0; i < _selectedCharacters.Count; i++)
	    {
		    _selectedCharacters[i] = null;
	    }
    }

    public void ResetCharacterUis()
    {
	    foreach(var item in _allCharactersUi)
	    {
		    item.SetSelected(false);
		    Destroy(item.gameObject);
	    }
	    
	    _allCharactersUi.Clear();
    }
    
    #endregion
    
    #region RANDOM CHARACTER CREATION
    
    private void TransformRandomSelectionInCharacter()
    {
	    for (int i = 0; i < _selectedCharacters.Count; i++)
	    {
		    if (_selectedCharacters[i] != null && _selectedCharacters[i].Name == "Random")
		    {
			    SetRandomCharacterForSpecifiedPlayer(i);
		    }
	    }
    }
    
    private void SetRandomCharacterForSpecifiedPlayer(int playerIndex)
    {
	    // players who selected "Random" have a question mark model so we remove it first
	    RemoveCharacterFromPlayerSelectionUi(playerIndex);
		
	    //_charactersUI[playerIndex].SetSelected(true);
	    CharacterData randomCharacter = ReturnRandomCharacter();
	    _allCharactersUi.FirstOrDefault(x => x.Character.Name == randomCharacter.Name)!.SetSelected(true);
	    _currentShowroomList[playerIndex].CharacterName.text = randomCharacter.Name;
	    _currentShowroomList[playerIndex].Background.color = randomCharacter.CharacterPrimaryColor;
	    _currentShowroomList[playerIndex].NameBackground.color = randomCharacter.CharacterSecondaryColor;
	    _currentShowroomList[playerIndex].CharacterEmblem.gameObject.SetActive(true);
	    _currentShowroomList[playerIndex].CharacterEmblem.sprite = randomCharacter.CharactersLogo;

	    if (!_characterModelsByName.TryGetValue(randomCharacter.Name, out GameObject go))
		    return;
			
	    go.transform.SetParent(_currentShowroomList[playerIndex].ModelLocation);
	    go.transform.localPosition = Vector3.zero;
	    go.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
	    go.transform.localScale = new Vector3(20, 20, 20);
	    go.SetActive(true);
			
	    _selectedCharacters[playerIndex] = randomCharacter;
    }
    
    private CharacterData ReturnRandomCharacter()
    {
	    int randomIndex;
	    do
	    {
		    randomIndex = Random.Range(0, _allCharactersUi.Count - 1); // Don't use the last character because it's the Random one
	    } while (_allCharactersUi[randomIndex].IsSelected);

	    return _allCharactersData[randomIndex];;
    }
    
    #endregion
    
    private bool RemoveCharacterFromPlayerSelectionUi(int playerIndex)
    {
	    if (_currentShowroomList[playerIndex].ModelLocation.childCount <= 0)
		    return false;
		
	    _selectedCharactersUIs[playerIndex].SetSelected(false);
	    _selectedCharactersUIs[playerIndex] = null;
		
	    GameObject oldGo = _currentShowroomList[playerIndex].ModelLocation.GetChild(0).gameObject;
	    oldGo.transform.SetParent(_characterModelsPool);
	    oldGo.transform.localPosition = Vector3.zero;
	    oldGo.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
	    oldGo.gameObject.SetActive(false);
		
	    _selectedCharacters[playerIndex] = null;

		_currentShowroomList[playerIndex].CharacterName.text = "";
		_currentShowroomList[playerIndex].NameBackground.color = Color.black;
		_currentShowroomList[playerIndex].Background.color = Color.white;
		_currentShowroomList[playerIndex].CharacterEmblem.sprite = null;
		_currentShowroomList[playerIndex].CharacterEmblem.gameObject.SetActive(false);

		CheckReadyToPlayStatus();

		return true;
    }

	private void CheckReadyToPlayStatus()
	{
		if (IsSoloMode() && !IsOnlineMode() && IsEveryCharSelectedByLocals())
		{
			_nextButton.interactable = true;
			StartCoroutine(WaitBeforeSelectNewButton(_nextButton.gameObject));
		}
		else if (IsSoloMode() && IsOnlineMode() && IsEveryCharSelectedByLocals()) 
		{
			_joinRoomButton.interactable = true;
			StartCoroutine(WaitBeforeSelectNewButton(_joinRoomButton.gameObject));
		}
		else if (IsEveryCharSelectedByLocals())
		{
			_aceItWindow.SetActive(true);
			MenuManager.Instance.PlaySound("AceItSound");
			_aceItPlayButton.gameObject.SetActive(true);
			StartCoroutine(WaitBeforeSelectNewButton(_aceItPlayButton.gameObject));
		}
		else
		{
			_aceItWindow.SetActive(false);
			_nextButton.interactable = false;
			_joinRoomButton.interactable = false;
		}
    }

    private bool IsSoloMode()
    {
	    return _totalNbPlayers == 1;
    }

    private bool IsOnlineMode()
    {
	    return _isOnlineMode;
    }
    
    private bool IsEveryCharSelectedByLocals()
    {
	    for (int i = 0; i < GameParameters.Instance.LocalNbPlayers; i++)
	    {
		    if (_selectedCharacters[i] == null)
			    return false;
	    }
		
	    return true;
    } 

    #region CALLED EXTERNALLY

    public bool HandleCharacterSelectionInput(Ray ray, int playerIndex)
    {
	    if (Physics.Raycast(ray, out var hit, float.PositiveInfinity, _characterUILayerMask)
	        && hit.collider.TryGetComponent(out CharacterUI characterUI)
	        && !characterUI.IsSelected)
	    {
		    // We do this because different players can select the Random Statement
		    if (characterUI.Character.Name != "Random")
			    characterUI.SetSelected(true);

		    characterUI.Character.PlaySound("Selected");
		    _currentShowroomList[playerIndex].CharacterName.text = characterUI.Character.Name;
		    _currentShowroomList[playerIndex].Background.color = characterUI.Character.CharacterPrimaryColor;
		    _currentShowroomList[playerIndex].NameBackground.color = characterUI.Character.CharacterSecondaryColor;

		    if (characterUI.Character.Name != "Random") 
		    {
			    _currentShowroomList[playerIndex].CharacterEmblem.gameObject.SetActive(true);
			    _currentShowroomList[playerIndex].CharacterEmblem.sprite = characterUI.Character.CharactersLogo; 
		    }
		    else
			    _currentShowroomList[playerIndex].CharacterEmblem.gameObject.SetActive(false);

		    string charNameToLookFor = characterUI.Character.Name == "Random" ? characterUI.Character.Name+ playerIndex : characterUI.Character.Name;

		    if (_characterModelsByName.TryGetValue(charNameToLookFor, out var characterModel))
		    {
			    characterModel.transform.SetParent(_currentShowroomList[playerIndex].ModelLocation);
				if (characterUI.Character.Name == "Random")
					characterModel.transform.localPosition = new Vector3(0f, 150f, 0f);
				else
					characterModel.transform.localPosition = Vector3.zero;
			    characterModel.transform.localRotation = Quaternion.Euler(new Vector3(characterUI.Character.Name == "Random" ? -90 : 0, 180, 0));
			    if(characterUI.Character.Name == "Random")
					characterModel.transform.localScale = new Vector3(2000, 2000, 2000);
				else
					characterModel.transform.localScale = new Vector3(20, 20, 20);
			    characterModel.SetActive(true);
			    _selectedCharactersUIs[playerIndex] = characterUI;
			    _selectedCharacters[playerIndex] = characterUI.Character;
			    CheckReadyToPlayStatus();
			    return true;
		    }
			
		    Debug.LogError("Character model not found for: " + characterUI.Character.Name);
	    }
	    return false;
    }

    public bool HandleCharacterDeselectionInput(int playerIndex)
    {
	    return RemoveCharacterFromPlayerSelectionUi(playerIndex);
    }

	public void DisableNavigationButtons()
	{
		_nextButton.gameObject.SetActive(false);
		_nextButton.interactable = false;
		_joinRoomButton.gameObject.SetActive(false);
		_joinRoomButton.interactable = false;
/*        _returnButton.gameObject.SetActive(false);
        _returnButton.interactable = false;*/
    }
    
    #endregion

	private IEnumerator WaitBeforeSelectNewButton(GameObject buttonToSelect)
	{
		yield return new WaitForSeconds(0.1f);
		MenuManager.Instance.CurrentEventSystem.SetSelectedGameObject(buttonToSelect);
	}
}
