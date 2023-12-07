using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class ControllerManager : MonoBehaviour
{
    // Reference to the InputAction for joining a player
    
    [Header("Parameters")]
    [SerializeField] private InputAction _joinPlayerAction;
    [SerializeField] private int _maxPlayerCount;
    
    [Header("Instances")]
    [SerializeField] private GameObject _gamepadPrefab;
    [SerializeField] private GameObject _keyboardPrefab;
    [SerializeField] private GameObject _joystickPrefab;
    
    [SerializeField] private Transform _controllerSelectionContainer;
    [SerializeField] private Transform _characterSelectionContainer;

    private int _playerCount;
    private Dictionary<int, GameObject> _controllers;

    #region Unity Functions
    private void OnEnable()
    {
        // Enable the input action
        _joinPlayerAction.Enable();
    }

    private void OnDisable()
    {
        // Disable the input action
        _joinPlayerAction.Disable();
    }

    private void Awake()
    {
        _controllers = new Dictionary<int, GameObject>();
    }
    #endregion

    #region Listeners
    public void ControllerCanBeAdded()
    {
        _joinPlayerAction.performed += PlayerTriesToJoin;
    }
    
    public void ControllerCantBeAdded()
    {
        _joinPlayerAction.performed -= PlayerTriesToJoin;
    }

    public void OnResetControllers()
    {
        foreach (var pair in _controllers)
        {
            Destroy(pair.Value);
        }
        
        _controllers.Clear();
    }

    public void OnValidateControllerSelection()
    {
        ControllerCantBeAdded();
        foreach (var controller in _controllers)
        {
            controller.Value.GetComponent<Controller>().CharacterSelectionMode();
            controller.Value.transform.SetParent(_characterSelectionContainer);
            controller.Value.transform.position = Vector3.zero;
            controller.Value.transform.localScale = Vector3.one;
        }
    }
    #endregion

    #region Subscribe function
    private void PlayerTriesToJoin(InputAction.CallbackContext context)
    {
        // every player joined full
        if (_controllers.Count >= _maxPlayerCount)
            return;
        
        // if already exists, dont recreate it
        if (_controllers.ContainsKey(context.control.device.deviceId))
            return;

        // Player Input Creation
        InputDevice inputDevice = context.control.device;

        switch (inputDevice)
        {
            case Joystick:
                _controllers.Add(inputDevice.deviceId,
                    PlayerInput.Instantiate(_joystickPrefab, -1, null, -1, inputDevice).gameObject);
                break;
            case Gamepad:
                _controllers.Add(inputDevice.deviceId,
                    PlayerInput.Instantiate(_gamepadPrefab, -1, null, -1, inputDevice).gameObject);
                break;
            case Keyboard:
                _controllers.Add(inputDevice.deviceId,
                    PlayerInput.Instantiate(_keyboardPrefab, -1, null, -1, inputDevice).gameObject);
                break;
        }

        Controller controllerInstance = _controllers[inputDevice.deviceId].GetComponent<Controller>();
        controllerInstance.ControllerSelectionMode();
        
        //UI Stuff
        // Move to specified container (TRANSFORM PARENT) + reset scale if canvas is in world space
        Transform createdControllerTransform = _controllers[inputDevice.deviceId].transform;
        createdControllerTransform.SetParent(_controllerSelectionContainer);
        //createdControllerTransform.localScale = Vector3.one;
        //createdControllerTransform.position = Vector3.zero;
    }
    #endregion
}
