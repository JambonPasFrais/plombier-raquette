using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TournamentBracket : MonoBehaviour
{
	[SerializeField] private List<CharacterData> _selectedCharacters = new List<CharacterData>();
	[SerializeField] private GameObject _characterTournamentUIPrefab;
	[SerializeField] private Image _cupImage;
	[SerializeField] private List<Sprite> _tournamentCupsSprite = new List<Sprite>();

	[Header("Locations")]
	[SerializeField] private Transform _charactersDisplayFirstRoundParent;
	[SerializeField] private Transform _charactersDisplaySecondRoundParent;
	[SerializeField] private Transform _charactersDisplayThirdRoundParent;
	[SerializeField] private List<Transform> _characterFirstRoundLocations = new List<Transform>();
	[SerializeField] private List<Transform> _characterSecondRoundLocations = new List<Transform>();
	[SerializeField] private List<Transform> _characterThirdRoundLocations = new List<Transform>();

	private List<CharacterData> _availableCharacters;
	private int _nbOfPlayers = 8;
	private int _currentRound = 0;

	[Header("Players at Rounds")]
	[SerializeField] private List<GameObject> _firstRoundPlayers = new List<GameObject>();
	[SerializeField] private List<GameObject> _secondRoundPlayers = new List<GameObject>();
	[SerializeField] private List<GameObject> _thirdRoundPlayers = new List<GameObject>();

	private void Start()
	{
		_currentRound = 0;

		for (int i = 0; i < _charactersDisplayFirstRoundParent.childCount; i++)
		{
			_characterFirstRoundLocations.Add(_charactersDisplayFirstRoundParent.GetChild(i));
		}

		for (int i = 0; i < _charactersDisplaySecondRoundParent.childCount; i++)
		{
			_characterSecondRoundLocations.Add(_charactersDisplaySecondRoundParent.GetChild(i));
		}

		for (int i = 0; i < _charactersDisplayThirdRoundParent.childCount; i++)
		{
			_characterThirdRoundLocations.Add(_charactersDisplayThirdRoundParent.GetChild(i));
		}
	}

	public void PlayCurrentRound()
	{
		switch (_currentRound)
		{
			case 0:
				PlayFirstRound();
				break;
			case 1:
				PlaySecondRound();
				break;
			case 2:
				PlayThirdRound();
				break;
		}

		_currentRound++;
	}

	public void SetCharacters()
	{
		_cupImage.sprite = _tournamentCupsSprite[GameParameters.Instance.ReturnCupIndex()];

		_selectedCharacters.Add(GameParameters.Instance.GetCharactersPlayers());
		_availableCharacters = new List<CharacterData>(MenuManager.Characters);
		_availableCharacters.Remove(_selectedCharacters[0]);
		_availableCharacters.Remove(_availableCharacters.Last());

		for (int i = 1; i < _nbOfPlayers; i++)
		{
			_selectedCharacters.Add(MenuManager.Instance.ReturnRandomCharacter(_availableCharacters));
			_availableCharacters.Remove(_selectedCharacters[i]);
		}

		GameObject go;

		for(int i = 0; i < _nbOfPlayers; i++)
		{
			go = Instantiate(_characterTournamentUIPrefab, _characterFirstRoundLocations[i]);
			go.GetComponent<CharacterUI>().SetVisual(_selectedCharacters[i]);
			_firstRoundPlayers.Add(go);
		}
	}

	private void PlayFirstRound()
	{
		System.Random random = new System.Random();
		GameObject winner;

		for (int i = 0; i < _nbOfPlayers; i = i + 2)
		{
			winner = _firstRoundPlayers[i + random.Next(2)];
			_secondRoundPlayers.Add(winner);
			winner.transform.SetParent(_characterSecondRoundLocations[i / 2]);
			winner.transform.localPosition = Vector3.zero;
		}

		var tempList = _firstRoundPlayers.Except(_secondRoundPlayers).ToList();
		_firstRoundPlayers = new List<GameObject>(tempList);
	}

	private void PlaySecondRound()
	{
		System.Random random = new System.Random();
		GameObject winner;

		for (int i = 0; i < _secondRoundPlayers.Count(); i = i + 2)
		{
			winner = _secondRoundPlayers[i + random.Next(2)];
			_thirdRoundPlayers.Add(winner);
			winner.transform.SetParent(_characterThirdRoundLocations[i / 2]);
			winner.transform.localPosition = Vector3.zero;
		}

		var tempList = _secondRoundPlayers.Except(_thirdRoundPlayers).ToList();
		_secondRoundPlayers = new List<GameObject>(tempList);
	}

	private void PlayThirdRound()
	{

	}

	public void Forfait()
	{
		_selectedCharacters.Clear();
		_firstRoundPlayers.Clear();
		_secondRoundPlayers.Clear();
		_thirdRoundPlayers.Clear();
		foreach(var item in _characterFirstRoundLocations)
		{
			if(item.childCount > 0)
				Destroy(item.GetChild(0).gameObject);
		}
		
		foreach(var item in _characterSecondRoundLocations)
		{
			if (item.childCount > 0)
				Destroy(item.GetChild(0).gameObject);
		}

		foreach (var item in _characterThirdRoundLocations)
		{
			if (item.childCount > 0)
				Destroy(item.GetChild(0).gameObject);
		}

		_currentRound = 0;
		gameObject.SetActive(false);
		MenuManager.Instance.GoBackToMainMenu();
	}
}
