using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectionSoloMenu : MonoBehaviour
{
	[Header("Instances")]
	// Character UI related
	[SerializeField] private GameObject _characterUIPrefab;
	[SerializeField] private Transform _characterUIContainer;
	[SerializeField] private LayerMask _characterUILayerMask;
	// Visual of Player References
	[SerializeField] private PlayerShowroom _playerShowroom;
	// Where the model are in not selected -> pooling optimisation technique
	[SerializeField] private Transform _charactersModelsContainer;
	[SerializeField] private Button _playButton;

	// All Characters Data
	private List<CharacterData> _characters = new List<CharacterData>();

	// All Characters Models
	private Dictionary<string, GameObject> _charactersModel = new Dictionary<string, GameObject>();

	// Character selected by the player
	private CharacterData _playerCharacter;

	// Reference to the previous selected character UI if player change his character to play with
	private CharacterUI _previousSelectedCharacterUI;

	// All characters UI
	private List<CharacterUI> _charactersUI = new List<CharacterUI>();

	private void Start()
	{
		_characters = MenuManager.Instance.Characters;
		_charactersModelsContainer = MenuManager.Instance.CharactersModelsParent;
		_charactersModel = MenuManager.Instance.CharactersModel;
		_playButton.interactable = false;

		GameObject go;

		foreach (var item in _characters)
		{
			go = Instantiate(_characterUIPrefab, _characterUIContainer);
			go.GetComponent<CharacterUI>().SetVisual(item);
            go.GetComponent<CharacterUI>().SetCharacterSelectionSoloMenu(this);
            _charactersUI.Add(go.GetComponent<CharacterUI>());
		}
	}

	private void Update()
	{
		// Set visual and selection when player selected his character
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			GameObject go;

			if (Physics.Raycast(ray, out hit, float.PositiveInfinity, _characterUILayerMask)
				&& hit.collider.TryGetComponent<CharacterUI>(out CharacterUI characterUI)
				&& !characterUI.IsSelected)
			{
				if (characterUI != _charactersUI.Last())
					characterUI.SetSelected(true);
				_playerShowroom.CharacterName.text = characterUI.Character.Name;
				_playerShowroom.Background.color = characterUI.Character.CharacterPrimaryColor;
				_playerShowroom.NameBackground.color = characterUI.Character.CharacterSecondaryColor;
				_playerShowroom.CharacterEmblem.sprite = characterUI.Character.CharactersLogo;

				if (_playerShowroom.ModelLocation.childCount > 0)
				{
					_previousSelectedCharacterUI.SetSelected(false);

					go = _playerShowroom.ModelLocation.GetChild(0).gameObject;
					go.transform.SetParent(_characterUIContainer);
					go.transform.localPosition = Vector3.zero;
					go.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
					go.transform.localScale = Vector3.one;
					go.gameObject.SetActive(false);
					_playerCharacter = null;
				}

				if (characterUI == _charactersUI.Last())
					_charactersModel.TryGetValue(characterUI.Character.Name + "0", out go);

				else
					_charactersModel.TryGetValue(characterUI.Character.Name, out go);

				go.transform.SetParent(_playerShowroom.ModelLocation);
				go.transform.localPosition = Vector3.zero;
				go.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
				go.transform.localScale = new Vector3(20, 20, 20);
				go.SetActive(true);
				_previousSelectedCharacterUI = characterUI;
				_playerCharacter = characterUI.Character;
				_playButton.interactable = true;
			}
		}
	}

	// Verify if the player has selected a character -> Useless we only have one player so do it when we know he selected a character
	/*private void VerifyCharacters()
	{
		if (_playerCharacter)
		{
			_aceItWindow.SetActive(true);
			EventSystem.current.SetSelectedGameObject(_playButton);
		}
	}*/

	// Reset the menu visual and variables when we came back to the this menu to avoid any problem
	public void ResetMenu()
	{
		foreach (var item in _charactersModel)
		{
			item.Value.transform.SetParent(_charactersModelsContainer);
			item.Value.transform.localPosition = Vector3.zero;
			item.Value.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
			item.Value.gameObject.SetActive(false);
		}

		_playerShowroom.Background.color = Color.white;
		_playerShowroom.NameBackground.color = Color.black;
		_playerShowroom.CharacterName.text = "";
		_playerShowroom.CharacterEmblem.sprite = null;
		_playerCharacter = null;

		foreach (var item in _charactersUI)
		{
			item.SetSelected(false);
		}

		_playButton.interactable = false;
	}

	// Button play that will make the random character selection and send player Character to Game Parameters 
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
