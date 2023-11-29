using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private GameObject _mainMenu;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			gameObject.SetActive(false);
			_mainMenu.SetActive(true);
		}
	}
}
