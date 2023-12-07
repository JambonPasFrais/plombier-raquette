using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TournamentEndMenu : MonoBehaviour
{
    [SerializeField] private GameObject WinnerDisplay;
    [SerializeField] private GameObject LoserDisplay;
    [SerializeField] private Image _currentCup;
    [SerializeField] private Transform _playerLocation;

    public void SetWinnerMenu(GameObject winnerPrefab, Sprite cupSprite)
    {
        gameObject.SetActive(true);
        _currentCup.sprite = cupSprite;
        GameObject go = Instantiate(winnerPrefab, _playerLocation.transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = new Vector3(30, 30, 30);
        go.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
    }
}
