using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

public class ScoreDisplayingUI : MonoBehaviour
{
	[SerializeField] private GameObject _setScorePrefab;
	[SerializeField] private GameObject _currentGameScorePrefab;
	[SerializeField] private List<Image> _charactersFacesBackground = new List<Image>();
	[SerializeField] private List<Transform> _charactersFacesParentSingle = new List<Transform>();
	[SerializeField] private List<Transform> _charactersFacesParentDouble = new List<Transform>();
	[SerializeField] private List<Image> _charactersFacesSingle = new List<Image>();
	[SerializeField] private List<Image> _charactersFacesDouble = new List<Image>();
	[SerializeField] private List<Transform> _scoreContainers = new List<Transform>();

	private List<Color> _playersColors = new List<Color>();
	private List<GameObject> _team1Sets = new List<GameObject>();
	private List<GameObject> _team2Sets = new List<GameObject>();
	private List<GameObject> _currentGamesScore = new List<GameObject>();

	private void Start()
	{
		
		foreach (var item in _charactersFacesParentSingle)
		{
			foreach(Transform child in item)
			{
				_charactersFacesSingle.Add(child.GetComponent<Image>());
			}

			item.gameObject.SetActive(false);
		}
		
		foreach(var item in _charactersFacesParentDouble)
		{
			foreach(Transform child in item)
			{
				_charactersFacesDouble.Add(child.GetComponent<Image>());
			}
			
			item.gameObject.SetActive(false);
		}

		for (int i = 0; i < 2; i++)
		{
			_playersColors.Add(GameParameters.Instance.PlayersCharacter[i * 2].CharacterPrimaryColor);

			if (GameParameters.Instance.NumberOfPlayerBySide == 0)
			{
				_charactersFacesParentSingle[i].gameObject.SetActive(true);
				_charactersFacesBackground[i].color = _playersColors[i];
				_charactersFacesSingle[i].sprite = GameParameters.Instance.PlayersCharacter[i].Picture;
			}
			else
			{
				_charactersFacesParentDouble[i].gameObject.SetActive(true);
				_charactersFacesParentDouble[i].gameObject.SetActive(true);
				_charactersFacesBackground[i].color = _playersColors[i];
				_charactersFacesDouble[i * 2].sprite = GameParameters.Instance.PlayersCharacter[i * 2].Picture;
				_charactersFacesDouble[i * 2 + 1].sprite = GameParameters.Instance.PlayersCharacter[i * 2 + 1].Picture;
			}
			
			GameObject go = Instantiate(_setScorePrefab, _scoreContainers[i]);
			go.GetComponent<ScoreDisplay>().Initialize(_playersColors[i], "0");

			if (i == 0)
				_team1Sets.Add(go);
			else
				_team2Sets.Add(go);

			go = Instantiate(_currentGameScorePrefab, _scoreContainers[i]);
			go.GetComponent<ScoreDisplay>().Initialize(_playersColors[i], "0");
			_currentGamesScore.Add(go);
		}
	}

	public void UpdateGameScore(int teamIndex, string newGameScore)
	{
		_currentGamesScore[teamIndex].GetComponent<ScoreDisplay>().SetScore(newGameScore);
	}

	public void UpdateSetScore(int teamIndex, int setIndex, string newSetScore)
	{
		if (teamIndex == 0)
		{
			_team1Sets[setIndex].GetComponent<ScoreDisplay>().SetScore(newSetScore);
		}

		else
		{
			_team2Sets[setIndex].GetComponent<ScoreDisplay>().SetScore(newSetScore);
		}
	}

	public void NewSet()
	{
		Destroy(_currentGamesScore[0]);
		Destroy(_currentGamesScore[1]);
		_currentGamesScore.Clear();

		for (int i = 0; i < 2; i++)
		{
			GameObject go = Instantiate(_setScorePrefab, _scoreContainers[i]);
			go.GetComponent<ScoreDisplay>().Initialize(_playersColors[i], "0");

			if (i == 0)
				_team1Sets.Add(go);
			else
				_team2Sets.Add(go);

			go = Instantiate(_currentGameScorePrefab, _scoreContainers[i]);
			go.GetComponent<ScoreDisplay>().Initialize(_playersColors[i], "0");
			_currentGamesScore.Add(go);
		}
	}
}
