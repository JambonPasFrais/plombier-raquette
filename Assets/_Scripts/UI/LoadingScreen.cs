using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private GameObject _mainMenu;

	private void Start()
	{
		gameObject.SetActive(true);
		_mainMenu.SetActive(false);
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
