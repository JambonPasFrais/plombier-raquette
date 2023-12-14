using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputHandler : MonoBehaviour
{
    // TODO : change this variable for private with getter and setter
    [HideInInspector] public Controller Controller;
    [HideInInspector] public Character Character;
    private PlayerInput _playerInput;
    
    #region GETTERS

    public PlayerInput PlayerInput => _playerInput;
    
    #endregion

    #region UNITY FUNCTIONS 
    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }
    #endregion

    #region UI ACTION MAP LISTENERS
    public void OnCursorMove(InputAction.CallbackContext context)
    {
        Controller.TryMove(context.ReadValue<Vector2>());
    }

    public void OnCursorPunch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Controller.TryPunch();
        }
    }
    
    public void OnSelect(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Controller.TrySelect();
        }
    }

    public void OnDeselect(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Controller.TryDeselect();   
        }
    }
    
    #endregion
    
    #region GAME ACTION MAP LISTENERS

    public void OnCharacterMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Character.PlayerController.Move(context);
        }
    }

    public void OnChargeShot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Character.PlayerController.ChargeShot(context);
        }
    }

    public void OnFlatShot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Character.PlayerController.Flat(context);
        }
    }

    public void OnTopSpinShot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Character.PlayerController.TopSpin(context);
        }
    }

    public void OnDropShot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Character.PlayerController.Drop(context);
        }
    }

    public void OnSliceShot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Character.PlayerController.Slice(context);
        }
    }

    public void OnLobShot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Character.PlayerController.Lob(context);
        }
    }

    public void OnSlowTime(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Character.PlayerController.Slice(context);
        }
    }

    public void OnTechnicalShot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Character.PlayerController.TechnicalShot(context);
        }
    }

    public void OnServeThrow(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Character.PlayerController.ServeThrow(context);
        }
    }

    public void OnSmash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Character.PlayerController.Smash();
        }
    }
    
    #endregion
    
    #region BASIC LISTENERS
    
    public void OnDeviceLost(PlayerInput playerInput)
    {
        ControllerManager.Instance.DeletePlayerFromControllerSelection(playerInput);
    }
    
    #endregion
    
    #region CORE FUNCTIONS

    public void ChangeInputActionMap(InputActionMap newInputActionMap)
    {
        _playerInput.currentActionMap = newInputActionMap;
    }
    
    #endregion
}
