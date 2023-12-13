using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputHandler : MonoBehaviour
{
    [HideInInspector] public Controller Controller;
    private PlayerInput _playerInput;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Controller.TryMove(context.ReadValue<Vector2>());
    }

    public void OnPunch(InputAction.CallbackContext context)
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
    
    public void OnDeviceLost(PlayerInput playerInput)
    {
        ControllerManager.Instance.DeletePlayerFromControllerSelection(playerInput);
    }
}
