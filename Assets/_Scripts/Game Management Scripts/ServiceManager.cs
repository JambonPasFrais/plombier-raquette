using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ServiceManager : MonoBehaviour
{
    #region PUBLIC FIELDS

    public bool ChangeSides;

    #endregion

    #region PRIVATE FIELDS

    [SerializeField] private List<GameObject> _lockServiceMovementColliders;

	[SerializeField] private int _lastServiceLateralSide = 0;
	[SerializeField] private int _nbOfGames = 0;

    #endregion

    #region GETTERS

    public int LastServiceLateralSide { get { return _lastServiceLateralSide; } }

	#endregion

	private void Start()
	{
		_lastServiceLateralSide = 1;
        _nbOfGames = 0;
		ChangeSides = false;
	}

	/// <summary>
	/// Places the restraining colliders on the serving player's side of the field, according to the side changes of the tennis rules.
	/// </summary>
	/// <param name="newGame"></param>
	public void SetServiceBoxCollider(bool newGame)
	{
		if (newGame)
		{
			_nbOfGames = (_nbOfGames + 1) % 2;
			_lastServiceLateralSide = 0;

			DisableLockServiceColliders();

            if (_nbOfGames == 1)
				ChangeSides = !ChangeSides;
		}
		else
		{
			_lastServiceLateralSide = (_lastServiceLateralSide + 1) % 2;
		}		

		if (!ChangeSides)
		{
			EnableLockServiceColliders(0);
		}
		else
		{
			EnableLockServiceColliders(1);
		}
	}

	/// <summary>
	/// Activates the colliders of a specific side of the field.
	/// </summary>
	/// <param name="side"></param>
	private void EnableLockServiceColliders(int side)
	{
		_lockServiceMovementColliders[side].SetActive(true);
	}

	/// <summary>
	/// Disables all the service colliders.
	/// </summary>
	public void DisableLockServiceColliders()
	{
		foreach(var item in _lockServiceMovementColliders)
			item.SetActive(false);
	}
}
