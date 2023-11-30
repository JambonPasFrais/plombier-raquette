using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideManager : MonoBehaviour
{
    #region PRIVATE FIELDS

	[SerializeField] private Transform _servicePointsFirstSideParent;
	[SerializeField] private Transform _servicePointsSecondSideParent;
	[SerializeField] private GameObject _firstSideCollidersParentObject;
	[SerializeField] private GameObject _secondSideCollidersParentObject;
	[SerializeField] private Transform _cameraParent;
	[SerializeField] private List<GameObject> _cameras;

	private Dictionary<string, Transform> _servicePointsFirstSide = new Dictionary<string, Transform>();
	private Dictionary<string, Transform> _servicePointsSecondSide = new Dictionary<string, Transform>();
	private Transform _activeCameraTransform;

    #endregion

    #region GETTERS

	public Transform ActiveCameraTransform { get { return _activeCameraTransform; } }

	#endregion

	private void Awake()
	{
		for (int i = 0; i < _servicePointsFirstSideParent.childCount; i++)
		{
			_servicePointsFirstSide.Add(_servicePointsFirstSideParent.GetChild(i).name, _servicePointsFirstSideParent.GetChild(i));
			_servicePointsSecondSide.Add(_servicePointsSecondSideParent.GetChild(i).name, _servicePointsSecondSideParent.GetChild(i));
		}

		for (int i = 0; i < _cameraParent.childCount; i++)
		{
			_cameras.Add(_cameraParent.GetChild(i).gameObject);
		}
	}

	/// <summary>
	/// Alternates the player's fields and set the players, the cameras and the bot targets to the correct positions for a 1v1 match.
	/// </summary>
	/// <param name="players"></param>
	/// <param name="serveRight"></param>
	/// <param name="originalSides"></param>
	public void ChangeSidesInGameSimple(List<ControllersParent> players, bool serveRight, bool originalSides)
	{
		string side = "";

		side = serveRight ? "Right" : "Left";

		if (originalSides)
		{
			_activeCameraTransform = _cameras[0].transform;
            _cameras[0].SetActive(true);
			_cameras[1].SetActive(false);
			players[0].transform.position = _servicePointsFirstSide[side].position;
			players[0].transform.rotation = _servicePointsFirstSide[side].rotation;
			players[1].transform.position = _servicePointsSecondSide[side].position;
			players[1].transform.rotation = _servicePointsSecondSide[side].rotation;
		}
		else
		{
            _activeCameraTransform = _cameras[1].transform;
            _cameras[0].SetActive(false);
			_cameras[1].SetActive(true);
			players[0].transform.position = _servicePointsSecondSide[side].position;
			players[0].transform.rotation = _servicePointsSecondSide[side].rotation;
			players[1].transform.position = _servicePointsFirstSide[side].position;
			players[1].transform.rotation = _servicePointsFirstSide[side].rotation;
		}

		UpdateBotValues(players, originalSides);
		SetCollidersOwnerPlayers(players, originalSides);
    }

    /// <summary>
    /// Alternates the player's fields and set the players, the cameras and the bot targets to the correct positions in a 2v2 match.
    /// </summary>
    /// <param name="players"></param>
    /// <param name="serveRight"></param>
    /// <param name="_originalSides"></param>
    public void ChangeSidesInGameDouble(List<PlayerController> players, bool serveRight, bool _originalSides)
	{
		string side = "";

		side = serveRight ? "Right" : "Left";

		if (_originalSides)
		{
			_cameras[0].SetActive(true);
			_cameras[1].SetActive(false);
		}
		else
		{
			_cameras[0].SetActive(false);
			_cameras[1].SetActive(true);
		}
	}

	/// <summary>
	/// Method called for placing the bot target points on the correct side of the field and reinitializing its 3D position vector.
	/// Only works for a simple game with a player and a bot.
	/// </summary>
	/// <param name="players"></param>
	/// <param name="isServiceOnOriginalSide"></param>
	private void UpdateBotValues(List<ControllersParent> players, bool isServiceOnOriginalSide)
	{
		BotBehavior botBehaviorComponent;

		if (players[0].gameObject.TryGetComponent<BotBehavior>(out botBehaviorComponent))
		{
			botBehaviorComponent.TargetPosVector3 = players[0].gameObject.transform.position;

            if (isServiceOnOriginalSide)
			{
				botBehaviorComponent.SetTargetsSide(FieldSide.SECONDSIDE.ToString());
			}
			else
			{
				botBehaviorComponent.SetTargetsSide(FieldSide.FIRSTSIDE.ToString());
			}
		}
		else if (players[1].gameObject.TryGetComponent<BotBehavior>(out botBehaviorComponent)) 
        {
            botBehaviorComponent.TargetPosVector3 = players[1].gameObject.transform.position;

            if (isServiceOnOriginalSide)
            {
                botBehaviorComponent.SetTargetsSide(FieldSide.FIRSTSIDE.ToString());
			}
			else
			{
                botBehaviorComponent.SetTargetsSide(FieldSide.SECONDSIDE.ToString());
            }
        }
    }

	private void SetCollidersOwnerPlayers(List<ControllersParent> players, bool originalSides)
	{
		if (originalSides)
		{
			for (int i = 0; i < _firstSideCollidersParentObject.transform.childCount; i++) 
			{
				_firstSideCollidersParentObject.transform.GetChild(i).gameObject.GetComponent<FieldGroundPart>().OwnerPlayer = players[0];
				_secondSideCollidersParentObject.transform.GetChild(i).gameObject.GetComponent<FieldGroundPart>().OwnerPlayer = players[1];
            }
		}
		else
		{
            for (int i = 0; i < _firstSideCollidersParentObject.transform.childCount; i++)
            {
                _firstSideCollidersParentObject.transform.GetChild(i).gameObject.GetComponent<FieldGroundPart>().OwnerPlayer = players[1];
                _secondSideCollidersParentObject.transform.GetChild(i).gameObject.GetComponent<FieldGroundPart>().OwnerPlayer = players[0];
            }
        }
	}
}
