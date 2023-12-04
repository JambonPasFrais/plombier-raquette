using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameParameters : MonoBehaviour
{
	private int _numberOfPlayers;
	private bool _isOnline;
	private int _numberOfPlayerBySide;
	private int _nbOfSets;
	private int _nbOfGames;
	private int _COMDifficulty;
	[SerializeField] private List<CharacterData> _playersCharacters = new List<CharacterData>();
    private bool _isTournamentMode;
    private int _tournamentDifficulty;
    [SerializeField] private string _tournamentName;
	private static GameParameters _instance;
    private List<string> _tournamentNames = new List<string>()
    {
        "Mushroom Cup",
        "Flower Cup",
        "Star Cup"
    };

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

    public void SetGameParameters(int isDouble, int gameMode, int COMDifficulty)
    {
        _numberOfPlayerBySide = isDouble;
        _COMDifficulty = COMDifficulty;
        _numberOfPlayers = isDouble * 2;
        _isTournamentMode = false;

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

    public void SetTournament(int difficulty)
    {
        _isTournamentMode = true;
        _tournamentDifficulty = difficulty;
        _tournamentName = _tournamentNames[difficulty];
    }
}
