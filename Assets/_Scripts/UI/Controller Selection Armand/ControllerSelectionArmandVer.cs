using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.HID;
using UnityEngine.InputSystem.XInput;

public class ControllerSelectionArmandVer : MonoBehaviour
{
    [Header("Instances")] 
    [SerializeField] private PlayerInstance _keyboardPlayerPrefab;
    [SerializeField] private PlayerInstance _gamepadPlayerPrefab;
    [SerializeField] private PlayerInstance _joystickPlayerPrefab;
    [SerializeField] private Transform _controllersContainer;

    private Dictionary<int,PlayerInstance> _playerInstances = new Dictionary<int, PlayerInstance>();
    private int _nbControllerConnected;
    [SerializeField] private bool _keyboardControllerNeeded;
    private InputDevice _keyboardDevice;

    #region Tests

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= InputDeviceChanged;
    }

    #endregion

    #region Listeners
    public void OnMenuLoaded()
    {
        InputSystem.onDeviceChange += InputDeviceChanged;
        CountConnectedControllers();
    }
    
    public void OnMenuUnloaded()
    {
        InputSystem.onDeviceChange -= InputDeviceChanged;
    }

    public void OnValidateControllerSelection()
    {
        Debug.Log("Validated Controllers");
    }

    public void OnCancelControllerSelection()
    {
        _playerInstances.Clear();
    }

    public void OnKeyboardControllerActivated()
    {
        _keyboardControllerNeeded = !_keyboardControllerNeeded;
        
        if (_keyboardControllerNeeded)
            DeviceAdded(_keyboardDevice);
        else
            DeviceRemoved(_keyboardDevice);
    }
    #endregion
    

    private void InputDeviceChanged(InputDevice device, InputDeviceChange change)
    {
        switch (change)
        {
            case InputDeviceChange.Added:
                DeviceAdded(device);
                break;

            case InputDeviceChange.Removed:
                DeviceRemoved(device);
                break;
        }
    }

    private void DeviceAdded(InputDevice device)
    {
        //Instantiate Controller + add it to an array
        PlayerInstance newPlayerInstance;
        if (device is Keyboard)
        {
            if (!_keyboardControllerNeeded)
            {
                _keyboardDevice = device;
                return;
            }
            
            newPlayerInstance = Instantiate(_keyboardPlayerPrefab, _controllersContainer);
            
        }else if (device is Joystick)
        {
            newPlayerInstance = Instantiate(_joystickPlayerPrefab, _controllersContainer);
        }else
        {
            newPlayerInstance = Instantiate(_gamepadPlayerPrefab, _controllersContainer);
        }
        
        newPlayerInstance.Device = device;
        
        _playerInstances.Add(device.deviceId, newPlayerInstance);
    }

    private void DeviceRemoved(InputDevice device)
    {
        if (_playerInstances.ContainsKey(device.deviceId))
        {
            Destroy(_playerInstances[device.deviceId].gameObject);
            _playerInstances.Remove(device.deviceId);
        }
    }

    private void CountConnectedControllers()
    {
        _nbControllerConnected = 0;
        
        foreach (var device in InputSystem.devices)
        {
            if (device is Keyboard || device is Joystick || device is Gamepad)
            {
                Debug.Log(device.deviceId);
                DeviceAdded(device);
            }
        }
    }
}
