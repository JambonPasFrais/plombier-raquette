using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private GameObject _mainMenu;
	[SerializeField] private Transform _menusParent;

	private void Start()
	{
		gameObject.SetActive(true);

		for (int i = 0; i < _menusParent.childCount; i++)
		{
			_menusParent.GetChild(i).gameObject.SetActive(false);
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
