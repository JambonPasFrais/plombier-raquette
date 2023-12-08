using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable, CreateAssetMenu(fileName = "TournamnentInfos", menuName = "ScriptableObjects/Tournament/TournamentInfos")]
public class TournamentInfos : ScriptableObject
{
    public Dictionary<int, List<CharacterData>> RoundPlayers =new Dictionary<int, List<CharacterData>>();
    public List<CharacterData> FirstRoundPlayers;
    public List<CharacterData> SecondRoundPlayers;
    public List<CharacterData> ThirdRoundPlayers;
    
    public void SetRoundPlayers(List<CharacterData> firstRound, List<CharacterData> secondRound, List<CharacterData> thirdRound)
    {
        RoundPlayers.Clear();

        for(int i = 0; i < 3; i++)
        {
            switch (i)
            {
                case 0:
                    RoundPlayers.Add(i, firstRound);
                    FirstRoundPlayers = new List<CharacterData>(RoundPlayers.GetValueOrDefault(i));
                    break;
                case 1:
                    if (secondRound != null)
                    {
                        RoundPlayers.Add(i, secondRound);
                        SecondRoundPlayers = new List<CharacterData>(RoundPlayers.GetValueOrDefault(i));
                    }
                    break;
				case 2:
                    if (thirdRound != null)
                    {
                        RoundPlayers.Add(i, thirdRound);
                        ThirdRoundPlayers = new List<CharacterData>(RoundPlayers.GetValueOrDefault(i));
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
		RoundPlayers.Clear();
        FirstRoundPlayers.Clear();
        SecondRoundPlayers.Clear();
        ThirdRoundPlayers.Clear();
	}
}
