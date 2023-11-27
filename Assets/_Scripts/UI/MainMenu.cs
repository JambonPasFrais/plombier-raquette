using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
	[SerializeField] private GameObject _mainMenu;
    [SerializeField] private GameObject _freeGameMenu;
    [SerializeField] private GameObject _tournamentMenu;
    [SerializeField] private GameObject _optionsMenu;
    [SerializeField] private GameObject _characterSelectionMenu;

	private void Start()
	{
		ReturnToMainMenu();
	}

	public void ShowFreeGameMenu()
    {
        _mainMenu.SetActive(false);
        _freeGameMenu.SetActive(true);
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

    public void ReturnToMainMenu()
    {
        _mainMenu.SetActive(true);
        _optionsMenu.SetActive(false);
        _freeGameMenu.SetActive(false);
        _characterSelectionMenu.SetActive(false);
        _tournamentMenu.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
