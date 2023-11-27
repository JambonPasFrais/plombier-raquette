using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ServiceManager : MonoBehaviour
{
    private static ServiceManager _instance;
	[SerializeField] private Transform _serviceBoxesParentSide1;
	[SerializeField] private Transform _serviceBoxesParentSide2;
	[SerializeField] private List<GameObject> _lockServiceMovementColliders;
	//[SerializeField] private GameObject _lockServiceMovementCollidersSide2;

	private List<GameObject> _serviceBoxesSide1 = new List<GameObject>();
	private List<GameObject> _serviceBoxesSide2 = new List<GameObject>();

	[SerializeField] private int _lastServiceLateralSide = 1;
	[SerializeField] private bool _changeSides;
	[SerializeField] private int _nbOfGames = 0;
	

	public static ServiceManager Instance => _instance;


	private void Awake()
	{
		if (_instance == null)
			_instance = this;
	}

	private void Start()
	{
		for(int i = 0; i < 2; i++)
		{
			_serviceBoxesSide1.Add(_serviceBoxesParentSide1.GetChild(i).gameObject);
			_serviceBoxesSide2.Add(_serviceBoxesParentSide2.GetChild(i).gameObject);
		}
		_nbOfGames = 0;
		_changeSides = false;
		SetServiceBoxCollider(false);
	}

	public void SetServiceBoxCollider(bool newGame)
	{
		if (newGame)
		{
			_nbOfGames = (_nbOfGames + 1) % 2;
			_lastServiceLateralSide = 0;

			for (int i = 0; i < 2; i++)
			{
				_serviceBoxesSide1[i].SetActive(false);
				_serviceBoxesSide2[i].SetActive(false);
				_lockServiceMovementColliders[i].SetActive(false);
			}

			if (_nbOfGames == 0)
				_changeSides = !_changeSides;
		}

		else
		{
			_lastServiceLateralSide = (_lastServiceLateralSide + 1) % 2;
		}		

		if (_changeSides)
		{
			_serviceBoxesSide2[_lastServiceLateralSide].SetActive(true);
			_serviceBoxesSide2[(_lastServiceLateralSide + 1) % 2].SetActive(false);
			EnableLockServiceColliders(0);
		}
		else
		{
			_serviceBoxesSide1[_lastServiceLateralSide].SetActive(true);
			_serviceBoxesSide1[(_lastServiceLateralSide + 1) % 2].SetActive(false);
			EnableLockServiceColliders(1);
		}
	}

	private void EnableLockServiceColliders(int side)
	{
		_lockServiceMovementColliders[side].SetActive(true);
	}

	public void DisableLockServiceColliders()
	{
		foreach(var item in _lockServiceMovementColliders)
			item.SetActive(false);
	}
}
