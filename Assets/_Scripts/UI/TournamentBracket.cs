using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TournamentBracket : MonoBehaviour
{
	[SerializeField] private List<CharacterData> _selectedCharacters = new List<CharacterData>();
	[SerializeField] private GameObject _characterTournamentUIPrefab;
	[SerializeField] private Transform _charactersDisplayParent;
	private List<Transform> _characterPlaces = new List<Transform>();
	private List<CharacterData> _availableCharacters;
	private int _nbOfPlayers = 8;

	private void Start()
	{
		for (int i = 0; i < _charactersDisplayParent.childCount; i++)
		{
			_characterPlaces.Add(_charactersDisplayParent.GetChild(i));
		}
	}

	public void SetCharacters()
	{
		_selectedCharacters.Add(GameParameters.Instance.GetCharactersPlayers());
		_availableCharacters = new List<CharacterData>(MenuManager.Characters);
		_availableCharacters.Remove(_selectedCharacters[0]);

		for (int i = 1; i < _nbOfPlayers; i++)
		{
			_selectedCharacters.Add(MenuManager.Instance.ReturnRandomCharacter(_availableCharacters));
			_availableCharacters.Remove(_selectedCharacters[i]);
		}

		GameObject go;

		for(int i = 0; i < _nbOfPlayers; i++)
		{
			go = Instantiate(_characterTournamentUIPrefab, _characterPlaces[i]);
			go.GetComponent<CharacterUI>().SetVisual(_selectedCharacters[i]);
		}
	}

	public void Forfait()
	{
		_selectedCharacters.Clear();
		gameObject.SetActive(false);
		MenuManager.Instance.GoBackToMainMenu();
	}
}
