using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TournamentSelection : MonoBehaviour
{
    [SerializeField] private GameObject _starCup;
    public void SetDifficulty(int difficulty)
    {
        GameParameters.Instance.SetTournament(difficulty);
    }

	private void Update()
	{
        _starCup.transform.Rotate(new Vector3(0f, 0f, .01f));
	}
}
