using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameMode
{
    [SerializeField] private string _name;
	[SerializeField] private int _nbOfSets;
	[SerializeField] private int _nbOfGames;

	public string Name => _name;
	public int NbOfSets => _nbOfSets;
	public int NbOfGames => _nbOfGames;

	public GameMode(string name, int nbOfSets, int nbOfGames)
	{
		_name = name;
		_nbOfSets = nbOfSets;
		_nbOfGames = nbOfGames;
	}
}
