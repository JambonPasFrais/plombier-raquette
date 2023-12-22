using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;


public class TournamentEndMenu : MonoBehaviour
{
    [SerializeField] private GameObject _winnerDisplay;
    [SerializeField] private GameObject _loserDisplay;
    [SerializeField] private Image _currentCup;
    [SerializeField] private Transform _winnerPlayerLocation;
    [SerializeField] private Transform _loserPlayerLocation;
	[SerializeField] private GameObject _continueText;
    //[SerializeField] private EventSystem _eventSystem;
    [SerializeField] private TextMeshProUGUI _pressInputTextWin;
    [SerializeField] private TextMeshProUGUI _pressInputTextLose;
	[SerializeField] private TextMeshProUGUI _pressInputGlobal;
    private InputSystemUIInputModule _inputSystemUIInputModule;

    [SerializeField]private bool _canReturn = false;
    private int _factor = -1;

    private void Start()
    {
		_inputSystemUIInputModule = MenuManager.Instance.CurrentEventSystem.gameObject.GetComponent<InputSystemUIInputModule>();
    }

    public void SetWinnerMenu(GameObject winnerPrefab, Sprite cupSprite)
    {
		_pressInputGlobal = _pressInputTextWin;
		_continueText.SetActive(false);
		_canReturn = false;
		_winnerDisplay.SetActive(true);
		_loserDisplay.SetActive(false);
		GameParameters.IsTournamentMode = false;
		_currentCup.sprite = cupSprite;
        GameObject go = Instantiate(winnerPrefab, _winnerPlayerLocation.transform);
        go.transform.localPosition = Vector3.zero;
		go.transform.localScale = new Vector3(30, 30, 30);
		go.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
		StartCoroutine(WaitBeforeCanReturn());
	}

	public void SetLoserMenu(GameObject loserPrefab)
	{
		_pressInputGlobal = _pressInputTextLose;
        _continueText.SetActive(false);
		_canReturn = false;
		_loserDisplay.SetActive(true);
		_winnerDisplay.SetActive(false);
		GameParameters.IsTournamentMode = false;
		GameObject go = Instantiate(loserPrefab, _loserPlayerLocation.transform);
		go.transform.localPosition = Vector3.zero;
		go.transform.localScale = new Vector3(20, 20, 20);
		go.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
		StartCoroutine(WaitBeforeCanReturn());
    }

    private void Update()
    {
		if(_inputSystemUIInputModule.submit.action.WasPressedThisFrame() && _canReturn)
		{
			if (_winnerPlayerLocation.childCount != 0)
			{
				Destroy(_winnerPlayerLocation.GetChild(0).gameObject);
			}
			else
				Destroy(_loserPlayerLocation.GetChild(0).gameObject);

			gameObject.SetActive(false);
			GameParameters.Instance.CurrentTournamentInfos.Reset();
			MenuManager.Instance.GoBackToMainMenu();
		}
        _pressInputGlobal.alpha = _pressInputGlobal.alpha + (1 * _factor * Time.deltaTime);

        if (_pressInputGlobal.alpha < 0 || _pressInputGlobal.alpha > 1)
            _factor *= -1;
    }

    private IEnumerator WaitBeforeCanReturn()
	{
		yield return new WaitForSeconds(3f);
		_canReturn = true;
		_continueText.SetActive(true);
	}
}
