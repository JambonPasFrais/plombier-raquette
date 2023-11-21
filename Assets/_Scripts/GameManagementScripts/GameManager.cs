using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    #region PUBLIC FIELDS

    public static GameManager Instance;

    public Transform BallInitializationTransform;
    public GameObject BallInstance;

    #endregion

    #region PRIVATE FIELDS

    [SerializeField] private List<ControllersParent> _controllers;

    private Dictionary<ControllersParent, Player> _playerControllersAssociated;
    private Dictionary<Player, int> _playersPoints;
    private Dictionary<Player, int> _playersGames;

    #endregion

    #region UNITY METHODS

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        _playerControllersAssociated = new Dictionary<ControllersParent, Player>();
        _playersPoints = new Dictionary<Player, int>();
        _playersGames = new Dictionary<Player, int>();

        int i = 0;
        foreach (ControllersParent controller in _controllers)
        {
            Player newPlayer = new Player($"Player {i + 1}");
            _playerControllersAssociated.Add(controller, newPlayer);
            _playersPoints.Add(newPlayer, 0);
            _playersGames.Add(newPlayer, 0);
            i++;
        }

        ScoreUpdate();
    }

    void Update()
    {
        // Ball instantiation.
        if (Input.GetKeyDown(KeyCode.C))
        {
            BallInstance.GetComponent<Ball>().ResetBallFunction();

            BallInstance.transform.position = BallInitializationTransform.position;
            BallInstance.SetActive(true);
        }
    }

    #endregion

    public void PointFinished(int reboundCount, ControllersParent lastPlayerToHit)
    {
        Player currentPlayer = _playerControllersAssociated[lastPlayerToHit];
        Player otherPlayer = GetOtherPlayer(lastPlayerToHit);

        BallInstance.GetComponent<Ball>().ResetBallFunction();
        
        if (reboundCount == 1)
        {
            // Si on est en avantage, on revient à 40.
            if (_playersPoints[currentPlayer] == 4)
            {
                _playersPoints[currentPlayer]--;
            }
            // Sinon si l'autre est en avantage ou (qu'il est à 40 mais que nous avons moins de 40), les sets gagnants de l'autre sont incrémentés et on fixe les points des 2 joueurs à 0.
            else if (_playersPoints[otherPlayer] == 4 || (_playersPoints[otherPlayer] == 3 && _playersPoints[currentPlayer] < 3))
            {
                _playersPoints[currentPlayer] = 0;
                _playersPoints[otherPlayer] = 0;

                _playersGames[otherPlayer]++;
                EndOfGameVerification(otherPlayer);
            }
            // Sinon on incrémente les points de l'autre joueur.
            else
            {
                _playersPoints[otherPlayer]++;
            }
        }
        else if (reboundCount == 2)
        {
            // Si l'autre a l'avantage, il revient à 40.
            if (_playersPoints[otherPlayer] == 4)
            {
                _playersPoints[otherPlayer]--;
            }
            // Sinon si on est en avantage ou (à 40 et que l'autre a moins de 40), on incrémente le set et on fixe les points des 2 joueurs à 0.
            else if (_playersPoints[currentPlayer] == 4 || (_playersPoints[currentPlayer] == 3 && _playersPoints[otherPlayer] < 3))
            {
                _playersPoints[currentPlayer] = 0;
                _playersPoints[otherPlayer] = 0;

                _playersGames[currentPlayer]++;
                EndOfGameVerification(currentPlayer);
            }
            // Sinon on incrémente les points de ce joueur.
            else
            {
                _playersPoints[currentPlayer]++;
            }
        }

        ScoreUpdate();
    }

    private void ScoreUpdate()
    {
        string scoreLog = "";

        foreach(KeyValuePair<ControllersParent, Player> kvp in _playerControllersAssociated)
        {
            Player player = kvp.Value;
            scoreLog += $"{player.Name} (Games : {_playersGames[player]} ; Points : {_playersPoints[player]}) - ";
        }

        Debug.Log(scoreLog);
    }

    private void EndOfGameVerification(Player gameWinner)
    {
        if (_playersGames[gameWinner] == 2)
        {
            EndOfGame(gameWinner);
        }
    }

    private void EndOfGame(Player finalWinner)
    {
        ScoreUpdate();
        Debug.Log($"End of the game - The winner is : {finalWinner.Name}");
    }

    private Player GetOtherPlayer(ControllersParent currentPlayer)
    {
        foreach (KeyValuePair<ControllersParent, Player> kvp in _playerControllersAssociated) 
        {
            if (kvp.Key != currentPlayer)
            {
                return kvp.Value;
            }
        }

        return null;
    }

    public string GetPlayerName(ControllersParent currentPlayer)
    {
        foreach (KeyValuePair<ControllersParent, Player> kvp in _playerControllersAssociated)
        {
            if (kvp.Key == currentPlayer)
            {
                return kvp.Value.Name;
            }
        }

        return null;
    }
}
