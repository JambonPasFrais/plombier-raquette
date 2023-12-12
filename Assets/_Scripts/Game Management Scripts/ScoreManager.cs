using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    #region PRIVATE FIELDS

    private List<Tuple<int, int>> _score = new List<Tuple<int, int>>(5);

	private Tuple<int, int> _currentGameScore = new Tuple<int, int>(0, 0);

	private static List<string> _possiblePoints = new List<string>(5)
	{
		"0",
		"15",
		"30",
		"40",
		"AD"
	};
	private int _currentSetIndex = 0;
	private Tuple<int, int> _nbOfSets = new Tuple<int, int>(0, 0);
	[SerializeField] private int _nbOfSetsToWin = 3;
	[SerializeField] private int _nbOfGamesToWin = 6;
	[SerializeField] private bool _isTieBreak = false;
	[SerializeField] private TextMeshProUGUI _scoreText;

	#endregion

	private void Start()
	{
		_score.Add(new Tuple<int, int>(0, 0));
	}

	public void AddPoint(Teams winnerTeam)
	{
		int winnerPoints = winnerTeam == Teams.TEAM1 ? _currentGameScore.Item1 : _currentGameScore.Item2;
		int loserPoints = winnerTeam == Teams.TEAM1 ? _currentGameScore.Item2 : _currentGameScore.Item1;

		if (!_isTieBreak)
		{
			if ((winnerPoints == 3 && loserPoints < 3) || (winnerPoints == 4 && loserPoints == 3))
			{
				AddGame(winnerTeam);
			}
			else if (winnerPoints == 3 && loserPoints == 4)
			{
				_currentGameScore = new Tuple<int, int>(3, 3);
				GameManager.Instance.ServiceManager.SetServiceBoxCollider(false);
				GameManager.Instance.SideManager.SetSidesInSimpleMatch(GameManager.Instance.Controllers, GameManager.Instance.ServiceManager.ServeRight,
					!GameManager.Instance.ServiceManager.ChangeSides);
			}
			else
			{
				if(winnerTeam == Teams.TEAM1)
					_currentGameScore = new Tuple<int, int>(_currentGameScore.Item1 + 1, _currentGameScore.Item2);
				else
					_currentGameScore = new Tuple<int, int>(_currentGameScore.Item1, _currentGameScore.Item2 + 1);

                GameManager.Instance.ServiceManager.SetServiceBoxCollider(false);
                GameManager.Instance.SideManager.SetSidesInSimpleMatch(GameManager.Instance.Controllers, GameManager.Instance.ServiceManager.ServeRight,
                    !GameManager.Instance.ServiceManager.ChangeSides);
            }
		}
		else
		{
			if(winnerTeam == Teams.TEAM1)
			{
				_currentGameScore = new Tuple<int, int>(_currentGameScore.Item1 + 1, _currentGameScore.Item2);
				winnerPoints++;
            }
			else
			{
				_currentGameScore = new Tuple<int, int>(_currentGameScore.Item1, _currentGameScore.Item2 + 1);
				winnerPoints++;
            }

			if (winnerPoints >= 7 && winnerPoints >= loserPoints + 2)
			{
				AddGame(winnerTeam);
			}
			else if ((_currentGameScore.Item1 + _currentGameScore.Item2) % 6 == 0)
			{
                GameManager.Instance.ServiceOnOriginalSide = !GameManager.Instance.ServiceOnOriginalSide;
                GameManager.Instance.ServiceManager.SetServiceBoxCollider(false);
                GameManager.Instance.SideManager.SetSidesInSimpleMatch(GameManager.Instance.Controllers, GameManager.Instance.ServiceManager.ServeRight,
                    !GameManager.Instance.ServiceManager.ChangeSides);
            }
			else if ((_currentGameScore.Item1 + _currentGameScore.Item2) % 2 == 1)
			{
                GameManager.Instance.ChangeServer();
                GameManager.Instance.ServiceManager.SetServiceBoxCollider(false);
                GameManager.Instance.SideManager.SetSidesInSimpleMatch(GameManager.Instance.Controllers, GameManager.Instance.ServiceManager.ServeRight,
                    !GameManager.Instance.ServiceManager.ChangeSides);
            }
        }

		if(_nbOfSets.Item1 != _nbOfSetsToWin && _nbOfSets.Item2 != _nbOfSetsToWin)
		{
            GetScore();
        }
    }

	public void AddGame(Teams winnerTeam)
    {
		GameManager.Instance.ChangeServer();

		_currentGameScore = new Tuple<int, int>(0, 0);

		Tuple<int, int> newScore = _score[^1];

		if (winnerTeam == Teams.TEAM1)
			newScore = new Tuple<int, int>(newScore.Item1 + 1, newScore.Item2);
		else
			newScore = new Tuple<int, int>(newScore.Item1, newScore.Item2 + 1);

		_score[_currentSetIndex] = newScore;

        GameManager.Instance.ServiceManager.SetServiceBoxCollider(true);
        GameManager.Instance.SideManager.SetSidesInSimpleMatch(GameManager.Instance.Controllers, GameManager.Instance.ServiceManager.ServeRight,
            !GameManager.Instance.ServiceManager.ChangeSides);

		// If the players changed sides, the field border points ownership needs to be changed.
		if (GameManager.Instance.ServiceManager.NbOfGames == 1)
		{
			GameManager.Instance.ChangeFieldBorderPointsOwnership();
		}

		if ((_score[_currentSetIndex].Item1 + _score[_currentSetIndex].Item2) % 2 == 1)
		{
            GameManager.Instance.ServiceOnOriginalSide = !GameManager.Instance.ServiceOnOriginalSide;
        }

        if (((_score[_currentSetIndex].Item1 == _nbOfGamesToWin && _score[_currentSetIndex].Item1 >= _score[_currentSetIndex].Item2 + 2) ||
			_score[_currentSetIndex].Item1 == _nbOfGamesToWin + 1) || (_nbOfGamesToWin == 1 && _score[_currentSetIndex].Item1 == _nbOfGamesToWin))
		{
			_isTieBreak = false;
			ChangeSet(1);
		}
		else if(((_score[_currentSetIndex].Item2 == _nbOfGamesToWin && _score[_currentSetIndex].Item2 >= _score[_currentSetIndex].Item1 + 2) ||
			_score[_currentSetIndex].Item2 == _nbOfGamesToWin + 1) || (_nbOfGamesToWin == 1 && _score[_currentSetIndex].Item2 == _nbOfGamesToWin))
		{
			_isTieBreak = false;
			ChangeSet(2);
		}
		else if(_score[_currentSetIndex].Item1 == _nbOfGamesToWin && _score[_currentSetIndex].Item2 == _nbOfGamesToWin)
		{
			_isTieBreak = true;
			_currentGameScore = new Tuple<int, int>(0, 0);
        }
	}

	public void ChangeSet(int player)
    {
		_currentSetIndex++;

		GameManager.Instance.ServiceManager.ChangeSides = false;
		GameManager.Instance.ServiceManager.NbOfGames = 0;
		GameManager.Instance.ServiceManager.GlobalGamesCount = 0;

		if (player == 1)
			_nbOfSets = new Tuple<int, int>(_nbOfSets.Item1 + 1, _nbOfSets.Item2);
		else
			_nbOfSets = new Tuple<int, int>(_nbOfSets.Item1, _nbOfSets.Item2 + 1);

		Debug.Log($"{_nbOfSets.Item1} set à {_nbOfSets.Item2}");

		if (_nbOfSets.Item1 == _nbOfSetsToWin)
		{
			string score = "";

			foreach (var item in _score)
			{
				score += $"{item.Item1}/{item.Item2} ";
			}

			Debug.Log($"Player1 wins with the score of : {score}");

			GameManager.Instance.EndOfGame();
		}
		else if (_nbOfSets.Item2 == _nbOfSetsToWin)
		{
			string score = "";

			foreach(var item in _score)
			{
				score += $"{item.Item1}/{item.Item2} ";
			}

			Debug.Log($"Player2 wins with the score of : {score}");

            GameManager.Instance.EndOfGame();
        }
        else
			_score.Add(new Tuple<int, int>(0, 0));
	}

	public string GetScore()
	{
		string score = "";

		for (int i = 0; i < _currentSetIndex + 1; i++)
		{
			score += $"{_score[i].Item1}/{_score[i].Item2} ";
		}

		if (!_isTieBreak)
			score += $"{_possiblePoints[_currentGameScore.Item1]} - {_possiblePoints[_currentGameScore.Item2]} ";
		else
			score += $"{_currentGameScore.Item1} - {_currentGameScore.Item2}";

		_scoreText.text = score;

		return score;
	}

	public void ResetScore()
	{
		_score = new List<Tuple<int, int>>()
		{
			new Tuple<int,int>(0,0),
		};
		_currentGameScore = new Tuple<int, int>(0, 0);
		
		_currentSetIndex = 0;
		_nbOfSets = new Tuple<int, int>(0, 0);
		
		_isTieBreak = false;

        _scoreText.text = "";
	}
}
