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

	[SerializeField] private bool _serveRight = false;
	[SerializeField] private int _nbOfGames = 0;

    #endregion

    #region GETTERS & SETTERS

    public bool ServeRight { get { return _serveRight; } }
	public int NbOfGames { set { _nbOfGames = value; } }

    #endregion

    private void Awake()
    {
        _serveRight = false;
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
			_serveRight = true;

			DisableLockServiceColliders();

            if (_nbOfGames == 1)
				ChangeSides = !ChangeSides;
		}
		else
		{
			_serveRight = !_serveRight;
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
