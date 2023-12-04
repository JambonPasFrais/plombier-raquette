using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;

public class ControllerSelectionArmandVer : MonoBehaviour
{
    [Header("Instances")] 
    [SerializeField] private GameObject _keyboardPlayerPrefab;
    [SerializeField] private GameObject _controllerPlayerPrefab;

    private Dictionary<int,PlayerInstance> _playerInstances = new Dictionary<int, PlayerInstance>();
    
    private void OnEnable()
    {
        InputSystem.onDeviceChange += InputDeviceChanged;
    }
    
    private void OnDisable()
    {
        InputSystem.onDeviceChange -= InputDeviceChanged;
    }

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
        PlayerInstance newPlayerInstance = new PlayerInstance();
        newPlayerInstance.Device = device;
        
        _playerInstances.Add(device.deviceId, newPlayerInstance);
    }

    private void DeviceRemoved(InputDevice device)
    {
        if (_playerInstances.ContainsKey(device.deviceId))
        {
            _playerInstances.Remove(device.deviceId);
        }
    }
}
