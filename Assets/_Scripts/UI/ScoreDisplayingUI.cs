using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplayingUI : MonoBehaviour
{
	[SerializeField] private GameObject _setScorePrefab;
	[SerializeField] private GameObject _currentGameScorePrefab;
    [SerializeField] private List<Image> _playersIconsSingleMatch = new List<Image>();
    [SerializeField] private List<Image> _backgrounds = new List<Image>();
    [SerializeField] private List<TextMeshProUGUI> _setsScoreTeam1 = new List<TextMeshProUGUI>();
    [SerializeField] private List<TextMeshProUGUI> _setsScoreTeam2 = new List<TextMeshProUGUI>();
	[SerializeField] private List<TextMeshProUGUI> _currentGameScore = new List<TextMeshProUGUI>();

	private void Start()
	{
		for(int i = 0; i < GameParameters.Instance.LocalNbPlayers; i++)
		{
			_playersIconsSingleMatch[i].sprite = GameParameters.Instance.PlayersCharacter[i].Picture;
			_backgrounds[i].color = GameParameters.Instance.PlayersCharacter[i].CharacterPrimaryColor;
		}

		foreach (var item in _setsScoreTeam1)
			item.text = "0";
		
		foreach (var item in _setsScoreTeam2)
			item.text = "0";

		foreach (var item in _currentGameScore)
			item.text = "0";
	}
}
