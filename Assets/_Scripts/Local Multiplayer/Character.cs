using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class Character : MonoBehaviour
{
    // Variables may be set tup automatically
    
    [Header("Instances")] private CharacterParameters _parameters;
    
    [Header("Components")]
    private PlayerController _playerController; // Component for controls
    
    #region GETTERS
    
    public PlayerController PlayerController => _playerController;
    
    #endregion

    #region UNITY METHODES

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    #endregion
    
    #region FUNCTIONS CALLED EXTERNALLY

    public void SetCharParameters(CharacterParameters characterParameters)
    {
        _parameters = characterParameters;
        
        // TODO Init Player Controllers with Parameters  (will do it when i can modify the playerControllers)
    }
    
    #endregion
    
    #region ACTIONS CALLED EXTERNALLY (temporary useless, waiting for permission to modify player controller)

    public void TryMove(Vector2 readValue)
    {
        Debug.Log("move");
    }

    public void TryChargeShot()
    {
        Debug.Log("charging shot");
    }

    public void TryFlatShot()
    {
        Debug.Log("charging shot");
    }

    public void TryTopSpinShot()
    {
        Debug.Log("charging shot");
    }

    public void TryDropShot()
    {
        Debug.Log("charging shot");
    }

    public void TrySliceShot()
    {
        Debug.Log("charging shot");
    }

    public void TryLobShot()
    {
        Debug.Log("charging shot");
    }

    public void TrySlowTime()
    {
        Debug.Log("charging shot");
    }

    public void TryTechnicalShot()
    {
        Debug.Log("charging shot");
    }

    public void TryServeThrow()
    {
        Debug.Log("charging shot");
    }

    public void TrySmash()
    {
        Debug.Log("charging shot");
    }

    #endregion
}
