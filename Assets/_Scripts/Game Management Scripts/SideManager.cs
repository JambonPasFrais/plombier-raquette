using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideManager : MonoBehaviour
{
    #region PRIVATE FIELDS

    private static SideManager _instance;

	[SerializeField] private Transform _servicePointsFirstSideParent;
	[SerializeField] private Transform _servicePointsSecondSideParent;
	[SerializeField] private Transform _cameraParent;
	[SerializeField] private List<GameObject> _cameras;

	private Dictionary<string, Transform> _servicePointsFirstSide = new Dictionary<string, Transform>();
	private Dictionary<string, Transform> _servicePointsSecondSide = new Dictionary<string, Transform>();
	private Transform _activeCameraTransform;

    #endregion

    #region GETTERS

    public static SideManager Instance => _instance;
	public Transform ActiveCameraTransform { get { return _activeCameraTransform; } }

	#endregion

	private void Awake()
	{
		if (_instance == null)
			_instance = this;

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

	public void ChangeSidesInGameSimple(List<ControllersParent> players, bool serveRight, bool _originalSides)
	{
		string side = "";

		side = serveRight ? "Right" : "Left";

		if (_originalSides)
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
	}

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
}
