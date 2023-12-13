using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RulesMenu : MonoBehaviour
{
	/*[SerializeField] private TextMeshProUGUI _COMDifficultyText;
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
	[SerializeField] private List<GameMode> _possibleGameModes = new List<GameMode>();
	private List<Image> _gameTypeButtonImages = new List<Image>();
	private List<Image> _gameModeButtonsImages = new List<Image>();
	private List<Button> _gameTypeButtons = new List<Button>();
	[SerializeField] private TextMeshProUGUI _playerNumberText;
	[SerializeField] private TextMeshProUGUI _explanationText;
	private int _currentNumberOfPlayers;
	private int _currentDifficulty;
	private int _gameMode;
	private int _numberOfPlayerBySide;
	private int _maxNumberOfPlayer;*/

	private int _maxNumberOfPlayer;
	private int _currentNumberOfPlayers;
	[SerializeField] private TextMeshProUGUI _playerNumberText;
	[SerializeField] private TextMeshProUGUI _explanationText;

	private void Start()
	{
		/*for (int i = 0; i < _gameModeButtonsParent.childCount; i++)
		{
			_gameModeButtonsImages.Add(_gameModeButtonsParent.GetChild(i).gameObject.GetComponent<Image>());
		}

		for (int i = 0; i < _gameTypeButtonsParent.childCount; i++)
		{
			_gameTypeButtonImages.Add(_gameTypeButtonsParent.GetChild(i).gameObject.GetComponent<Image>());
			_gameTypeButtons.Add(_gameTypeButtonsParent.GetChild(i).gameObject.GetComponent<Button>());
		}
		
		_gameMode = -1;
		_numberOfPlayerBySide = -1;
		VerifyEntries();
		_currentDifficulty = 1;
		_COMDifficultyText.text = _difficulties[_currentDifficulty];
		_currentNumberOfPlayers = 1;*/

		_maxNumberOfPlayer = 4;
		_currentNumberOfPlayers = 1;
		_playerNumberText.text = _currentNumberOfPlayers.ToString();
	}

	public void ModifyNumberOfPlayer(int value)
	{
		_currentNumberOfPlayers = Mathf.Clamp(_currentNumberOfPlayers + value, 1, _maxNumberOfPlayer);
		_playerNumberText.text = _currentNumberOfPlayers.ToString();
		_explanationText.text = $"Play match with {_currentNumberOfPlayers} local players";
	}

	/*public void ModifyCOMDifficulty(int value)
    {
        _currentDifficulty = Mathf.Clamp(_currentDifficulty + value, 0, _difficulties.Count - 1);
        _COMDifficultyText.text = _difficulties[_currentDifficulty];
    }

	public void SetGameMode(int modeIndex)
	{
		if (_gameMode != -1)
			_gameModeButtonsImages[_gameMode].color = Color.white;

		_gameMode = modeIndex;

		_gameModeButtonsImages[_gameMode].color = Color.red;

		VerifyEntries();
	}

	public void SetDouble(int numberOfPlayerBySide)
	{
		if (_numberOfPlayerBySide != -1)
		{
			_gameTypeButtonImages[_numberOfPlayerBySide - 1].color = Color.white;
		}

		if (numberOfPlayerBySide == 1)
		{
			_maxNumberOfPlayer = 2;
		}

		else
			_maxNumberOfPlayer = 4;

		_numberOfPlayerBySide = numberOfPlayerBySide;

		_gameTypeButtonImages[_numberOfPlayerBySide - 1].color = Color.red;
		VerifyEntries();
	}

	private void VerifyEntries()
	{
		if (_numberOfPlayerBySide != -1 && _gameMode != -1)
			_validationButton.interactable = true;
		else
			_validationButton.interactable = false;
	}

	public void ResetMenu()
	{
		foreach (var item in _gameTypeButtons)
		{
			item.interactable = true;
		}

		for (int i = 0; i < _gameModeButtonsParent.childCount; i++)
		{
			_gameModeButtonsParent.GetChild(i).GetComponent<Button>().interactable = true;
		}

		foreach (var item in _gameTypeButtonImages)
		{
			item.color = Color.white;
		}

		foreach (var item in _gameModeButtonsImages)
		{
			item.color = Color.white;
		}

		_maxNumberOfPlayer = 4;
		_gameMode = -1;
		_numberOfPlayerBySide = -1;
		_currentDifficulty = 1;
		_COMDifficultyText.text = _difficulties[_currentDifficulty];
		_currentNumberOfPlayers = 1;
		_playerNumberText.text = _currentNumberOfPlayers.ToString();
	}

	public void ValidateParameters()
	{
		GameParameters.Instance.SetGameParameters(_currentNumberOfPlayers, _numberOfPlayerBySide, _gameMode, _currentDifficulty);
	}*/
}
