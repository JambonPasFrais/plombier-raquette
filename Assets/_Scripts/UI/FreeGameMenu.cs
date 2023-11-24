using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FreeGameMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _playerNumberText;
    [SerializeField] private int _currentNumberOfPlayers;

	private void Start()
	{
		_currentNumberOfPlayers = 1;
	}

	public void IncreaseNumberOfPlayer()
    {
		_currentNumberOfPlayers = Mathf.Clamp(_currentNumberOfPlayers + 1, 1, 4);
		_playerNumberText.text = $"Players: {_currentNumberOfPlayers}";

	}
    
    public void DecreaseNumberOfPlayer()
    {
		_currentNumberOfPlayers = Mathf.Clamp(_currentNumberOfPlayers - 1, 1, 4);
		_playerNumberText.text = $"Players: {_currentNumberOfPlayers}";
	}
}
