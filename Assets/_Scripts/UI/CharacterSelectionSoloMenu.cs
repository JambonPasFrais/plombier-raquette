using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectionSoloMenu : MonoBehaviour
{
	[SerializeField] private GameObject _characterUIPrefab;
	[SerializeField] private List<CharacterData> _characters = new List<CharacterData>();
	[SerializeField] private Transform _charactersListTransform;
	[SerializeField] private TextMeshProUGUI _selectedCharactersName;
	[SerializeField] private Transform _characterModelLocation;
	[SerializeField] private Image _selectedCharacterBackground;
	[SerializeField] private LayerMask _characterUILayerMask;
	[SerializeField] private Transform _charactersModelsParent;
	[SerializeField] private Button _playButton;
	private List<CharacterData> _availableCharacters;
	private Dictionary<string, GameObject> _charactersModel = new Dictionary<string, GameObject>();
	private CharacterData _playerCharacter;
	private CharacterUI _selectedCharacterUIs;
	private List<CharacterUI> _selectableCharacters = new List<CharacterUI>();

	private void Start()
	{
		_characters = MenuManager.Instance.Characters;
		_charactersModelsParent = MenuManager.Instance.CharactersModelsParent;
		_charactersModel = MenuManager.Instance.CharactersModel;
		_availableCharacters = new List<CharacterData>(_characters);

        _playButton.interactable = false;
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
				_selectedCharactersName.text = characterUI.Character.Name;
				_selectedCharacterBackground.color = characterUI.Character.CharacterBackgroundColor;

				if (_characterModelLocation.childCount > 0)
				{
					_selectedCharacterUIs.SetSelected(false);

					go = _characterModelLocation.GetChild(0).gameObject;
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

				go.transform.SetParent(_characterModelLocation);
				go.transform.localPosition = Vector3.zero;
				go.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
				go.transform.localScale = new Vector3(20, 20, 20);
				go.SetActive(true);
				_selectedCharacterUIs = characterUI;
				_playerCharacter = characterUI.Character;

			}

            VerifyCharacters();
        }
	}

	private void VerifyCharacters()
	{
		if (_playerCharacter)
			_playButton.interactable = true;
		else
			_playButton.interactable = false;
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

		_selectedCharacterBackground.color = Color.black;
		_selectedCharactersName.text = "";
		_playerCharacter = null;

		foreach (var item in _selectableCharacters)
		{
			item.SetSelected(false);
		}

		_playButton.interactable = false;
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
