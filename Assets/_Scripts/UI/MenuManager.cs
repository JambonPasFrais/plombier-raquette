using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour
{
	private static MenuManager _instance;
	[SerializeField] private List<CharacterData> _characters = new List<CharacterData>();
	[SerializeField] private Transform _charactersModelsParent;
    [SerializeField] private List<GameObject> _visitedMenus = new List<GameObject>();
    [SerializeField] private EventSystem _eventSystem;
	private Dictionary<string, GameObject> _charactersModel = new Dictionary<string, GameObject>();

	public static List<CharacterData> Characters => _instance._characters;
	public static Dictionary<string, GameObject> CharactersModel => _instance._charactersModel;
	public static Transform CharactersModelsParent => _instance._charactersModelsParent;

	private void Awake()
	{
		if(_instance == null)
			_instance = this;

		InitCharactersModel();
	}
	
    private void Start()
    {
        _visitedMenus.Add(transform.GetChild(0).gameObject);
        SetDefaultSelected(_visitedMenus.Last());
    }

    public void GoToNextMenu(GameObject nextMenu)
    { 
		nextMenu.SetActive(true);
        _visitedMenus.Last().SetActive(false);// Inactive menu break the controllers of the controllerManager
        _visitedMenus.Add(nextMenu);
        SetDefaultSelected(nextMenu);
    }
    public void SetFirstSelectedButton(GameObject Button)
    {
      _eventSystem.SetSelectedGameObject(Button);
    }
    public void GoToPreviousMenu()
    {
        _visitedMenus.Last().SetActive(false);
        _visitedMenus.Remove(_visitedMenus.Last());
		_visitedMenus.Last().SetActive(true);
        SetDefaultSelected(_visitedMenus.Last());
	}

	public void InitCharactersModel()
	{
		GameObject go;

		foreach (var item in _characters)
		{
			if (item != _characters.Last())
			{
				go = Instantiate(item.Model3D, _charactersModelsParent);
				go.name = item.Name;
				go.SetActive(false);
				_charactersModel.Add(item.Name, go);
			}
			else
			{
				for (int i = 0; i < 4; i++)
				{
					go = Instantiate(item.Model3D, _charactersModelsParent);
					go.name = item.Name + i;
					go.SetActive(false);
					_charactersModel.Add(item.Name + i, go);
				}
			}
		}
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
