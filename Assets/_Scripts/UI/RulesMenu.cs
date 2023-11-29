using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
	[SerializeField] private Button _validationButton;
	[SerializeField] private Transform _gameTypeButtonsParent;
	[SerializeField] private Transform _gameModeButtonsParent;
	[SerializeField] private List<Image> _gameTypeButtons = new List<Image>();
	[SerializeField] private List<Image> _gameModeButtons = new List<Image>();
	private int _currentDifficulty;
	private int _gameMode;
	private int _numberOfPlayerBySide;

	private void Start()
	{
		for (int i = 0; i < _gameModeButtonsParent.childCount; i++)
		{
			_gameModeButtons.Add(_gameModeButtonsParent.GetChild(i).gameObject.GetComponent<Image>());
		}
		
		for (int i = 0; i < _gameTypeButtonsParent.childCount; i++)
		{
			_gameTypeButtons.Add(_gameTypeButtonsParent.GetChild(i).gameObject.GetComponent<Image>());
		}

		_gameMode = -1;
		_numberOfPlayerBySide = -1;
		VerifyEntries();
		_currentDifficulty = 1;
		_COMDifficultyText.text = _difficulties[_currentDifficulty];
	}

	public void ModifyCOMDifficulty(int value)
    {
        _currentDifficulty = Mathf.Clamp(_currentDifficulty + value, 0, _difficulties.Count - 1);
        _COMDifficultyText.text = _difficulties[_currentDifficulty];
    }

	public void SetGameMode(int modeIndex)
	{
		if (_gameMode != -1)
			_gameModeButtons[_gameMode].color = Color.white;

		_gameMode = modeIndex;

		_gameModeButtons[_gameMode].color = Color.red;
		VerifyEntries();
	}

	public void SetDouble(int numberOfPlayerBySide)
	{
		if (_numberOfPlayerBySide != -1)
			_gameTypeButtons[_numberOfPlayerBySide - 1].color = Color.white;

		_numberOfPlayerBySide = numberOfPlayerBySide;

		_gameTypeButtons[_numberOfPlayerBySide - 1].color = Color.red;
		VerifyEntries();
	}

	private void VerifyEntries()
	{
		if (_numberOfPlayerBySide != -1 && _gameMode != -1)
			_validationButton.interactable = true;
		else
			_validationButton.interactable = false;
	}

	public void ValidateParameters()
	{
		GameParameters.Instance.SetGameParameters(_numberOfPlayerBySide, _gameMode, _currentDifficulty);
		gameObject.SetActive(false);
		_characterSelectionMenu.SetActive(true);
	}
}
