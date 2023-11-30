using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FreeGameMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _playerNumberText;
    [SerializeField] private int _currentNumberOfPlayers;
	[SerializeField] private GameObject _rulesMenu;

	private void Start()
	{
		_currentNumberOfPlayers = 1;
		_playerNumberText.text = $"Players: {_currentNumberOfPlayers}";
	}

	public void ModifyNumberOfPlayer(int value)
    {
		_currentNumberOfPlayers = Mathf.Clamp(_currentNumberOfPlayers + value, 1, 4);
		_playerNumberText.text = $"Players: {_currentNumberOfPlayers}";
	}

	public void GoToRulesSelection(bool isOnline)
	{
		GameParameters.Instance.SetNumberOfPlayer(_currentNumberOfPlayers);
		GameParameters.Instance.SetMode(isOnline);
	}
}
