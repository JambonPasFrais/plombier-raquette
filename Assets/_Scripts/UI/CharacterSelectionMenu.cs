using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectionMenu : MonoBehaviour
{
    [SerializeField] private GameObject _characterUIPrefab;
	[SerializeField] private List<CharacterData> _characters = new List<CharacterData>();
	[SerializeField] private Transform _charactersListTransform;
	[SerializeField] private List<TextMeshProUGUI> _selectedCharactersName = new List<TextMeshProUGUI>();
	[SerializeField] private List<Transform> _characterModelLocation = new List<Transform>();
	[SerializeField] private List<Image> _selectedCharacterBackground = new List<Image>();
	[SerializeField] private LayerMask _characterUILayerMask;
	[SerializeField] private Transform _charactersModelsParent;
	private Dictionary<string, GameObject> _charactersModel = new Dictionary<string, GameObject>();
	private List<CharacterData> _playersCharacter = new List<CharacterData>(new CharacterData[4]);
	private List<CharacterUI> _selectedCharacterUIs = new List<CharacterUI>(new CharacterUI[4]);
	private int _playerIndex = 0;

	private void Start()
	{
		GameObject go;
		foreach (var item in _characters)
		{
			go = Instantiate(_characterUIPrefab, _charactersListTransform);
			go.GetComponent<CharacterUI>().SetVisual(item);
		}

		foreach(var item in _characters)
		{
			go = Instantiate(item.Model3D, _charactersModelsParent);
			go.name = item.Name;
			go.SetActive(false);
			_charactersModel.Add(item.Name, go);
		}
	}

	private void Update()
	{
		if(Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			GameObject go;

			if (Physics.Raycast(ray, out hit, float.PositiveInfinity, _characterUILayerMask) 
				&& hit.collider.TryGetComponent<CharacterUI>(out CharacterUI characterUI)
				&& !characterUI.IsSelected)
			{
				characterUI.SetSelected(true);
				_selectedCharactersName[_playerIndex].text = characterUI.Character.Name;
				_selectedCharacterBackground[_playerIndex].color = characterUI.Character.CharacterColor;

				if (_characterModelLocation[_playerIndex].childCount > 0)
				{
					_selectedCharacterUIs[_playerIndex].SetSelected(false);
					go = _characterModelLocation[_playerIndex].GetChild(0).gameObject;
					go.transform.SetParent(_charactersListTransform);
					go.transform.localPosition = Vector3.zero;
					go.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
					go.gameObject.SetActive(false);
					_playersCharacter[_playerIndex] = null;
				}

				_charactersModel.TryGetValue(characterUI.Character.Name, out go);
				go.transform.SetParent(_characterModelLocation[_playerIndex]);
				go.transform.localPosition = Vector3.zero;
				go.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
				go.SetActive(true);
				_selectedCharacterUIs[_playerIndex] = characterUI;
				_playersCharacter[_playerIndex] = characterUI.Character;
				_playerIndex = (_playerIndex + 1) % 4;
			}
		}
	}

	public void Play()
	{
		GameParameters.Instance.SetCharactersPlayers(_playersCharacter);
		SceneManager.LoadScene(1);
	}
}
