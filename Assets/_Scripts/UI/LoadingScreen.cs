using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private GameObject _mainMenu;

	private void Start()
	{
		if (GameParameters.CurrentTournamentInfos.CurrentRound == 0)
		{
			gameObject.SetActive(true);
			_mainMenu.SetActive(false);
		}
		else
		{
			gameObject.SetActive(false);
			_mainMenu.SetActive(true);
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			gameObject.SetActive(false);
			_mainMenu.SetActive(true);
		}
	}
}
