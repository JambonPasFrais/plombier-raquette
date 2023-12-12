using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectionMenu : MonoBehaviour
{
	[Header("Instances")] [SerializeField] private GameObject _aceItWindow;
	[SerializeField] private GameObject _characterUIPrefab;
	[SerializeField] private List<CharacterData> _characters = new List<CharacterData>();
	[SerializeField] private Transform _charactersListTransform;
	private List<TextMeshProUGUI> _currentSelectedCharactersName = new List<TextMeshProUGUI>();
	[SerializeField] private List<TextMeshProUGUI> _selectedCharactersNameSingle = new List<TextMeshProUGUI>();
	[SerializeField] private List<TextMeshProUGUI> _selectedCharactersNameDouble = new List<TextMeshProUGUI>();
	private List<Transform> _currentCharacterModelLocation = new List<Transform>();
	[SerializeField] private List<Transform> _characterModelLocationSimple = new List<Transform>();
	[SerializeField] private List<Transform> _characterModelLocationDouble = new List<Transform>();
	private List<Image> _currentSelectedCharacterBackground = new List<Image>();
	[SerializeField] private List<Image> _selectedCharacterBackgroundSingle = new List<Image>();
	[SerializeField] private List<Image> _selectedCharacterBackgroundDouble = new List<Image>();
	[SerializeField] private LayerMask _characterUILayerMask;
	[SerializeField] private Transform _charactersModelsParent;
	[SerializeField] private Button _playButton;
	[SerializeField] private GameObject _playersShowRoomSingle;
	[SerializeField] private GameObject _playersShowRoomDouble;
	[SerializeField] private List<TextMeshProUGUI> _playersInfoSingle = new List<TextMeshProUGUI>();
	[SerializeField] private List<TextMeshProUGUI> _playersInfoDouble = new List<TextMeshProUGUI>();
	private List<CharacterData> _availableCharacters;
	private Dictionary<string, GameObject> _charactersModel = new Dictionary<string, GameObject>();
	private List<CharacterData> _playersCharacter; // Every character that are selected
	private List<CharacterUI> _selectedCharacterUIs;
	private List<CharacterUI> _selectableCharacters = new List<CharacterUI>();
	private int _keyboardPlayerIndex;
	private int _totalNbPlayers;

	#region Unity Functions
	private void Start()
	{
		// Init
		_aceItWindow.SetActive(false);
		_characters = MenuManager.Characters;
		_charactersModelsParent = MenuManager.CharactersModelsParent;
		_charactersModel = MenuManager.CharactersModel;

		SetPlayButtonInteractability();

		// Create the character's GameObjects
		GameObject go;

		foreach (var item in _characters)
		{
			go = Instantiate(_characterUIPrefab, _charactersListTransform);

			go.GetComponent<CharacterUI>().SetVisual(item);

			go.GetComponent<CharacterUI>().setCharacterSelectionMenu(this);

			_selectableCharacters.Add(go.GetComponent<CharacterUI>());
		}
	}

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
				go.SetActive(true);
				_selectedCharacterUIs[_playerIndex] = characterUI;
				_playersCharacter[_playerIndex] = characterUI.Character;
				VerifyCharacters();
				_playerIndex = (_playerIndex + 1) % _totalNbPlayers;
			}
		}
	}*/
	#endregion

	#region Listeners
	
	// Button "play"
	public void Play()
	{
		TransformRandomSelectionInCharacter();
		SetCharacterForBots();
		_aceItWindow.SetActive(true);
	}

	// Button "ACE IT"
	public void OnConfirmPlay()
	{
		GameParameters.Instance.SetCharactersPlayers(_playersCharacter);
		Debug.Log("Go to play");
		_aceItWindow.SetActive(false);
		
		//TODO : launch local scene
	}
	
	// Button "validation" from the rules menu
	public void SetNumberOfShowrooms(bool isDouble)
	{
		if (isDouble)
		{
			_playersShowRoomDouble.SetActive(true);
			_playersShowRoomSingle.SetActive(false);
			_totalNbPlayers = 4;
			_currentCharacterModelLocation = _characterModelLocationDouble;
			_currentSelectedCharacterBackground = _selectedCharacterBackgroundDouble;
			_currentSelectedCharactersName = _selectedCharactersNameDouble;
		}
		else
		{
			_playersShowRoomDouble.SetActive(false);
			_playersShowRoomSingle.SetActive(true);
			_totalNbPlayers = 2;
			_currentCharacterModelLocation = _characterModelLocationSimple;
			_currentSelectedCharacterBackground = _selectedCharacterBackgroundSingle;
			_currentSelectedCharactersName = _selectedCharactersNameSingle;
		}

		_playersCharacter = new List<CharacterData>(new CharacterData[_totalNbPlayers]);
		_selectedCharacterUIs = new List<CharacterUI>(new CharacterUI[_totalNbPlayers]);
	}
	
	// Any button that loads the menu
	public void OnMenuLoaded()
	{
		SetPlayerInfos();
		_availableCharacters = new List<CharacterData>(_characters);
	}

	// Any button that disables the menu
	public void OnMenuDisabled()
	{
		MenuUiReset();
		ControllerManager.Instance.ResetControllers();
	}

	#endregion

	// Used to set "ui" information above player's character
	private void SetPlayerInfos()
	{
		if(_totalNbPlayers == 2)
		{
			_playersInfoSingle[0].text = "P1";

			if (GameParameters.LocalNbPlayers == 1)
				_playersInfoSingle[1].text = "COM";
			else
				_playersInfoSingle[1].text = "P2";
		}
		else
		{
			_playersInfoDouble[0].text = "P1";

			for(int i = 1; i < 4; i++)
			{
				if (i < GameParameters.LocalNbPlayers)
					_playersInfoDouble[i].text = "P" + (i + 1).ToString();

				else
					_playersInfoDouble[i].text = "COM";
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
	
	// Used at the of the selection, it transforms each bot random character into a character
	private void SetCharacterForBots()
	{

		for (int i = _totalNbPlayers - 1; i > GameParameters.LocalNbPlayers - 1; i--)
		{
			SetRandomCharacterForSpecifiedPlayer(i);
		}
	}

	private void SetRandomCharacterForSpecifiedPlayer(int playerIndex)
	{
		CharacterData cd = ReturnRandomCharacter();
		_currentSelectedCharactersName[playerIndex].text = cd.Name;
		_currentSelectedCharacterBackground[playerIndex].color = cd.CharacterColor;

		if (!_charactersModel.TryGetValue(cd.Name, out GameObject go))
			return;
			
		go.transform.SetParent(_currentCharacterModelLocation[playerIndex]);
		go.transform.localPosition = Vector3.zero;
		go.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
		go.SetActive(true);
			
		_playersCharacter[playerIndex] = cd;
	}
	
	// Returns a random character from the availableCharacters List and deletes it from the list
	private CharacterData ReturnRandomCharacter()
	{
		System.Random rand = new System.Random();

		int currentIndex = rand.Next(_availableCharacters.Count);
		CharacterData data = _availableCharacters[currentIndex];
		_availableCharacters.RemoveAt(currentIndex);

		return data;
	}
	
	private void SetPlayButtonInteractability()
	{
		_playButton.interactable = IsEveryCharSelectedByLocals();
	}

	// Check if every local player selected his 
	private bool IsEveryCharSelectedByLocals()
	{
		for (int i = 0; i < GameParameters.LocalNbPlayers; i++)
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
			item.Value.transform.SetParent(_charactersModelsParent);
			item.Value.transform.localPosition = Vector3.zero;
			item.Value.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
			item.Value.gameObject.SetActive(false);
		}

		foreach (var item in _currentSelectedCharacterBackground)
		{
			item.color = Color.black;
		}

		foreach (var item in _currentSelectedCharactersName)
		{
			item.text = "";
		}

		for (int i = 0; i < _playersCharacter.Count; i++)
		{
			_playersCharacter[i] = null;
		}

		foreach(var item in _selectableCharacters)
		{
			item.SetSelected(false);
		}

		_playButton.interactable = false;
	}
	
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
	public bool HandleCharacterSelectionInput(Ray ray, int playerIndex)
	{
		if (Physics.Raycast(ray, out var hit, float.PositiveInfinity, _characterUILayerMask)
		    && hit.collider.TryGetComponent(out CharacterUI characterUI)
		    && !characterUI.IsSelected)
		{
			characterUI.SetSelected(true);
			_currentSelectedCharactersName[playerIndex].text = characterUI.Character.Name;
			_currentSelectedCharacterBackground[playerIndex].color = characterUI.Character.CharacterColor;

			if (_charactersModel.TryGetValue(characterUI.Character.Name, out var characterModel))
			{
				characterModel.transform.SetParent(_currentCharacterModelLocation[playerIndex]);
				characterModel.transform.localPosition = Vector3.zero;
				characterModel.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
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

	public bool HandleCharacterDeselectionInput(int playerIndex)
	{
		if (_currentCharacterModelLocation[playerIndex].childCount > 0)
		{
			_selectedCharacterUIs[playerIndex].SetSelected(false);
			GameObject go = _currentCharacterModelLocation[playerIndex].GetChild(0).gameObject;
			go.transform.SetParent(_charactersListTransform);
			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
			go.gameObject.SetActive(false);
			_playersCharacter[playerIndex] = null;
			return true;
		}
		return false;
	}
	#endregion
}
