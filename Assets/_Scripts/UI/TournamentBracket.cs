using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TournamentBracket : MonoBehaviour
{
	[SerializeField] private List<CharacterData> _selectedCharacters = new List<CharacterData>();
	[SerializeField] private GameObject _characterTournamentUIPrefab;
	[SerializeField] private Image _cupImage;
	[SerializeField] private List<Sprite> _tournamentCupsSprite = new List<Sprite>();
	[SerializeField] private TournamentEndMenu _tournamentEndMenu;

	[Header("Locations")]
	[SerializeField] private Transform _charactersDisplayFirstRoundParent;
	[SerializeField] private Transform _charactersDisplaySecondRoundParent;
	[SerializeField] private Transform _charactersDisplayThirdRoundParent;
	[SerializeField] private Transform _winnerLocation;
	[SerializeField] private List<Transform> _characterFirstRoundLocations = new List<Transform>();
	[SerializeField] private List<Transform> _characterSecondRoundLocations = new List<Transform>();
	[SerializeField] private List<Transform> _characterThirdRoundLocations = new List<Transform>();

	private List<CharacterData> _availableCharacters;
	private int _nbOfPlayers = 8;
	[SerializeField] private int _currentRound = 0;

	[Header("Players at Rounds")]
	[SerializeField] private List<GameObject> _firstRoundPlayers = new List<GameObject>();
	[SerializeField] private List<GameObject> _secondRoundPlayers = new List<GameObject>();
	[SerializeField] private List<GameObject> _thirdRoundPlayers = new List<GameObject>();
	[SerializeField] private GameObject _tournamentWinner;

	private GameObject _playersCharacter;

	private void Awake()
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
	}

	public void SetCharacters()
	{
		_cupImage.sprite = _tournamentCupsSprite[GameParameters.Instance.ReturnCupIndex()];
		GameParameters.CurrentTournamentInfos.CupSprite = _tournamentCupsSprite[GameParameters.Instance.ReturnCupIndex()];

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

		GameParameters.CurrentTournamentInfos.PlayersCharacter = _firstRoundPlayers[0].GetComponent<CharacterUI>().Character;
	}

	public void SetCurrentBracket(TournamentInfos currentTournament)
	{
		_cupImage.sprite = GameParameters.CurrentTournamentInfos.CupSprite;
		_currentRound = currentTournament.CurrentRound;
		List<CharacterData> datas = new List<CharacterData>();
		currentTournament.RoundPlayers.TryGetValue(0, out datas);

		GameObject go;

		for (int i = 0; i < datas.Count; i++)
		{
			if (datas[i] != null)
			{
				go = Instantiate(_characterTournamentUIPrefab, _characterFirstRoundLocations[i]);
				go.GetComponent<CharacterUI>().SetVisual(datas[i]);
				_firstRoundPlayers.Add(go);
				if (datas[i] == currentTournament.PlayersCharacter)
					_playersCharacter = go;
			}
			else
				_firstRoundPlayers.Add(null);
		}

		currentTournament.RoundPlayers.TryGetValue(1, out datas);
		for (int i = 0; i < datas.Count; i++)
		{
			if (datas[i] != null)
			{
				go = Instantiate(_characterTournamentUIPrefab, _characterSecondRoundLocations[i]);
				go.GetComponent<CharacterUI>().SetVisual(datas[i]);
				_secondRoundPlayers.Add(go);
				if (datas[i] == currentTournament.PlayersCharacter)
					_playersCharacter = go;
			}
			else
				_secondRoundPlayers.Add(null);
		}

		if(!datas.Contains(currentTournament.PlayersCharacter) && currentTournament.CurrentRound == 1)
			StartCoroutine(WaitBeforeShowingLoserMenu(_playersCharacter));

		if (currentTournament.CurrentRound > 1)
		{
			currentTournament.RoundPlayers.TryGetValue(2, out datas);
			for (int i = 0; i < datas.Count; i++)
			{
				if (datas[i] != null)
				{
					go = Instantiate(_characterTournamentUIPrefab, _characterThirdRoundLocations[i]);
					go.GetComponent<CharacterUI>().SetVisual(datas[i]);
					_thirdRoundPlayers.Add(go);
					if (datas[i] == currentTournament.PlayersCharacter)
						_playersCharacter = go;
				}
				else
					_thirdRoundPlayers.Add(null);
			}

			if (!datas.Contains(currentTournament.PlayersCharacter) && currentTournament.CurrentRound == 2)
				StartCoroutine(WaitBeforeShowingLoserMenu(_playersCharacter));

			if (currentTournament.CurrentRound > 2)
			{
				currentTournament.RoundPlayers.TryGetValue(3, out datas);
				go = Instantiate(_characterTournamentUIPrefab, _winnerLocation);
				go.GetComponent<CharacterUI>().SetVisual(datas[0]);
				_tournamentWinner = go;

				if (datas[0] != currentTournament.PlayersCharacter)
					StartCoroutine(WaitBeforeShowingLoserMenu(_thirdRoundPlayers[0]));
				else
					StartCoroutine(WaitBeforeShowWinner(_tournamentWinner));
			}
		}
	}

	private void PlayFirstRound()
	{
		System.Random random = new System.Random();
		GameObject winner;

		/*_secondRoundPlayers.Add(_firstRoundPlayers[0]);
		_firstRoundPlayers[0].transform.SetParent(_characterSecondRoundLocations[0]);
		_firstRoundPlayers[0].transform.localPosition = Vector3.zero;
		_firstRoundPlayers[0] = null;*/

		for (int i = 0; i < _nbOfPlayers; i = i + 2)
		{
			winner = _firstRoundPlayers[i + random.Next(2)];
			_secondRoundPlayers.Add(winner);
			winner.transform.SetParent(_characterSecondRoundLocations[i / 2]);
			winner.transform.localPosition = Vector3.zero;
			_firstRoundPlayers[_firstRoundPlayers.IndexOf(winner)] = null;
		}

		List<CharacterData> _firstRoundDatas = new List<CharacterData>();
		List<CharacterData> _secondRoundDatas = new List<CharacterData>();
		foreach (var item in _firstRoundPlayers)
		{
			if (item != null)
				_firstRoundDatas.Add(item.GetComponent<CharacterUI>().Character);
			else
				_firstRoundDatas.Add(null);
		}
		foreach (var item in _secondRoundPlayers)
		{
			_secondRoundDatas.Add(item.GetComponent<CharacterUI>().Character);
		}

		GameParameters.CurrentTournamentInfos.SetRoundPlayers(_firstRoundDatas, _secondRoundDatas, null, null);

		SceneManager.LoadScene(0);
	}

	private void PlaySecondRound()
	{
		System.Random random = new System.Random();
		GameObject winner;

		/*_thirdRoundPlayers.Add(_secondRoundPlayers[0]);
		_secondRoundPlayers[0].transform.SetParent(_characterThirdRoundLocations[0]);
		_secondRoundPlayers[0].transform.localPosition = Vector3.zero;
		_secondRoundPlayers[0] = null;*/

		for (int i = 0; i < _secondRoundPlayers.Count(); i = i + 2)
		{
			winner = _secondRoundPlayers[i + random.Next(2)];
			_thirdRoundPlayers.Add(winner);
			winner.transform.SetParent(_characterThirdRoundLocations[i / 2]);
			winner.transform.localPosition = Vector3.zero;
			_secondRoundPlayers[_secondRoundPlayers.IndexOf(winner)] = null;
		}

		List<CharacterData> _firstRoundDatas = new List<CharacterData>();
		List<CharacterData> _secondRoundDatas = new List<CharacterData>();
		List<CharacterData> _thirdRoundDatas = new List<CharacterData>();

		for (int i = 0; i < _firstRoundPlayers.Count(); i++)
		{
			if (_firstRoundPlayers[i] != null)
				_firstRoundDatas.Add(_firstRoundPlayers[i].GetComponent<CharacterUI>().Character);
			else
				_firstRoundDatas.Add(null);
		}
		for (int i = 0; i < _secondRoundPlayers.Count(); i++)
		{
			if (_secondRoundPlayers[i] != null)
				_secondRoundDatas.Add(_secondRoundPlayers[i].GetComponent<CharacterUI>().Character);
			else
				_secondRoundDatas.Add(null);
		}
		foreach (var item in _thirdRoundPlayers)
		{
			_thirdRoundDatas.Add(item.GetComponent<CharacterUI>().Character);
		}

		GameParameters.CurrentTournamentInfos.SetRoundPlayers(_firstRoundDatas, _secondRoundDatas, _thirdRoundDatas, null);

		SceneManager.LoadScene(0);
	}

	private void PlayThirdRound()
	{
		System.Random random = new System.Random();
		GameObject winner;
		winner = _thirdRoundPlayers[random.Next(2)];
		//winner = _thirdRoundPlayers[0];
		_tournamentWinner = winner;
		winner.transform.SetParent(_winnerLocation);
		winner.transform.localPosition = Vector3.zero;
		_thirdRoundPlayers[_thirdRoundPlayers.IndexOf(winner)] = null;

		List<CharacterData> _firstRoundDatas = new List<CharacterData>();
		List<CharacterData> _secondRoundDatas = new List<CharacterData>();
		List<CharacterData> _thirdRoundDatas = new List<CharacterData>();

		for (int i = 0; i < _firstRoundPlayers.Count(); i++)
		{
			if (_firstRoundPlayers[i] != null)
				_firstRoundDatas.Add(_firstRoundPlayers[i].GetComponent<CharacterUI>().Character);
			else
				_firstRoundDatas.Add(null);
		}
		for (int i = 0; i < _secondRoundPlayers.Count(); i++)
		{
			if (_secondRoundPlayers[i] != null)
				_secondRoundDatas.Add(_secondRoundPlayers[i].GetComponent<CharacterUI>().Character);
			else
				_secondRoundDatas.Add(null);
		}
		for (int i = 0; i < _thirdRoundPlayers.Count(); i++)
		{
			if (_thirdRoundPlayers[i] != null)
				_thirdRoundDatas.Add(_thirdRoundPlayers[i].GetComponent<CharacterUI>().Character);
			else
				_thirdRoundDatas.Add(null);
		}

		CharacterData winnerData = winner.GetComponent<CharacterUI>().Character;

		GameParameters.CurrentTournamentInfos.SetRoundPlayers(_firstRoundDatas, _secondRoundDatas, _thirdRoundDatas, winnerData);

		SceneManager.LoadScene(0);
	}

	private void ResetBracket()
	{
		_selectedCharacters.Clear();
		_firstRoundPlayers.Clear();
		_secondRoundPlayers.Clear();
		_thirdRoundPlayers.Clear();
		foreach (var item in _characterFirstRoundLocations)
		{
			if (item.childCount > 0)
				Destroy(item.GetChild(0).gameObject);
		}

		foreach (var item in _characterSecondRoundLocations)
		{
			if (item.childCount > 0)
				Destroy(item.GetChild(0).gameObject);
		}

		foreach (var item in _characterThirdRoundLocations)
		{
			if (item.childCount > 0)
				Destroy(item.GetChild(0).gameObject);
		}

		if(_winnerLocation.childCount > 0)
			Destroy(_winnerLocation.GetChild(0).gameObject);

		_currentRound = 0;
		gameObject.SetActive(false);
	}

	public void Forfait()
	{
		ResetBracket();
		GameParameters.CurrentTournamentInfos.Reset();
		MenuManager.Instance.GoBackToMainMenu();
	}

	private IEnumerator WaitBeforeShowWinner(GameObject playerCharacter)
	{
		yield return new WaitForSeconds(1);
		ResetBracket();
		_tournamentEndMenu.gameObject.SetActive(true);
		_tournamentEndMenu.SetWinnerMenu(playerCharacter.GetComponent<CharacterUI>().Character.Model3D, _cupImage.sprite);
	}

	private IEnumerator WaitBeforeShowingLoserMenu(GameObject playerCharacter)
	{
		yield return new WaitForSeconds(1);
		ResetBracket();
		_tournamentEndMenu.gameObject.SetActive(true);
		_tournamentEndMenu.SetLoserMenu(playerCharacter.GetComponent<CharacterUI>().Character.Model3D);
	}
}
