using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CharacterSelectionMenu : CharacterSelection
{
	[Header("Instances")] 
	// Windows
	[SerializeField] private GameObject _aceItWindow;
	[SerializeField] private GameObject _matchSingleWindow;
	[SerializeField] private GameObject _matchDoubleWindow;
	// Character Ui related
	[SerializeField] private GameObject _characterUIPrefab;
	[SerializeField] private Transform _characterUIContainer;
	[SerializeField] private LayerMask _characterUILayerMask;
	// List of player's visual references
	[SerializeField] private List<PlayerShowroom> _characterShowroomsSingle = new List<PlayerShowroom>();
	[SerializeField] private List<PlayerShowroom> _characterShowroomsDouble = new List<PlayerShowroom>();
	// Where the model are in not selected -> pooling optimisation technique
	[SerializeField] private Transform _charactersModelsContainer;
	[SerializeField] private Button _playButton;
	[SerializeField] private GameObject _confirmPlayButton;
	[Header("Other Menus References")]
	[SerializeField] private ControllerSelectionMenu _controllerSelectionMenu;
	
	// All Characters Data
	private List<CharacterData> _characters = new List<CharacterData>();
	
	// All characters UI
	private List<CharacterUI> _charactersUI = new List<CharacterUI>();
	
	// Characters that can be selected
	private List<CharacterData> _availableCharacters;
	
	// Every character that are selected
	[SerializeField] private List<CharacterData> _playersCharacter; 
	
	// Models sorted by names
	private Dictionary<string, GameObject> _charactersModel = new Dictionary<string, GameObject>();
	
	// Keeps track of the selected characters per player index
	private List<CharacterUI> _selectedCharacterUIs;

	private List<PlayerShowroom> _currentShowroomList = new List<PlayerShowroom>();
	
	private int _totalNbPlayers;

	private InputSystemUIInputModule _inputSystemUIInputModule;

	#region Unity Functions
	// Potentially Useless
	/*private void Update()
	{
		/*if (Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			GameObject go;

			if (Physics.Raycast(ray, out hit, float.PositiveInfinity, _characterUILayerMask)
				&& hit.collider.TryGetComponent<CharacterUI>(out CharacterUI characterUI)
				&& !characterUI.IsSelected)
			{
				if(characterUI != _selectableCharacters.Last())
					characterUI.SetSelected(true);

				_currentSelectedCharactersName[_playerIndex].text = characterUI.Character.Name;
				_currentSelectedCharacterBackground[_playerIndex].color = characterUI.Character.CharacterColor;
				_availableCharacters.Remove(characterUI.Character);

				if (_currentCharacterModelLocation[_playerIndex].childCount > 0)
				{
					_selectedCharacterUIs[_playerIndex].SetSelected(false);
					go = _currentCharacterModelLocation[_playerIndex].GetChild(0).gameObject;
					go.transform.SetParent(_charactersListTransform);
					go.transform.localPosition = Vector3.zero;
					go.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
					go.transform.localScale = Vector3.one;
					go.gameObject.SetActive(false);
					_availableCharacters.Add(_playersCharacter[_playerIndex]);
					_playersCharacter[_playerIndex] = null;
				}
				
				if(characterUI == _selectableCharacters.Last())
					_charactersModel.TryGetValue(characterUI.Character.Name + _playerIndex, out go);

				else
					_charactersModel.TryGetValue(characterUI.Character.Name, out go);
				go.transform.SetParent(_currentCharacterModelLocation[_playerIndex]);
				go.transform.localPosition = Vector3.zero;
				go.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
				go.transform.localScale = new Vector3(20, 20, 20);
				go.SetActive(true);
				_selectedCharacterUIs[_playerIndex] = characterUI;
				_playersCharacter[_playerIndex] = characterUI.Character;
				VerifyCharacters();
				_playerIndex = (_playerIndex + 1) % _totalNbPlayers;
			}
		}
	}*/

	private void Start()
	{
		_inputSystemUIInputModule = MenuManager.Instance.CurrentEventSystem.GetComponent<InputSystemUIInputModule>();
	}

	private void Update()
	{
		if (_inputSystemUIInputModule.cancel.action.WasPressedThisFrame())
			CloseMenu();
	}
	#endregion

	private void CloseMenu()
	{
		if (_aceItWindow.activeSelf)
		{
			_aceItWindow.SetActive(false);
			MenuManager.Instance.CurrentEventSystem.SetSelectedGameObject(null);
		}
		else
		{
			OnMenuDisabled();
			_controllerSelectionMenu.OnBackToControllerSelection();
			MenuManager.Instance.GoToPreviousMenu();
		}
	}

	#region Listeners

	// Button "play"
	public void Play()
	{
		TransformRandomSelectionInCharacter();
		StartCoroutine(WaitBeforeDisplayingAceItMenu());
		//MenuManager.Instance.CurrentEventSystem.SetSelectedGameObject(_confirmPlayButton);
	}

	// Button "ACE IT"
	public void OnConfirmPlay()
	{
		GameParameters.Instance.SetCharactersPlayers(_playersCharacter);
		
		_aceItWindow.SetActive(false);

		//Debug.Log("Let's Ace It !");

		ControllerManager.Instance.ChangeCtrlersActMapToGame();
		
		SceneManager.LoadScene("Local_Multiplayer");
	}
	
	// Display right number of showrooms whether it is a single or double match
	public void SetNumberOfShowrooms(bool isDouble)
	{
		if (isDouble)
		{
			_matchDoubleWindow.SetActive(true);
			_matchSingleWindow.SetActive(false);
			_totalNbPlayers = 4;
			_currentShowroomList = _characterShowroomsDouble;
		}
		else
		{
			_matchDoubleWindow.SetActive(false);
			_matchSingleWindow.SetActive(true);
			_totalNbPlayers = 2;
			_currentShowroomList = _characterShowroomsSingle;
		}

		_playersCharacter = new List<CharacterData>(new CharacterData[_totalNbPlayers]);
		_selectedCharacterUIs = new List<CharacterUI>(new CharacterUI[_totalNbPlayers]);
	}
	
	// Any button that loads the menu
	public void OnMenuLoaded()
	{
		AudioManager.Instance.PlaySfx("ChooseYourCharacter");
		_characters = MenuManager.Instance.Characters;
		_availableCharacters = new List<CharacterData>(_characters);
		_charactersModelsContainer = MenuManager.Instance.CharactersModelsParent;
		_charactersModel = MenuManager.Instance.CharactersModel;
		_aceItWindow.SetActive(false);

		SetNumberOfShowrooms(GameParameters.Instance.IsDouble);
		SetPlayerInfos();
		_playButton.interactable = false;

		foreach(var item in _characterShowroomsSingle)
		{
			item.CharacterEmblem.gameObject.SetActive(false);
		}
		
		foreach(var item in _characterShowroomsDouble)
		{
			item.CharacterEmblem.gameObject.SetActive(false);
		}

		foreach (var item in _characters)
		{
			CharacterUI charUI = Instantiate(_characterUIPrefab, _characterUIContainer).GetComponent<CharacterUI>();

			charUI.SetVisual(item);

			charUI.GetComponent<CharacterUI>().Character.Init();

			charUI.SetCharacterSelectionMenu(this);

			_charactersUI.Add(charUI);
		}
		
		SetTheRandomSelectionForBots();
	}

	// Any button that disables the menu
	public void OnMenuDisabled()
	{
		MenuUiReset();
		MenuVariablesReset();
		ControllerManager.Instance.ResetControllers();
	}

	#endregion

	// Used to set "ui" information above player's character
	private void SetPlayerInfos()
	{
		if(_totalNbPlayers == 2)
		{
			_characterShowroomsSingle[0].PlayerInfo.text = "P1";

			if (GameParameters.Instance.LocalNbPlayers == 1)
				_characterShowroomsSingle[1].PlayerInfo.text = "COM";
			else
				_characterShowroomsSingle[1].PlayerInfo.text = "P2";
		}
		else
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
	}

	// Used at the end of the selection when about to start the game, it transforms the "random character" into a character
	private void TransformRandomSelectionInCharacter()
	{
		for (int i = 0; i < _playersCharacter.Count; i++)
		{
			if (_playersCharacter[i] != null && _playersCharacter[i].Name == "Random")
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
		CharacterData cd = ReturnRandomCharacter();
		_charactersUI.Where(x => x.Character.Name == cd.Name).FirstOrDefault().SetSelected(true);
		_currentShowroomList[playerIndex].CharacterName.text = cd.Name;
		_currentShowroomList[playerIndex].Background.color = cd.CharacterPrimaryColor;
		_currentShowroomList[playerIndex].NameBackground.color = cd.CharacterSecondaryColor;
		_currentShowroomList[playerIndex].CharacterEmblem.gameObject.SetActive(true);
		_currentShowroomList[playerIndex].CharacterEmblem.sprite = cd.CharactersLogo;

		if (!_charactersModel.TryGetValue(cd.Name, out GameObject go))
			return;
			
		go.transform.SetParent(_currentShowroomList[playerIndex].ModelLocation);
		go.transform.localPosition = Vector3.zero;
		go.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
		go.transform.localScale = new Vector3(20, 20, 20);
		go.SetActive(true);
			
		_playersCharacter[playerIndex] = cd;
	}

	// Will load "random" model for each bot and assign the right variables for later transformations
	private void SetTheRandomSelectionForBots()
	{
		for (int i = _totalNbPlayers - 1; i > GameParameters.Instance.LocalNbPlayers - 1; i--)
		{
			// We select the "Random" Character UI so it's in Last
			CharacterUI characterUI = _charactersUI.Last();
			
			if (!_charactersModel.TryGetValue(characterUI.Character.Name + i, out var characterModel))
				return;
			
			_currentShowroomList[i].CharacterName.text = characterUI.Character.Name;
			_currentShowroomList[i].Background.color = characterUI.Character.CharacterPrimaryColor;
			_currentShowroomList[i].NameBackground.color = characterUI.Character.CharacterSecondaryColor;

			characterModel.transform.SetParent(_currentShowroomList[i].ModelLocation);
			characterModel.transform.localPosition = Vector3.zero;
			characterModel.transform.localRotation = Quaternion.Euler(new Vector3(characterUI.Character.Name == "Random" ? -90 : 0, 180, 0));
			characterModel.SetActive(true);
			_selectedCharacterUIs[i] = characterUI;
			_playersCharacter[i] = characterUI.Character;
		}
	}
	
	// Returns a random character from the _charactersUI List
	// If the character is already selected, it chooses another one
	private CharacterData ReturnRandomCharacter()
	{
		int randomIndex;
		do
		{
			randomIndex = Random.Range(0, _charactersUI.Count - 1); // Don't use the last character because it's the Random one
		} while (_charactersUI[randomIndex].IsSelected);

		return _characters[randomIndex];;
	}

	// This function removes any model, color or asset, previously selected on a player selection UI
	private bool RemoveCharacterFromPlayerSelectionUi(int playerIndex)
	{
		if (_currentShowroomList[playerIndex].ModelLocation.childCount <= 0)
			return false;
		
		_selectedCharacterUIs[playerIndex].SetSelected(false);
		_selectedCharacterUIs[playerIndex] = null;
		
		GameObject oldGo = _currentShowroomList[playerIndex].ModelLocation.GetChild(0).gameObject;
		oldGo.transform.SetParent(_characterUIContainer);
		oldGo.transform.localPosition = Vector3.zero;
		oldGo.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
		oldGo.gameObject.SetActive(false);
		
		_playersCharacter[playerIndex] = null;
		return true;
	}
	
	private void SetPlayButtonInteractability()
	{
		//_playButton.interactable = IsEveryCharSelectedByLocals();
		if (IsEveryCharSelectedByLocals())
			Play();
	}

	// Check if every local player selected his 
	private bool IsEveryCharSelectedByLocals()
	{
		for (int i = 0; i < GameParameters.Instance.LocalNbPlayers; i++)
		{
			if (_playersCharacter[i] == null)
				return false;
		}
		
		return true;
	}
	
	// Resets every UI and intern variables
	private void MenuUiReset()
	{
		foreach(var item in _charactersModel)
		{
			item.Value.transform.SetParent(_charactersModelsContainer);
			item.Value.transform.localPosition = Vector3.zero;
			item.Value.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
			item.Value.gameObject.SetActive(false);
		}

		foreach (var item in _currentShowroomList)
		{
			item.Background.color = Color.white;
			item.NameBackground.color = Color.black;
			item.CharacterName.text = "";
			item.CharacterEmblem.sprite = null;
			item.CharacterEmblem.gameObject.SetActive(false);
		}

		for (int i = 0; i < _playersCharacter.Count; i++)
		{
			_playersCharacter[i] = null;
		}

		foreach(var item in _charactersUI)
		{
			item.SetSelected(false);
		}

		_playButton.interactable = false;
		_aceItWindow.SetActive(false);
	}

	private void MenuVariablesReset()
	{
		_characters = new List<CharacterData>();
		_availableCharacters = new List<CharacterData>();
		_charactersModelsContainer = null;
		_charactersModel = new Dictionary<string, GameObject>();

		for (int i = 0; i < _charactersUI.Count; i++)
		{
			Destroy(_charactersUI[i].gameObject);
			_charactersUI[i] = null;
		}
		
		_charactersUI.Clear();
		_charactersUI = new List<CharacterUI>();
	}

	#region COROUTINES

	private IEnumerator WaitBeforeDisplayingAceItMenu()
	{
		yield return new WaitForSeconds(1f);

		_aceItWindow.SetActive(true);
		AudioManager.Instance.PlaySfx("AceItSound");
	}

	#endregion

	// Potentially useless
	/*
	public void HandleCharacterSelectionInput(CharacterUI characterUI)
	{
		if (_keyboardPlayerIndex == -1)
			return;
		
		if (!characterUI.IsSelected)
		{
			characterUI.SetSelected(true);
			_currentSelectedCharactersName[_keyboardPlayerIndex].text = characterUI.Character.Name;
			_currentSelectedCharacterBackground[_keyboardPlayerIndex].color = characterUI.Character.CharacterColor;

			if (_currentCharacterModelLocation[_keyboardPlayerIndex].childCount > 0)
			{
				_selectedCharacterUIs[_keyboardPlayerIndex].SetSelected(false);

				GameObject previousCharacter = _currentCharacterModelLocation[_keyboardPlayerIndex].GetChild(0).gameObject;
				previousCharacter.transform.SetParent(_charactersListTransform);
				previousCharacter.transform.localPosition = Vector3.zero;
				previousCharacter.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
				previousCharacter.SetActive(false);
				_playersCharacter[_keyboardPlayerIndex] = null;
			}

			GameObject characterModel;
			if (_charactersModel.TryGetValue(characterUI.Character.Name, out characterModel))
			{
				characterModel.transform.SetParent(_currentCharacterModelLocation[_keyboardPlayerIndex]);
				characterModel.transform.localPosition = Vector3.zero;
				characterModel.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
				characterModel.SetActive(true);
				_selectedCharacterUIs[_keyboardPlayerIndex] = characterUI;
				_playersCharacter[_keyboardPlayerIndex] = characterUI.Character;
				VerifyCharacters();
				_keyboardPlayerIndex = (_keyboardPlayerIndex + 1) % _totalNbPlayers;
			}
			else
			{
				Debug.LogError("Character model not found for: " + characterUI.Character.Name);
			}
		}
	}*/

	#region Called Externally
	public override bool HandleCharacterSelectionInput(Ray ray, int playerIndex)
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

			if (_charactersModel.TryGetValue(charNameToLookFor, out var characterModel))
			{
				characterModel.transform.SetParent(_currentShowroomList[playerIndex].ModelLocation);
				characterModel.transform.localPosition = Vector3.zero;
				characterModel.transform.localRotation = Quaternion.Euler(new Vector3(characterUI.Character.Name == "Random" ? -90 : 0, 180, 0));
				characterModel.transform.localScale = new Vector3(20, 20, 20);
				characterModel.SetActive(true);
				_selectedCharacterUIs[playerIndex] = characterUI;
				_playersCharacter[playerIndex] = characterUI.Character;
				SetPlayButtonInteractability();
				return true;
			}
			
			Debug.LogError("Character model not found for: " + characterUI.Character.Name);
		}
		return false;
	}

	public override bool HandleCharacterDeselectionInput(int playerIndex)
	{
		return RemoveCharacterFromPlayerSelectionUi(playerIndex);
	}
	#endregion
}
