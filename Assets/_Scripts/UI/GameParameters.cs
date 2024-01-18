using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class GameParameters : MonoBehaviour
{
	[SerializeField] private int _localNbPlayers;
	private bool _isOnline;
	[SerializeField] private bool _isDouble;
	[SerializeField] private GameMode _currentGameMode;
	[SerializeField] private int _COMDifficulty;
	[SerializeField] private List<CharacterData> _playersCharacters = new List<CharacterData>(); // TODO : delete [serializeField] after tests
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

    #region GETTERS
    
    public static GameParameters Instance => _instance;
    public int LocalNbPlayers => _localNbPlayers;
    public TournamentInfos CurrentTournamentInfos => _tournamentInfos;
    public List<CharacterData> PlayersCharacter => _playersCharacters;
    public GameMode CurrentGameMode => _currentGameMode;
    public static bool IsTournamentMode
    {
        get { return _instance._isTournamentMode; }
        set { _instance._isTournamentMode = value; }
    }
    public bool IsDouble => _isDouble;
    
    #endregion

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

    public void SetGameParameters(int nbOfLocalPlayers, bool isDouble, GameMode gameMode, int COMDifficulty)
    {
        _isDouble = isDouble;
        _COMDifficulty = COMDifficulty;
        _localNbPlayers = nbOfLocalPlayers;
        _isTournamentMode = false;
        _currentGameMode = gameMode;
    }

    public void SetCharactersPlayers(List<CharacterData> playersCharacters)
    {
        _playersCharacters = new List<CharacterData>(playersCharacters);
    }

    public CharacterData GetCharactersPlayers()
    {
        return _playersCharacters[0];
    }

    public void SetTournament(int difficulty)
    {
        _isTournamentMode = true;
        _tournamentDifficulty = difficulty;
        _COMDifficulty = difficulty;
        _currentTournamentName = _tournamentNames[difficulty];
        
        _localNbPlayers = 1;
        _isDouble = false;
        _currentGameMode = new GameMode("Tournament", 1, 6);
    }

    public int ReturnCupIndex()
    {
        return _tournamentNames.IndexOf(_currentTournamentName);
    }
}
