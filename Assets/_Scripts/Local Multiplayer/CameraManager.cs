using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Instances")] [SerializeField] private Transform _soloCamerasContainer;
    [SerializeField] private Transform _splitScreenCamerasContainer;
    
    private List<GameObject> _soloCamerasGo;
    private List<GameObject> _splitScreenCamerasGo;
    private Transform _activeCameraTransform;

    #region GETTERS

    public Transform ActiveCameraTransform => _activeCameraTransform;

    #endregion

    private void Awake()
    {
        _soloCamerasGo = new List<GameObject>();
        _splitScreenCamerasGo = new List<GameObject>();
        
        for (int i = 0; i < _soloCamerasContainer.childCount; i++)
        {
            _soloCamerasGo.Add(_soloCamerasContainer.GetChild(i).gameObject);
        }
        
        for (int i = 0; i < _splitScreenCamerasContainer.childCount; i++)
        {
            _splitScreenCamerasGo.Add(_splitScreenCamerasContainer.GetChild(i).gameObject);
        }
    }

    public void InitSplitScreenCameras()
    {
        foreach (var soloCamera in _soloCamerasGo)
        {
            soloCamera.SetActive(false);
        }

        foreach (var splitScreenCamera in _splitScreenCamerasGo)
        {
            splitScreenCamera.SetActive(true);
        }
    }

    public void InitSoloCamera()
    {
        foreach (var splitScreenCamera in _splitScreenCamerasGo)
        {
            splitScreenCamera.SetActive(false);
        }
        
        foreach (var soloCamera in _soloCamerasGo)
        {
            soloCamera.SetActive(true);
        }
    }

    public void ChangeCameraSide(bool isOriginalSide)
    {
        _activeCameraTransform = _soloCamerasGo[isOriginalSide ? 0 : 1].transform;
        _soloCamerasGo[isOriginalSide ? 0 : 1].SetActive(true);
        _soloCamerasGo[isOriginalSide ? 1 : 0].SetActive(false);
    }
}
