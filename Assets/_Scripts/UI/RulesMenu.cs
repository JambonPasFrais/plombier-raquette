using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RulesMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _COMDifficultyText;
	[SerializeField] private List<string> _difficulties = new List<string>
	{
		"Easy",
		"Medium",
		"Hard",
		"Expert",
		"Invincible"
	};
	[SerializeField] private GameObject _characterSelectionMenu;
	private int _currentDifficulty;
	private int _nbOfSets;
	private int _nbOfGames;
	private bool _isDouble;

	private void Start()
	{
		_currentDifficulty = 1;
		_COMDifficultyText.text = _difficulties[_currentDifficulty];
	}

	public void ModifyCOMDifficulty(int value)
    {
        _currentDifficulty = Mathf.Clamp(_currentDifficulty + value, 0, _difficulties.Count - 1);
        _COMDifficultyText.text = _difficulties[_currentDifficulty];
    }

	public void SetNumberOfSets(int numberOfSets)
	{
		_nbOfSets = numberOfSets;
	}

	public void SetNumberOfGames(int nbOfGames)
	{
		_nbOfGames = nbOfGames;
	}

	public void SetDouble(bool isDouble)
	{
		_isDouble = isDouble;
	}

	public void ValidateParameters()
	{
		GameParameters.Instance.SetGameParameters(_isDouble, _nbOfSets, _nbOfGames, _currentDifficulty);
		gameObject.SetActive(false);
		_characterSelectionMenu.SetActive(true);
	}
}
