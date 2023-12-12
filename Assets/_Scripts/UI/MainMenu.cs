using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject _loadingScreen;
	[SerializeField] private GameObject _mainMenu;

	private void Start()
	{
        _mainMenu.SetActive(false);
        _loadingScreen.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
