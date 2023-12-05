using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> _visitedMenus = new List<GameObject>();

    private void Start()
    {
        _visitedMenus.Add(transform.GetChild(0).gameObject);
        SetDefaultSelected(_visitedMenus.Last());
    }

    public void GoToNextMenu(GameObject nextMenu)
    {
        nextMenu.SetActive(true);
        _visitedMenus.Last().SetActive(false);
        _visitedMenus.Add(nextMenu);
        SetDefaultSelected(nextMenu);
    }

    public void GoToPreviousMenu()
    {
        _visitedMenus.Last().SetActive(false);
        _visitedMenus.Remove(_visitedMenus.Last());
        _visitedMenus.Last().SetActive(true);
        SetDefaultSelected(_visitedMenus.Last());
    }

    private void SetDefaultSelected(GameObject menu)
    {
        GameObject firstSelectable = menu.GetComponentInChildren<Selectable>()?.gameObject;

        if (firstSelectable != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectable);
        }
    }
}
