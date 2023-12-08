using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class ControllerManager : MonoBehaviour
{
    public static ControllerManager Instance => _instance;
    
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
    private static ControllerManager _instance;
    private Coroutine _currentDeleteCtrlCoroutine;
    
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
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Listeners
    public void OnControllerSelectionLoad()
    {
        Init();
        ControllerCanBeAdded();
    }

    public void OnBackToControllerSelection()
    {
        ControllerCanBeAdded();
        SwitchCtrlersToCtrlSelectMode();
    }
    
    public void OnExitControllerSelection()
    {
        ControllerCantBeAdded();
        OnResetControllers();
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
        Debug.Log(_controllers);
        ControllerCantBeAdded();
        Debug.Log(_controllers);
        SwitchCtrlersToCharSelectMode();
    }
    #endregion

    #region CALLED EXTERNALLY

    public void DeletePlayerFromControllerSelection(PlayerInput playerInput)
    {
        //Use coroutine in order to delay the delete and await the end of the deviceLost event
        _currentDeleteCtrlCoroutine = StartCoroutine(DeleteControllerCoroutine(playerInput.devices[0].deviceId));
    }
    
    #endregion
    
    #region Unclassable functions

    private void Init()
    {
        _maxPlayerCount = GameParameters.NumberOfPlayers;
        _controllers = new Dictionary<int, GameObject>();
    }
    
    private void SwitchCtrlersToCharSelectMode()
    {
        // Enters with no "input at all"
        foreach (var controller in _controllers)
        {
            controller.Value.GetComponent<Controller>().CharacterSelectionMode();
            controller.Value.transform.SetParent(_characterSelectionContainer); // when exits that line, has a new input base on the "input order"
            controller.Value.transform.position = Vector3.zero;
            controller.Value.transform.localScale = Vector3.one;
        }
    }

    private void SwitchCtrlersToCtrlSelectMode()
    {
        foreach (var controller in _controllers)
        {
            controller.Value.GetComponent<Controller>().ControllerSelectionMode();
            controller.Value.transform.SetParent(_controllerSelectionContainer);
            //controller.Value.transform.position = Vector3.zero;
            //controller.Value.transform.localScale = Vector3.one;
        }
    }
    
    private void ControllerCanBeAdded()
    {
        _joinPlayerAction.performed += PlayerTriesToJoin;
    }
    
    private void ControllerCantBeAdded()
    {
        _joinPlayerAction.performed -= PlayerTriesToJoin;
        Debug.Log(_controllers);
    }

    private IEnumerator DeleteControllerCoroutine(int deviceId)
    {
        yield return new WaitForSeconds(.1f);
        Destroy(_controllers[deviceId]);
        _controllers.Remove(deviceId);
    }
    
    #endregion
    
    #region Subscribe function
    private void PlayerTriesToJoin(InputAction.CallbackContext context)
    {
        // every player joined 
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
