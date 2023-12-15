using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TournamentSelection : MonoBehaviour
{
    [SerializeField] private Transform _trophyPosition;
    [SerializeField] private List<TrophyModel> _trophyModels = new List<TrophyModel>();
    [SerializeField] private EventSystem _eventSystem;
    [SerializeField] private GameObject _firstSelectedObject;
    [SerializeField] private GameObject _currentSelectedButton;
    [SerializeField] private float _scalingFactor = 0.05f;
    private int _trophyIndex;
    private Vector3 _currentScalingFactor;

	private void Start()
	{
        GameObject go;

		foreach (var item in _trophyModels)
        {
            go = Instantiate(item.TrophyPrefab, _trophyPosition);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.Euler(item.Rotation);
            /*go.transform.localScale = item.Scale;*/
            go.SetActive(false);
        }

        _currentScalingFactor = Vector3.one * _scalingFactor;
        _trophyIndex = 0;
        _trophyPosition.GetChild(0).gameObject.SetActive(true);
        _currentSelectedButton = _firstSelectedObject;
        _eventSystem.SetSelectedGameObject(_firstSelectedObject);
	}

	public void SetDifficulty(int difficulty)
    {
        GameParameters.Instance.SetTournament(difficulty);
    }

	private void Update()
	{
        if (_trophyPosition.localScale.x > _trophyModels[_trophyIndex].MaxScale.x
            || _trophyPosition.localScale.x < _trophyModels[_trophyIndex].MinScale.x)
			_currentScalingFactor *= -1;

        _trophyPosition.localScale += _currentScalingFactor;

        if (_eventSystem.currentSelectedGameObject != _currentSelectedButton 
            && _eventSystem.currentSelectedGameObject.GetComponent<TrophySelection>().TrophyIndex != -1)
        {
            _trophyPosition.GetChild(_trophyIndex).gameObject.SetActive(false);
            _currentSelectedButton = _eventSystem.currentSelectedGameObject;
            _trophyIndex = _currentSelectedButton.GetComponent<TrophySelection>().TrophyIndex;
            _trophyPosition.GetChild(_trophyIndex).localRotation = Quaternion.Euler(_trophyModels[_trophyIndex].Rotation);
			_currentScalingFactor = Vector3.one * _scalingFactor;
			_trophyPosition.localScale = _trophyModels[_trophyIndex].Scale;
			_trophyPosition.GetChild(_trophyIndex).gameObject.SetActive(true);
		}
        else if (_eventSystem.currentSelectedGameObject.GetComponent<TrophySelection>().TrophyIndex == -1)
        {
			_trophyPosition.GetChild(_trophyIndex).gameObject.SetActive(false);
            _currentSelectedButton = null;
		}
	}
}
