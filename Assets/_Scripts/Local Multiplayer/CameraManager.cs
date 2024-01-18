using Photon.Pun;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Instances")] 
    [SerializeField] private Transform _soloCamerasContainer;
    [SerializeField] private Transform _splitScreenCamerasContainer;
    [SerializeField] private GameObject _endGameUICamera;
    //[SerializeField] private List<Transform> _activeCameraTransformsBySide;

    private bool _splitScreenCameraIsOn;
    private List<Camera> _soloCameras;
    private Dictionary<string, List<Camera>> _splitScreenCamerasBySide;
    private List<Camera> _camerasToActivateAfterSmash;

    //private Transform _activeCameraTransform;

    #region GETTERS

    //public Transform ActiveCameraTransform => _activeCameraTransform;
    //public List<Transform> ActiveCameraTransformsBySide => _activeCameraTransformsBySide;

    #endregion

    private void Awake()
    {
        _soloCameras = new List<Camera>();
        //_activeCameraTransformsBySide = new List<Transform>();
        _splitScreenCamerasBySide = new Dictionary<string, List<Camera>>();

        for (int i = 0; i < _soloCamerasContainer.childCount; i++)
        {
            _soloCameras.Add(_soloCamerasContainer.GetChild(i).gameObject.GetComponent<Camera>());
        }

        if (!PhotonNetwork.IsConnected)
        {
            for (int i = 0; i < _splitScreenCamerasContainer.childCount; i++)
            {
                List<Camera> cameraListForCurrentSide = new List<Camera>();

                for (int j = 0; j < _splitScreenCamerasContainer.GetChild(i).childCount; j++)
                {
                    cameraListForCurrentSide.Add(_splitScreenCamerasContainer.GetChild(i).GetChild(j).GetComponent<Camera>());
                }

                _splitScreenCamerasBySide.Add(i == 0 ? "Left" : "Right", cameraListForCurrentSide);
            }
        }

        _endGameUICamera.SetActive(false);
    }

    private void Start()
    {
        GameManager.Instance.SideManager.Cameras = new List<GameObject>();

        for (int i = 0; i < _soloCamerasContainer.childCount; i++)
        {
            GameManager.Instance.SideManager.Cameras.Add(_soloCamerasContainer.GetChild(i).gameObject);
        }
    }

    public void InitSplitScreenCameras()
    {
        foreach (var soloCamera in _soloCameras)
        {
            soloCamera.gameObject.SetActive(false);
        }

        foreach (var splitScreenCamera in _splitScreenCamerasBySide.Values)
        {
            splitScreenCamera[0].gameObject.SetActive(true);
            splitScreenCamera[1].gameObject.SetActive(true);
        }

        _splitScreenCameraIsOn = true;
    }

    public void InitSoloCamera()
    {
        foreach (var splitScreenCamera in _splitScreenCamerasBySide.Values)
        {
            splitScreenCamera[0].gameObject.SetActive(false);
            splitScreenCamera[1].gameObject.SetActive(false);
        }
        
        foreach (var soloCamera in _soloCameras)
        {
            soloCamera.gameObject.SetActive(true);
        }
        
        _splitScreenCameraIsOn = false;
    }

    public void ChangeCameraSide(bool isOriginalSide)
    {
        if (_splitScreenCameraIsOn)
        {
            _splitScreenCamerasBySide[isOriginalSide ? "Left" : "Right"][0].gameObject.SetActive(true);
            _splitScreenCamerasBySide[isOriginalSide ? "Left" : "Right"][1].gameObject.SetActive(true);

            _splitScreenCamerasBySide[isOriginalSide ? "Right" : "Left"][0].gameObject.SetActive(false);
            _splitScreenCamerasBySide[isOriginalSide ? "Right" : "Left"][1].gameObject.SetActive(false);
        }
        else
        {
            if ((PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient) || !PhotonNetwork.IsConnected) 
            {
                _soloCameras[isOriginalSide ? 0 : 1].gameObject.SetActive(true);
                _soloCameras[isOriginalSide ? 1 : 0].gameObject.SetActive(false);
            }
            else
            {
                _soloCameras[isOriginalSide ? 1 : 0].gameObject.SetActive(true);
                _soloCameras[isOriginalSide ? 0 : 1].gameObject.SetActive(false);
            }
        }
    }

    public Transform GetActiveCameraTransformBySide(bool isOriginalSide)
    {
        if (_splitScreenCameraIsOn)
        {
            var cameraBySide = _splitScreenCamerasBySide[isOriginalSide ? "Left" : "Right"];
            
            for (int i = 0; i < cameraBySide.Count; i++)
            {
                if (cameraBySide[i].gameObject.activeSelf)
                    return cameraBySide[i].transform;
            }
        }

        return _soloCameras[isOriginalSide ? 0 : 1].transform;
    }

    public void ToggleGameCamerasForSmash()
    {
        if (_camerasToActivateAfterSmash != null)
        {
            foreach (var cameraToActivate in _camerasToActivateAfterSmash)
            {
                cameraToActivate.gameObject.SetActive(true);
            }

            _camerasToActivateAfterSmash = null;
            return;
        }

        _camerasToActivateAfterSmash = new List<Camera>();
        
        if (_splitScreenCameraIsOn)
        {
            foreach (var sideCameras in _splitScreenCamerasBySide.Values)
            {
                foreach (var sideCamera in sideCameras)
                {
                    if (sideCamera.gameObject.activeSelf)
                    {
                        _camerasToActivateAfterSmash.Add(sideCamera);
                        sideCamera.gameObject.SetActive(false);
                    }
                }
            }
        }
        else
        {
            foreach (var soloCamera in _soloCameras)
            {
                if (soloCamera.gameObject.activeSelf)
                {
                    _camerasToActivateAfterSmash.Add(soloCamera);
                    soloCamera.gameObject.SetActive(false);
                }
            }
        }
    }

    public void EndGameCameraMode()
    {
        foreach(var soloCamera in _soloCameras)
            soloCamera.gameObject.SetActive(false);

		foreach (var splitScreenCamera in _splitScreenCamerasBySide.Values)
		{
			splitScreenCamera[0].gameObject.SetActive(false);
			splitScreenCamera[1].gameObject.SetActive(false);
		}

        _endGameUICamera.SetActive(true);
	}
}
