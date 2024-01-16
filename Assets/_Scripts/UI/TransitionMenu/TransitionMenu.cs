using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionMenu : MonoBehaviour
{
    [SerializeField] private GameObject _playerInfoPrefab;
    [SerializeField] private Transform _playerInfosContainer;
    [SerializeField] private Transform _playerLocationsParent;

    private List<CharacterData> _playersCharacters;
    
    public void OnMenuLoad()
    {
        _playersCharacters = new List<CharacterData>(GameParameters.Instance.PlayersCharacter);

        for(int i = 0; i < _playersCharacters.Count; i++)
        {
            GameObject go = Instantiate(_playerInfoPrefab, _playerInfosContainer);
            string playerType = i >= GameParameters.Instance.LocalNbPlayers ? "COM" : $"P{i + 1}";
            go.GetComponent<PlayerInfos>().Init(_playersCharacters[i], playerType);

			go = Instantiate(_playersCharacters[i].BasicModel, _playerLocationsParent.GetChild(i));
            go.transform.localScale = new Vector3(10, 10, 10);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
		}
	}
}
