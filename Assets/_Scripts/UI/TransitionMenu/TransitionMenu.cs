using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TransitionMenu : MonoBehaviour
{
    [SerializeField] private GameObject _playerInfoPrefab;
    [SerializeField] private Transform _playerInfosContainer;
    [SerializeField] private Transform _playerLocationsParent;
    [SerializeField] private List<Image> _playerTypeImages = new List<Image>(); 
    [SerializeField] private List<TextMeshProUGUI> _playerTypeTexts = new List<TextMeshProUGUI>();
    [SerializeField] private GameObject _loadingObject;

    private List<CharacterData> _playersCharacters;

	public void OnMenuLoad()
    {
        if (!PhotonNetwork.IsConnected)
        {
            _playersCharacters = new List<CharacterData>(GameParameters.Instance.PlayersCharacter);

            for (int i = 0; i < _playersCharacters.Count; i++)
            {
                GameObject go = Instantiate(_playerInfoPrefab, _playerInfosContainer);
                string playerType = i >= GameParameters.Instance.LocalNbPlayers ? "COM" : $"P{i + 1}";
                go.GetComponent<PlayerInfos>().Init(_playersCharacters[i], playerType);
            }

            foreach (Transform child in _playerLocationsParent)
                child.gameObject.SetActive(false);

            StartCoroutine(PlayersInstantiation());
        }
	}

    private IEnumerator PlayersInstantiation()
    {
        for (int i = 0; i < _playersCharacters.Count; i++)
        {
			yield return new WaitForSeconds(1f);

            _playerLocationsParent.GetChild(i).gameObject.SetActive(true);
            MenuManager.Instance.PlaySound("CharacterAppearance");
			GameObject go = Instantiate(_playersCharacters[i].BasicModel, _playerLocationsParent.GetChild(i));
            _playerTypeImages[i].color = _playersCharacters[i].CharacterPrimaryColor;
			string playerType = i >= GameParameters.Instance.LocalNbPlayers ? "COM" : $"P{i + 1}";
            _playerTypeTexts[i].text = playerType;
            go.transform.localScale = new Vector3(10, 10, 10);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
        }

        StartCoroutine(WaitBeforePlayMatch());
        StartCoroutine(LoadingObjectRotation());
	}

    private IEnumerator WaitBeforePlayMatch()
    {
        yield return new WaitForSeconds(5f);

        MenuManager.Instance.PlaySound("PlayMatch");

		ControllerManager.Instance.ChangeCtrlersActMapToGame();

		SceneManager.LoadScene("Local_Multiplayer");
	}

    private IEnumerator LoadingObjectRotation()
    {
        while(true)
        {
            _loadingObject.transform.Rotate(0f, 0f, -0.5f);

            yield return new WaitForEndOfFrame();
        }
    }
}
