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
	private List<CharacterData> _playersCharacter;
	private int _playerIndex = 0;

	private void Start()
	{
		_playersCharacter = new List<CharacterData>(new CharacterData[4]);
		foreach (var item in _characters)
		{
			GameObject go = Instantiate(_characterUIPrefab, _charactersListTransform);
			go.GetComponent<CharacterUI>().SetVisual(item);
		}

		for (int i = 0; i < _charactersModelsParent.childCount; i++)
		{
			_charactersModel.Add(_charactersModelsParent.GetChild(i).name, _charactersModelsParent.GetChild(i).gameObject);
		}
	}

	private void Update()
	{
		if(Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, float.PositiveInfinity, _characterUILayerMask) && hit.collider.TryGetComponent<CharacterUI>(out CharacterUI characterUI))
			{
				_selectedCharactersName[_playerIndex].text = characterUI.Character.Name;
				_selectedCharacterBackground[_playerIndex].color = characterUI.Character.CharacterColor;

				if (_characterModelLocation[_playerIndex].childCount > 0)
				{
					for(int i = 0; i < _characterModelLocation[_playerIndex].childCount; i++)
					{
						Destroy(_characterModelLocation[_playerIndex].GetChild(i).gameObject);
					}
				}

				//Instantiate(characterUI.Character.Model3D, _characterModelLocation[_playerIndex]);
				_charactersModel.TryGetValue(characterUI.Character.Name, out GameObject go);
				go.transform.SetParent(_characterModelLocation[_playerIndex]);
				go.transform.localPosition = Vector3.zero;
				go.SetActive(true);
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
