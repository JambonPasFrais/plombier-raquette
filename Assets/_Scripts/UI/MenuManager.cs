using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
	private static MenuManager _instance;
	[SerializeField] private List<CharacterData> _characters = new List<CharacterData>();
	[SerializeField] private Transform _charactersModelsParent;
	private List<GameObject> _visitedMenus = new List<GameObject>();
	[SerializeField] private EventSystem _eventSystem;
	private Dictionary<string, GameObject> _charactersModel = new Dictionary<string, GameObject>();
	[SerializeField] private GameObject _tournamentBracketMenu;
	[SerializeField] private TextMeshProUGUI _numberOfControllersConnectedOnMenu;

	public static MenuManager Instance => _instance;
	public  List<CharacterData> Characters => _characters;
	public Dictionary<string, GameObject> CharactersModel => _charactersModel;
	public Transform CharactersModelsParent => _charactersModelsParent;
	public EventSystem CurrentEventSystem => _eventSystem;
	public TextMeshProUGUI NumberOfControllersConnectedOnMenu => _numberOfControllersConnectedOnMenu;

	private void Awake()
	{
		if (_instance == null)
			_instance = this;

		InitCharactersModel();
	}
	
    private void Start()
    {
		if (GameParameters.Instance.CurrentTournamentInfos.CurrentRound == 0)
		{
			transform.GetChild(0).gameObject.SetActive(true);

			for (int i = 1; i < transform.childCount; i++)
			{
				transform.GetChild(i).gameObject.SetActive(false);
			}
		}

		else
		{
			_tournamentBracketMenu.SetActive(true);
			_tournamentBracketMenu.GetComponent<TournamentBracket>().SetCurrentBracket(GameParameters.Instance.CurrentTournamentInfos);
		}

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
				go = Instantiate(item.BasicModel, _charactersModelsParent);
				go.name = item.Name;
				go.SetActive(false);
				_charactersModel.Add(item.Name, go);
			}
			else
			{
				for (int i = 0; i < 4; i++)
				{
					go = Instantiate(item.BasicModel, _charactersModelsParent);
					go.name = item.Name + i;
					go.SetActive(false);
					_charactersModel.Add(item.Name + i, go);
				}
			}
		}
	}

	public CharacterData ReturnRandomCharacter(List<CharacterData> availableCharacters)
	{
		CharacterData data;
		int currentIndex;

		System.Random rand = new System.Random();

		currentIndex = rand.Next(availableCharacters.Count);
		data = availableCharacters[currentIndex];
		availableCharacters.RemoveAt(currentIndex);

		return data;
	}

	public void GoBackToMainMenu()
	{
		_visitedMenus[0].SetActive(true);
		GameObject mainMenu = _visitedMenus[0];

		_visitedMenus.Clear();
		_visitedMenus.Add(mainMenu);
	}
        
    private void SetDefaultSelected(GameObject menu)
    {
        GameObject firstSelectable = menu.GetComponentInChildren<Selectable>()?.gameObject;

        if (firstSelectable != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectable);
        }
    }

	public void SetButtonNavigation(Button currentButton, Selectable leftObject, Selectable rightObject, Selectable upObject, Selectable downObject)
	{
		Navigation navigation = new Navigation();

		navigation.mode = Navigation.Mode.Explicit;

		navigation.selectOnRight = rightObject;
		navigation.selectOnLeft = leftObject;
		navigation.selectOnUp = upObject;
		navigation.selectOnDown = downObject;

		currentButton.navigation = navigation;
	}

	public void QuitGame()
	{
		Application.Quit();
	}

	public void PlaySound(string soundName)
	{
		AudioManager.Instance.PlaySfx(soundName);
	}
}
