using System.Collections;
using System.Collections.Generic;
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
	private Dictionary<string, GameObject> _charactersModel = new Dictionary<string, GameObject>();
	private CharacterData _playerCharacter;
	private CharacterUI _selectedCharacterUIs;
	private List<CharacterUI> _selectableCharacters = new List<CharacterUI>();

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
				_selectedCharactersName.text = characterUI.Character.Name;
				_selectedCharacterBackground.color = characterUI.Character.CharacterColor;

				if (_characterModelLocation.childCount > 0)
				{
					_selectedCharacterUIs.SetSelected(false);
					go = _characterModelLocation.GetChild(0).gameObject;
					go.transform.SetParent(_charactersListTransform);
					go.transform.localPosition = Vector3.zero;
					go.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
					go.gameObject.SetActive(false);
					_playerCharacter = null;
				}

				_charactersModel.TryGetValue(characterUI.Character.Name, out go);
				go.transform.SetParent(_characterModelLocation);
				go.transform.localPosition = Vector3.zero;
				go.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
				go.SetActive(true);
				_selectedCharacterUIs = characterUI;
				_playerCharacter = characterUI.Character;
				VerifyCharacters();
			}
		}
	}

	private void VerifyCharacters()
	{
		if(_playerCharacter)
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
		GameParameters.Instance.SetCharactersPlayers(new List<CharacterData>() { _playerCharacter });
	}
}
