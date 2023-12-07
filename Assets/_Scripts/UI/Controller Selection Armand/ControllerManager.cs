using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerManager : MonoBehaviour
{
    // Reference to the InputAction for joining a player
    public InputAction joinPlayerAction;
    [SerializeField] private GameObject _controllerPrefab;
    [SerializeField] private Transform _controllerContainer;

    private Dictionary<int, GameObject> _controllers;

    private void OnEnable()
    {
        // Enable the input action
        joinPlayerAction.Enable();
    }

    private void OnDisable()
    {
        // Disable the input action
        joinPlayerAction.Disable();
    }

    private void Awake()
    {
        // Register a callback for the input action
        _controllers = new Dictionary<int, GameObject>();
    }

    public void ControllerCanBeAdded()
    {
        joinPlayerAction.performed += PlayerTriesToJoin;
    }
    
    public void ControllerCantBeAdded()
    {
        joinPlayerAction.performed -= PlayerTriesToJoin;
    }


    private void PlayerTriesToJoin(InputAction.CallbackContext context)
    {
        // if already exists, dont recreate it
        if (_controllers.ContainsKey(context.control.device.deviceId))
            return;

        //Instantiate and add to a dict
        _controllers.Add(context.control.device.deviceId,
            PlayerInput.Instantiate(_controllerPrefab, -1, null, -1, context.control.device).gameObject);
        
        // Move to specified container (TRANSFORM PARENT) + reset scale (we move it from world position to canvas
        Transform createdControllerTransform = _controllers[context.control.device.deviceId].transform;
        createdControllerTransform.SetParent(_controllerContainer);
        //createdControllerTransform.localScale = Vector3.one;
        //createdControllerTransform.position = Vector3.zero;

    }
}
