using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private GameObject _menusContainer;
	[SerializeField] private TextMeshProUGUI _pressInputText;
	private int _factor = -1;

	private void Start()
	{
		if (GameParameters.Instance.CurrentTournamentInfos.CurrentRound == 0)
		{
			gameObject.SetActive(true);
			_menusContainer.SetActive(false);
		}
		else
		{
			gameObject.SetActive(false);
			_menusContainer.SetActive(true);
		}

		_factor = -1;
	}

	private void Update()
	{
		if (MenuManager.Instance.CurrentEventSystem.gameObject.GetComponent<InputSystemUIInputModule>().submit.action.WasPressedThisFrame())
		{
			MenuManager.Instance.PlaySound("LoadingScreenTransition");
			gameObject.SetActive(false);
			_menusContainer.SetActive(true);
		}

		_pressInputText.alpha = _pressInputText.alpha + (1 * _factor * Time.deltaTime);

		if (_pressInputText.alpha < 0 || _pressInputText.alpha > 1)
			_factor *= -1;
	}
}
