using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreGestion : MonoBehaviour
{
	[SerializeField] private ScoreManager _sm;

	private void Awake()
	{
		_sm = gameObject.GetComponent<ScoreManager>();	
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			_sm.AddPoint(Teams.TEAM1);
		}

		else if (Input.GetMouseButtonDown(1))
		{
			_sm.AddPoint(Teams.TEAM2);
		}
	}
}
