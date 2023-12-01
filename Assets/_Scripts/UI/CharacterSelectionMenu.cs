using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectionMenu : MonoBehaviour
{
	[SerializeField] private GameObject _characterUIPrefab;
	[SerializeField] private List<CharacterData> _characters = new List<CharacterData>();
	[SerializeField] private Transform _charactersListTransform;
	private List<TextMeshProUGUI> _currentSelectedCharactersName = new List<TextMeshProUGUI>();
	[SerializeField] private List<TextMeshProUGUI> _selectedCharactersNameSimple = new List<TextMeshProUGUI>();
	[SerializeField] private List<TextMeshProUGUI> _selectedCharactersNameDouble = new List<TextMeshProUGUI>();
	private List<Transform> _currentCharacterModelLocation = new List<Transform>();
	[SerializeField] private List<Transform> _characterModelLocationSimple = new List<Transform>();
	[SerializeField] private List<Transform> _characterModelLocationDouble = new List<Transform>();
	private List<Image> _currentSelectedCharacterBackground = new List<Image>();
	[SerializeField] private List<Image> _selectedCharacterBackgroundSimple = new List<Image>();
	[SerializeField] private List<Image> _selectedCharacterBackgroundDouble = new List<Image>();
	[SerializeField] private LayerMask _characterUILayerMask;
	[SerializeField] private Transform _charactersModelsParent;
	[SerializeField] private Button _playButton;
	[SerializeField] private GameObject _playersShowRoomSimple;
	[SerializeField] private GameObject _playersShowRoomDouble;
	private Dictionary<string, GameObject> _charactersModel = new Dictionary<string, GameObject>();
	private List<CharacterData> _playersCharacter;
	private List<CharacterUI> _selectedCharacterUIs;
	private List<CharacterUI> _selectableCharacters = new List<CharacterUI>();
	private int _playerIndex = 0;
	private int _nbOfPlayers;

	private void Start()
	{
		VerifyCharacters();
		GameObject go;

		foreach (var item in _characters)
		{
			go = Instantiate(_characterUIPrefab, _charactersListTransform);
			go.GetComponent<CharacterUI>().SetVisual(item);
			_selectableCharacters.Add(go.GetComponent<CharacterUI>());
		}

		foreach (var item in _characters)
		{
			go = Instantiate(item.Model3D, _charactersModelsParent);
			go.name = item.Name;
			go.SetActive(false);
			_charactersModel.Add(item.Name, go);
		}
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			GameObject go;

			if (Physics.Raycast(ray, out hit, float.PositiveInfinity, _characterUILayerMask)
				&& hit.collider.TryGetComponent<CharacterUI>(out CharacterUI characterUI)
				&& !characterUI.IsSelected)
			{
				characterUI.SetSelected(true);
				_currentSelectedCharactersName[_playerIndex].text = characterUI.Character.Name;
				_currentSelectedCharacterBackground[_playerIndex].color = characterUI.Character.CharacterColor;

				if (_currentCharacterModelLocation[_playerIndex].childCount > 0)
				{
					_selectedCharacterUIs[_playerIndex].SetSelected(false);
					go = _currentCharacterModelLocation[_playerIndex].GetChild(0).gameObject;
					go.transform.SetParent(_charactersListTransform);
					go.transform.localPosition = Vector3.zero;
					go.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
					go.gameObject.SetActive(false);
					_playersCharacter[_playerIndex] = null;
				}

				_charactersModel.TryGetValue(characterUI.Character.Name, out go);
				go.transform.SetParent(_currentCharacterModelLocation[_playerIndex]);
				go.transform.localPosition = Vector3.zero;
				go.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
				go.SetActive(true);
				_selectedCharacterUIs[_playerIndex] = characterUI;
				_playersCharacter[_playerIndex] = characterUI.Character;
				VerifyCharacters();
				_playerIndex = (_playerIndex + 1) % _nbOfPlayers;
			}
		}
	}

	public void Play()
	{
		GameParameters.Instance.SetCharactersPlayers(_playersCharacter);
		SceneManager.LoadScene(1);
	}

	private void VerifyCharacters()
	{
		foreach (var item in _playersCharacter)
		{
			if (item == null)
				return;
		}

		_playButton.interactable = true;
	}

	public void SetNumberOfShowrooms(bool isDouble)
	{
		if (isDouble)
		{
			_playersShowRoomDouble.SetActive(true);
			_playersShowRoomSimple.SetActive(false);
			_nbOfPlayers = 4;
			_currentCharacterModelLocation = _characterModelLocationDouble;
			_currentSelectedCharacterBackground = _selectedCharacterBackgroundDouble;
			_currentSelectedCharactersName = _selectedCharactersNameDouble;
		}
		else
		{
			_playersShowRoomDouble.SetActive(false);
			_playersShowRoomSimple.SetActive(true);
			_nbOfPlayers = 2;
			_currentCharacterModelLocation = _characterModelLocationSimple;
			_currentSelectedCharacterBackground = _selectedCharacterBackgroundSimple;
			_currentSelectedCharactersName = _selectedCharactersNameSimple;
		}

		_playersCharacter = new List<CharacterData>(new CharacterData[_nbOfPlayers]);
		_selectedCharacterUIs = new List<CharacterUI>(new CharacterUI[_nbOfPlayers]);
	}

	public void ResetMenu()
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
}
