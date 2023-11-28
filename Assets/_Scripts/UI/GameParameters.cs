using System.Collections;
using System.Collections.Generic;
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

    public void SetGameParameters(bool isDouble, int nbOfSets, int nbOfGames, int COMDifficulty)
    {
        _isDouble = isDouble;
        _nbOfSets = nbOfSets;
        _nbOfGames = nbOfGames;
        _COMDifficulty = COMDifficulty;
    }

    public void SetCharactersPlayers(List<CharacterData> playersCharacters)
    {
        _playersCharacters = playersCharacters;
    }
}
