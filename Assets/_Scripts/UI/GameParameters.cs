using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameParameters : MonoBehaviour
{
	[SerializeField] private int _numberOfPlayers;
	[SerializeField] private bool _isOnline;
	[SerializeField] private bool _isDouble;
	[SerializeField] private int _nbOfSets;
	[SerializeField] private int _nbOfGames;
	[SerializeField] private int _COMDifficulty;
	[SerializeField] private List<CharacterData> _playersCharacters = new List<CharacterData>();
	private static GameParameters _instance;

    public static GameParameters Instance => _instance;

	private void Awake()
	{
        if(_instance == null)
            _instance = this;

		DontDestroyOnLoad(gameObject);
	}

	public void SetNumberOfPlayer(int numberOfPlayers)
    {
        _numberOfPlayers = numberOfPlayers;
    }

    public void SetMode(bool isOnline)
    {
        _isOnline = isOnline;
    }

    public void SetGameParameters(bool isDouble, int gameMode, int COMDifficulty)
    {
        _isDouble = isDouble;
        _COMDifficulty = COMDifficulty;

        switch (gameMode)
        {
            case 0:
                _nbOfSets = 1;
                _nbOfGames = 1;
                break;
			case 1:
				_nbOfSets = 1;
				_nbOfGames = 6;
				break;
            case 2:
				_nbOfSets = 2;
				_nbOfGames = 6;
				break;
            case 3:
				_nbOfSets = 3;
				_nbOfGames = 6;
				break;
		}
    }

    public void SetCharactersPlayers(List<CharacterData> playersCharacters)
    {
        _playersCharacters = playersCharacters;
    }
}
