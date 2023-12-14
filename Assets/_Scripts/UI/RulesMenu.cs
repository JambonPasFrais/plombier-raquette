using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RulesMenu : MonoBehaviour
{
	private int _maxNumberOfPlayer;
	private int _currentNumberOfPlayers;
	[SerializeField] private TextMeshProUGUI _playerNumberText;
	[SerializeField] private TextMeshProUGUI _gameTypeText;
	[SerializeField] private TextMeshProUGUI _gameModeText;
	[SerializeField] private TextMeshProUGUI _comDifficultyText;
	[SerializeField] private TextMeshProUGUI _explanationText;
	private int _currentGameType;
	private int _currentGameMode;
	private int _currentCOMDifficulty;
	private List<string> _gameTypes = new List<string>()
	{
		"Single",
		"Double"
	};
	[SerializeField] private List<GameMode> _gameModes = new List<GameMode>();
	[SerializeField]
	private List<string> _comDifficulties = new List<string>
	{
		"Easy",
		"Medium",
		"Hard",
		"Expert",
		"Invincible"
	};
	[SerializeField] private GameObject _firstSelectedObject;

	private void Start()
	{
		_maxNumberOfPlayer = 2;
		_currentNumberOfPlayers = 1;
		_playerNumberText.text = _currentNumberOfPlayers.ToString();
		_currentGameType = 0;
		_gameTypeText.text = _gameTypes[_currentGameType];
		_currentGameMode = 1;
		_gameModeText.text = _gameModes[_currentGameMode].Name;
		_currentCOMDifficulty = 1;
		_comDifficultyText.text = _comDifficulties[_currentCOMDifficulty];
	}

	public void ModifyNumberOfPlayer(int value)
	{
		if (_currentNumberOfPlayers == 1 && value == -1)
			_currentNumberOfPlayers = _maxNumberOfPlayer;
		else if(_currentNumberOfPlayers == _maxNumberOfPlayer && value == 1)
			_currentNumberOfPlayers = 1;
		else
			_currentNumberOfPlayers = Mathf.Clamp(_currentNumberOfPlayers + value, 1, _maxNumberOfPlayer);

		_playerNumberText.text = _currentNumberOfPlayers.ToString();
		_explanationText.text = $"Play match with {_currentNumberOfPlayers} local players";
	}

	public void ModifyGameType(int value)
	{
		_currentGameType = Mathf.Abs((_currentGameType + value) % 2);
		_gameTypeText.text = _gameTypes[_currentGameType];
		_explanationText.text = $"Play in a {_gameTypes[_currentGameType]} match";

		if (_currentGameType == 0)
		{
			_maxNumberOfPlayer = 2;

			if (_currentNumberOfPlayers > _maxNumberOfPlayer)
			{
				_currentNumberOfPlayers = _maxNumberOfPlayer;
				_playerNumberText.text = _currentNumberOfPlayers.ToString();
			}
		}
		else
			_maxNumberOfPlayer = 4;
	}

	public void ModifyGameMode(int value)
	{
		if (_currentGameMode == _gameModes.Count - 1 && value == 1)
			_currentGameMode = 0;
		else if(_currentGameMode == 0 && value == -1)
			_currentGameMode = _gameModes.Count - 1;
		else
			_currentGameMode = Mathf.Clamp(_currentGameMode + value, 0, _gameModes.Count - 1);

		GameMode gm = _gameModes[_currentGameMode];
		_gameModeText.text = gm.Name;
		_explanationText.text = $"Play a match in {gm.NbOfSets} sets and {gm.NbOfGames} games";
	}

	public void ModifyCOMDifficulty(int value)
	{
		if (_currentCOMDifficulty == _comDifficulties.Count - 1 && value == 1)
			_currentCOMDifficulty = 0;
		else if (_currentCOMDifficulty == 0 && value == -1)
			_currentCOMDifficulty = _comDifficulties.Count - 1;
		else
			_currentCOMDifficulty = Mathf.Clamp(_currentCOMDifficulty + value, 0, _comDifficulties.Count - 1);

		_comDifficultyText.text = _comDifficulties[_currentCOMDifficulty];
		_explanationText.text = $"Play a match with COM at {_comDifficulties[_currentCOMDifficulty]} difficulty";
	}

	/*private void VerifyEntries()
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
