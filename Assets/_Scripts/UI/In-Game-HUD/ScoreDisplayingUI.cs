using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplayingUI : MonoBehaviour
{
	[SerializeField] private GameObject _setScorePrefab;
	[SerializeField] private GameObject _currentGameScorePrefab;
	[SerializeField] private List<GameObject> _charactersFaces = new List<GameObject>();
	[SerializeField] private List<Transform> _scoreContainers = new List<Transform>();

	private List<Color> _playersColors = new List<Color>();

	private void Start()
	{
		for(int i = 0; i < 2; i++)
		{
			_playersColors.Add(GameParameters.Instance.PlayersCharacter[i].CharacterPrimaryColor);
			_charactersFaces[i].GetComponent<Image>().color = GameParameters.Instance.PlayersCharacter[i].CharacterPrimaryColor;
			_charactersFaces[i].transform.GetChild(0).GetComponent<Image>().sprite = GameParameters.Instance.PlayersCharacter[i].Picture;
			
			GameObject go = Instantiate(_setScorePrefab, _scoreContainers[i]);
			go.GetComponent<ScoreDisplay>().Initialize(_playersColors[i], 0);

			go = Instantiate(_currentGameScorePrefab, _scoreContainers[i]);
			go.GetComponent<ScoreDisplay>().Initialize(_playersColors[i], 0);
		}
	}
}
