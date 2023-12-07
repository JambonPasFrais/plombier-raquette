using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TournamentSelection : MonoBehaviour
{
    public void SetDifficulty(int difficulty)
    {
        GameParameters.Instance.SetTournament(difficulty);
    }
}
