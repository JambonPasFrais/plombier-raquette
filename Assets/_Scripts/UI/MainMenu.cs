using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
	[SerializeField] private GameObject _mainMenu;
    [SerializeField] private GameObject _localGameMenu;
    [SerializeField] private GameObject _onlineGameMenu;
    [SerializeField] private GameObject _tournamentMenu;
    [SerializeField] private GameObject _optionsMenu;

    public void ShowLocalGameMenu()
    {
        _mainMenu.SetActive(false);
        _localGameMenu.SetActive(true);
    }
    
    public void ShowOnlineGameMenu()
    {
        _mainMenu.SetActive(false);
        _onlineGameMenu.SetActive(true);
    }
    
    public void ShowTournamentMenu()
    {
        _mainMenu.SetActive(false);
        _tournamentMenu.SetActive(true);
    }

	public void ShowOptionsMenu()
	{
		_mainMenu.SetActive(false);
		_optionsMenu.SetActive(true);
	}

    public void QuitGame()
    {
        Application.Quit();
    }
}
