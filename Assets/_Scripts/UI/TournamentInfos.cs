using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[Serializable, CreateAssetMenu(fileName = "TournamentInfos", menuName = "ScriptableObjects/Tournament/TournamentInfos")]
public class TournamentInfos : ScriptableObject
{
    public Dictionary<int, List<CharacterData>> RoundPlayers = new Dictionary<int, List<CharacterData>>();
    public List<CharacterData> FirstRoundPlayers = new List<CharacterData>();
    public List<CharacterData> SecondRoundPlayers = new List<CharacterData>();
    public List<CharacterData> ThirdRoundPlayers = new List<CharacterData>();
    public CharacterData Winner;
    public int CurrentRound = 0;
    public Sprite CupSprite;
    public CharacterData PlayersCharacter;

	public void SetRoundPlayers(List<CharacterData> firstRound, List<CharacterData> secondRound, List<CharacterData> thirdRound, CharacterData winner)
    {
        RoundPlayers.Clear();

        for(int i = 0; i < 3; i++)
        {
            switch (i)
            {
                case 0:
                    if (secondRound != null)
                    {
						RoundPlayers.Add(0, firstRound);
						RoundPlayers.Add(1, secondRound);
                        FirstRoundPlayers = new List<CharacterData>(firstRound);
                        SecondRoundPlayers = new List<CharacterData>(secondRound);
                        CurrentRound = 1;
                    }
                    break;
				case 1:
                    if (thirdRound != null)
                    {
                        RoundPlayers.Add(2, thirdRound);
                        ThirdRoundPlayers = new List<CharacterData>(thirdRound);
                        CurrentRound = 2;
                    }
                    break;
                case 2:
					if (winner != null)
					{
						RoundPlayers.Add(3, new List<CharacterData>() { winner });
                        Winner = winner;
                        CurrentRound = 3;
					}
					break;
			}
        }
    }

    public List<CharacterData> GetPlayersAtRound(int round)
    {
        return RoundPlayers.GetValueOrDefault(round);
    }

	public void Reset()
	{
        CurrentRound = 0;
		RoundPlayers.Clear();
        FirstRoundPlayers.Clear();
        SecondRoundPlayers.Clear();
        ThirdRoundPlayers.Clear();
        PlayersCharacter = null;
        CupSprite = null;
        Winner = null;
	}
}