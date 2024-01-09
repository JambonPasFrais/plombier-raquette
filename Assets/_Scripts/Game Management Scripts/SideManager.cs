using JetBrains.Annotations;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SideManager : MonoBehaviour
{
    #region PRIVATE FIELDS

	[SerializeField] private  Transform _serviceNodesFirstSideContainer;
	[SerializeField] private Transform _serviceNodesSecondSideContainer;
	[SerializeField] private GameObject _firstSideCollidersParentObject;
	[SerializeField] private GameObject _secondSideCollidersParentObject;
	[SerializeField] private Transform _cameraParent;
	[SerializeField] private List<GameObject> _cameras;

	private Dictionary<string, List<Transform>> _servicePointsFirstSide = new Dictionary<string, List<Transform>>();
	private Dictionary<string, List<Transform>> _servicePointsSecondSide = new Dictionary<string, List<Transform>>();
	private Transform _activeCameraTransform;

    #endregion

    #region GETTERS

	public Transform ActiveCameraTransform { get { return GameManager.Instance.CameraManager.GetActiveCameraTransformBySide(GameManager.Instance.ServiceOnOriginalSide); } }

    #endregion

	private void Awake()
	{
		for (int i = 0; i < _serviceNodesFirstSideContainer.childCount; i++)
		{
			List<Transform> firstSideServiceNodes = new List<Transform>();
			List<Transform> secondSideServiceNodes = new List<Transform>();

			for (int j = 0; j < _serviceNodesFirstSideContainer.GetChild(i).childCount; j++)
			{
				firstSideServiceNodes.Add(_serviceNodesFirstSideContainer.GetChild(i).GetChild(j));
				secondSideServiceNodes.Add(_serviceNodesSecondSideContainer.GetChild(i).GetChild(j));
			}
			
			_servicePointsFirstSide.Add(_serviceNodesFirstSideContainer.GetChild(i).name, firstSideServiceNodes);
			_servicePointsSecondSide.Add(_serviceNodesSecondSideContainer.GetChild(i).name, secondSideServiceNodes);
		}

		/*for (int i = 0; i < _cameraParent.childCount; i++)
		{
			_cameras.Add(_cameraParent.GetChild(i).gameObject);
		}*/
	}


	/// <summary>
	/// This function is called externally and will determine either to set sides for simple or double match based on the
	/// nb of players
	/// </summary>
	public void SetSides(List<ControllersParent> players, bool serveRight, bool originalSides)
	{
		if (players.Count > 2)
			SetSidesInDoubleMatch(players, serveRight, originalSides);
		else
			SetSidesInSimpleMatch(players, serveRight, originalSides);
	}
	
	/// <summary>
	/// Alternates the player's fields and set the players, the cameras and the bot targets to the correct positions for a 1v1 match.
	/// </summary>
	/// <param name="players"></param>
	/// <param name="serveRight"></param>
	/// <param name="originalSides"></param>
	public void SetSidesInSimpleMatch(List<ControllersParent> players, bool serveRight, bool originalSides)
	{
        string side = "";

        side = serveRight ? "Right" : "Left";

		if (originalSides)
		{
			players[0].transform.position = _servicePointsFirstSide[side][0].position;
			players[0].transform.rotation = _servicePointsFirstSide[side][0].rotation;
			players[0].IsInOriginalSide = true;
			
			players[1].transform.position = _servicePointsSecondSide[side][0].position;
			players[1].transform.rotation = _servicePointsSecondSide[side][0].rotation;
			players[1].IsInOriginalSide = false;
		}
		else
		{
            players[0].transform.position = _servicePointsSecondSide[side][0].position;
			players[0].transform.rotation = _servicePointsSecondSide[side][0].rotation;
			players[0].IsInOriginalSide = false;

			players[1].transform.position = _servicePointsFirstSide[side][0].position;
			players[1].transform.rotation = _servicePointsFirstSide[side][0].rotation;
			players[1].IsInOriginalSide = true;
		}

		GameManager.Instance.CameraManager.ChangeCameraSide(originalSides);

		UpdateBotValues(players, originalSides);
		SetCollidersOwnerPlayers(players, originalSides);
	}

    /// <summary>
    /// Alternates the player's fields and set the players, the cameras and the bot targets to the correct positions in a 2v2 match.
    /// </summary>
    /// <param name="players"></param>
    /// <param name="serveRight"></param>
    /// <param name="_originalSides"></param>
    private void SetSidesInDoubleMatch(List<ControllersParent> players, bool serveRight, bool originalSides)
	{
		string side = "";

        side = serveRight ? "Right" : "Left";

		if (originalSides)
		{
			players[0].transform.position = _servicePointsFirstSide[side][0].position;
			players[0].transform.rotation = _servicePointsFirstSide[side][0].rotation;
			players[0].IsInOriginalSide = true;
			
			players[1].transform.position = _servicePointsSecondSide[side][0].position;
			players[1].transform.rotation = _servicePointsSecondSide[side][0].rotation;
			players[1].IsInOriginalSide = false;
			
			players[2].transform.position = _servicePointsFirstSide[side][1].position;
			players[2].transform.rotation = _servicePointsFirstSide[side][1].rotation;
			players[2].IsInOriginalSide = true;
			
			players[3].transform.position = _servicePointsSecondSide[side][1].position;
			players[3].transform.rotation = _servicePointsSecondSide[side][1].rotation;
			players[3].IsInOriginalSide = false;
		}
		else
		{
			players[0].transform.position = _servicePointsSecondSide[side][0].position;
			players[0].transform.rotation = _servicePointsSecondSide[side][0].rotation;
			players[0].IsInOriginalSide = false;

			players[1].transform.position = _servicePointsFirstSide[side][0].position;
			players[1].transform.rotation = _servicePointsFirstSide[side][0].rotation;
			players[1].IsInOriginalSide = true;
			
			players[2].transform.position = _servicePointsSecondSide[side][1].position;
			players[2].transform.rotation = _servicePointsSecondSide[side][1].rotation;
			players[2].IsInOriginalSide = false;

			players[3].transform.position = _servicePointsFirstSide[side][1].position;
			players[3].transform.rotation = _servicePointsFirstSide[side][1].rotation;
			players[3].IsInOriginalSide = true;
		}
		
		GameManager.Instance.CameraManager.ChangeCameraSide(originalSides);
		
		Debug.Log("Sides in Double Match set");
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

    public void SetSideOnline(bool serveRight, bool originalSides)
    {
        GameManager.Instance.photonView.RPC("SetSidesInOnlineMatch", RpcTarget.All, serveRight, originalSides);
    }

    [PunRPC]
    public void SetSidesInOnlineMatch(bool serveRight, bool originalSides)
    {
        string side = "";
        List<ControllersParent> players = GameManager.Instance.Controllers;
        side = serveRight ? "Right" : "Left";

        if (originalSides)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                _activeCameraTransform = _cameras[0].transform;
                _cameras[0].SetActive(true);
                _cameras[1].SetActive(false);
                players[0].transform.position = _servicePointsFirstSide[side][0].position;
                players[0].transform.rotation = _servicePointsFirstSide[side][0].rotation;
            }
            else if(PhotonNetwork.IsMasterClient == false)
            {
                _activeCameraTransform = _cameras[1].transform;
                _cameras[0].SetActive(false);
                _cameras[1].SetActive(true);
                players[1].transform.position = _servicePointsSecondSide[side][0].position;
                players[1].transform.rotation = _servicePointsSecondSide[side][0].rotation;
            }
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                _activeCameraTransform = _cameras[1].transform;
                _cameras[0].SetActive(false);
                _cameras[1].SetActive(true);
                players[0].transform.position = _servicePointsSecondSide[side][0].position;
                players[0].transform.rotation = _servicePointsSecondSide[side][0].rotation;
            }
            else if(PhotonNetwork.IsMasterClient == false)
            {
                _activeCameraTransform = _cameras[0].transform;
                _cameras[0].SetActive(true);
                _cameras[1].SetActive(false);
                players[1].transform.position = _servicePointsFirstSide[side][0].position;
                players[1].transform.rotation = _servicePointsFirstSide[side][0].rotation;
            }
        }

        SetCollidersOwnerPlayers(players, originalSides);
    }
}
