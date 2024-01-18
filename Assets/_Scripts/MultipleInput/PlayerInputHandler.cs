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
	[SerializeField] private PlayerInput _playerInput; // TODO : comment when finishing localmultiplayer implementation

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

	public void OnContinue(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			/*MenuManager.Instance.PlaySound("LoadingScreenTransition");
			gameObject.SetActive(false);
			_menusContainer.SetActive(true);*/
			Debug.Log("J'appuie");
		}
	}

	#endregion

	#region GAME ACTION MAP LISTENERS

	public void OnAimShot(InputAction.CallbackContext context)
    {
        Character.PlayerController.AimShot(context);
    }

	public void OnCharacterMove(InputAction.CallbackContext context)
	{
		Character.PlayerController.Move(context);
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

	public void OnSliceShot(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			Character.PlayerController.Slice(context);
		}
	}

	public void OnDropShot(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			Character.PlayerController.Drop(context);
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
			Character.PlayerController.SlowTime(context);
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
			Character.PlayerController.ServiceThrow(context);
		}
	}

	public void OnPrepSmash(InputAction.CallbackContext context)
	{
		if (context.performed)
			Character.PlayerController.PrepareSmash(context);
	}
    
    public void OnAimSmash(InputAction.CallbackContext context)
    {
        Character.PlayerController.PlayerCameraController.AimSmashTarget(context);
    }

    public void OnSmash(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			Character.PlayerController.Smash(context);
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

	public void ChangeInputActionMap(string inputActionMapName)
	{
		_playerInput.SwitchCurrentActionMap(inputActionMapName);
	}

	public void InitDirectionController()
	{
		Character.PlayerController.SetDirectionController(_playerInput.devices[0] is Keyboard ? 
			ShootDirectionController.MOUSE : ShootDirectionController.STICK);
	}

	#endregion
}
