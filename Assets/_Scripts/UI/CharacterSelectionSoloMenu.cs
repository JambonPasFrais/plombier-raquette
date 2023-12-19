using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectionSoloMenu : MonoBehaviour
{
	[SerializeField] private GameObject _characterUIPrefab;
	[SerializeField] private List<CharacterData> _characters = new List<CharacterData>();
	[SerializeField] private PlayerShowroom _playerShowroom;
	[SerializeField] private Transform _charactersListTransform;
	[SerializeField] private LayerMask _characterUILayerMask;
	[SerializeField] private Transform _charactersModelsParent;
	[SerializeField] private GameObject _aceItMenu;
	[SerializeField] private GameObject _playButton;

	private List<CharacterData> _availableCharacters;
	private Dictionary<string, GameObject> _charactersModel = new Dictionary<string, GameObject>();
	private CharacterData _playerCharacter;
	private CharacterUI _selectedCharacterUIs;
	private List<CharacterUI> _selectableCharacters = new List<CharacterUI>();

	private void Start()
	{
		_characters = MenuManager.Characters;
		_charactersModelsParent = MenuManager.CharactersModelsParent;
		_charactersModel = MenuManager.CharactersModel;
		_availableCharacters = new List<CharacterData>(_characters);

		VerifyCharacters();
		GameObject go;

		foreach (var item in _characters)
		{
			go = Instantiate(_characterUIPrefab, _charactersListTransform);
			go.GetComponent<CharacterUI>().SetVisual(item);
            go.GetComponent<CharacterUI>().SetCharacterSelectionSoloMenu(this);
            _selectableCharacters.Add(go.GetComponent<CharacterUI>());
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
				if (characterUI != _selectableCharacters.Last())
					characterUI.SetSelected(true);
				_playerShowroom.CharacterName.text = characterUI.Character.Name;
				_playerShowroom.Background.color = characterUI.Character.CharacterPrimaryColor;
				_playerShowroom.NameBackground.color = characterUI.Character.CharacterSecondaryColor;
				_playerShowroom.CharacterEmblem.sprite = characterUI.Character.CharactersLogo;

				if (_playerShowroom.ModelLocation.childCount > 0)
				{
					_selectedCharacterUIs.SetSelected(false);

					go = _playerShowroom.ModelLocation.GetChild(0).gameObject;
					go.transform.SetParent(_charactersListTransform);
					go.transform.localPosition = Vector3.zero;
					go.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
					go.transform.localScale = Vector3.one;
					go.gameObject.SetActive(false);
					_playerCharacter = null;
				}

				if (characterUI == _selectableCharacters.Last())
					_charactersModel.TryGetValue(characterUI.Character.Name + "0", out go);

				else
					_charactersModel.TryGetValue(characterUI.Character.Name, out go);

				go.transform.SetParent(_playerShowroom.ModelLocation);
				go.transform.localPosition = Vector3.zero;
				go.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
				go.transform.localScale = new Vector3(20, 20, 20);
				go.SetActive(true);
				_selectedCharacterUIs = characterUI;
				_playerCharacter = characterUI.Character;
				VerifyCharacters();
			}
		}
	}

	private void VerifyCharacters()
	{
		/*if (_playerCharacter)
			_playButton.interactable = true;

		else
			_playButton.interactable = false;*/

		_aceItMenu.SetActive(true);
		EventSystem.current.SetSelectedGameObject(_playButton);
	}

	public void ResetMenu()
	{
		foreach (var item in _charactersModel)
		{
			item.Value.transform.SetParent(_charactersModelsParent);
			item.Value.transform.localPosition = Vector3.zero;
			item.Value.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
			item.Value.gameObject.SetActive(false);
		}

		_playerShowroom.Background.color = Color.white;
		_playerShowroom.NameBackground.color = Color.black;
		_playerShowroom.CharacterName.text = "";
		_playerShowroom.CharacterEmblem.sprite = null;
		_playerCharacter = null;

		foreach (var item in _selectableCharacters)
		{
			item.SetSelected(false);
		}

		_aceItMenu.SetActive(false);
	}

	public void Play()
	{
		System.Random random = new System.Random();

		if (_playerCharacter == _characters.Last())
			_playerCharacter = _characters[random.Next(_characters.Count - 1)];

		GameParameters.Instance.SetCharactersPlayers(new List<CharacterData>() { _playerCharacter });
	}

	public void HandleCharacterSelectionSoloMenu(CharacterUI characterUI)
	{
		/*if (!characterUI.IsSelected)
		{
            if (characterUI != _selectableCharacters.Last())
				characterUI.SetSelected(true);
			_selectedCharactersName.text = characterUI.Character.Name;
			_selectedCharacterBackground.color = characterUI.Character.CharacterColor;

			if (_characterModelLocation.childCount > 0)
			{
				_selectedCharacterUIs.SetSelected(false);

				GameObject previousCharacter = _characterModelLocation.GetChild(0).gameObject;
				previousCharacter.transform.SetParent(_charactersListTransform);
				previousCharacter.transform.localPosition = Vector3.zero;
				previousCharacter.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
				previousCharacter.SetActive(false);
				_playerCharacter = null;
			}

			GameObject characterModel;
            if (_charactersModel.TryGetValue(characterUI.Character.Name, out characterModel))
            {
                characterModel.transform.SetParent(_characterModelLocation);
                characterModel.transform.localPosition = Vector3.zero;
                characterModel.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
                characterModel.SetActive(true);
                _selectedCharacterUIs = characterUI;
                _playerCharacter = characterUI.Character;
                VerifyCharacters();
            }
            else
			{
				Debug.LogError("Character model not found for: " + characterUI.Character.Name);
			}
		}*/
	}
}
