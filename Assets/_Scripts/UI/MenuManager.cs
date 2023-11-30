using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> _visitedMenus = new List<GameObject>();

    public void GoToNextMenu(GameObject nextMenu)
    {
        nextMenu.SetActive(true);
        _visitedMenus.Last().SetActive(false);
        _visitedMenus.Add(nextMenu);
    }

    public void GoToPreviousMenu()
    {
        _visitedMenus.Last().SetActive(false);
        _visitedMenus.Remove(_visitedMenus.Last());
		_visitedMenus.Last().SetActive(true);
	}
}
