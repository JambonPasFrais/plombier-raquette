using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class GameParameters : MonoBehaviour
{
	[SerializeField] private int _localNbPlayers;
	private bool _isOnline;
	[SerializeField] private int _numberOfPlayerBySide;
	[SerializeField] private GameMode _currentGameMode;
	[SerializeField] private int _COMDifficulty;
	[SerializeField] private List<CharacterData> _playersCharacters = new List<CharacterData>();
    [SerializeField] private bool _isTournamentMode;
    private int _tournamentDifficulty;
    private string _currentTournamentName;
	private static GameParameters _instance;
    private List<string> _tournamentNames = new List<string>()
    {
        "Mushroom Cup",
        "Flower Cup",
        "Star Cup"
    };
    [SerializeField] private TournamentInfos _tournamentInfos;

    public static GameParameters Instance => _instance;
    public int LocalNbPlayers => _localNbPlayers;
    public TournamentInfos CurrentTournamentInfos => _tournamentInfos;
    public List<CharacterData> PlayersCharacter => _playersCharacters;
    public static bool IsTournamentMode
    {
        get { return _instance._isTournamentMode; }
        set { _instance._isTournamentMode = value; }
    }
    public int NumberOfPlayerBySide => _numberOfPlayerBySide;

	private void Awake()
	{
        if (_instance == null)
        {
            _instance = this;
			DontDestroyOnLoad(gameObject);
			CurrentTournamentInfos.Reset();
		}
		else
            Destroy(gameObject);
	}

    public void SetMode(bool isOnline)
    {
        _isOnline = isOnline;
    }

    public void SetGameParameters(int nbOfLocalPlayers, int isDouble, GameMode gameMode, int COMDifficulty)
    {
        _numberOfPlayerBySide = isDouble;
        _COMDifficulty = COMDifficulty;
        _localNbPlayers = nbOfLocalPlayers;
        _isTournamentMode = false;
        _currentGameMode = gameMode;
    }

    public void SetCharactersPlayers(List<CharacterData> playersCharacters)
    {
        _playersCharacters = playersCharacters;
    }

    public CharacterData GetCharactersPlayers()
    {
        return _playersCharacters[0];
    }

    public void SetTournament(int difficulty)
    {
        _isTournamentMode = true;
        _tournamentDifficulty = difficulty;
        _currentTournamentName = _tournamentNames[difficulty];
    }

    public int ReturnCupIndex()
    {
        return _tournamentNames.IndexOf(_currentTournamentName);
    }
}
