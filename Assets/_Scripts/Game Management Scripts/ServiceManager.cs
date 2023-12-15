using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ServiceManager : MonoBehaviour
{
    #region PUBLIC FIELDS

    public bool ChangeSides;
    public int NbOfGames = 0;

    #endregion

    #region PRIVATE FIELDS

    [SerializeField] private List<GameObject> _lockServiceMovementColliders;

	[SerializeField] private bool _serveRight = false;

	private int _globalGamesCount;

    #endregion

    #region GETTERS & SETTERS

    public bool ServeRight { get { return _serveRight; } }
	public int GlobalGamesCount { set { _globalGamesCount = value; } }

    #endregion

    private void Awake()
    {
        _serveRight = false;
        NbOfGames = 0;
		_globalGamesCount = 0;
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
			_globalGamesCount++;
            NbOfGames = (NbOfGames + 1) % 2;
			_serveRight = true;

			DisableLockServiceColliders();

            if (NbOfGames == 1)
				ChangeSides = !ChangeSides;
		}
		else
		{
			_serveRight = !_serveRight;
		}

		EnableLockServiceColliders();
    }

	/// <summary>
	/// Activates the colliders of a specific side of the field.
	/// </summary>
	/// <param name="side"></param>
	public void EnableLockServiceColliders()
	{
        int sideIndex = (_globalGamesCount % 4) / 2;
        _lockServiceMovementColliders[sideIndex].SetActive(true);
	}

	/// <summary>
	/// Disables all the service colliders.
	/// </summary>
	public void DisableLockServiceColliders()
	{
		foreach(var item in _lockServiceMovementColliders)
			item.SetActive(false);
	}

    public void SetServiceOnline(bool newGame)
    {
        GameManager.Instance.photonView.RPC("SetServiceBoxColliderOnline", RpcTarget.All, newGame);
    }


	[PunRPC]
    public void SetServiceBoxColliderOnline(bool newGame)
    {
        if (newGame)
        {
            _globalGamesCount++;
            NbOfGames = (NbOfGames + 1) % 2;
            _serveRight = true;

            DisableLockServiceColliders();

            if (NbOfGames == 1)
                ChangeSides = !ChangeSides;
        }
        else
        {
            _serveRight = !_serveRight;
        }

        EnableLockServiceColliders();
    }
}
