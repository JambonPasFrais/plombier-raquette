using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private GameObject _mainMenu;
	[SerializeField] private TextMeshProUGUI _pressInputText;
	private int _factor = -1;

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

		_factor = -1;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			gameObject.SetActive(false);
			_mainMenu.SetActive(true);
		}

		_pressInputText.alpha = _pressInputText.alpha + (1 * _factor * Time.deltaTime);

		if (_pressInputText.alpha < 0 || _pressInputText.alpha > 1)
			_factor *= -1;
	}
}
