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
    [SerializeField] private Controller _gamepadPrefab;
    [SerializeField] private Controller _keyboardPrefab;
    [SerializeField] private Controller _joystickPrefab;
    [SerializeField] private GameObject _playerInputHandlerPrefab;
    
    [SerializeField] private Transform _controllerSelectionContainer;
    [SerializeField] private Transform _characterSelectionContainer;

    [SerializeField] private CharacterSelectionMenu _characterSelectionMenu;
    
    private int _playerCount;
    private Dictionary<int, GameObject> _controllers = new Dictionary<int, GameObject>();
    private static ControllerManager _instance;
    private Coroutine _currentDeleteCtrlCoroutine;
    
    #region Getters

    public CharacterSelectionMenu CharacterSelectionMenu => _characterSelectionMenu;
    public Dictionary<int, GameObject> Controllers => _controllers;
    
    #endregion
    
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
        ControllerCantBeAdded();
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
            controller.Value.GetComponent<PlayerInputHandler>().Controller.CharacterSelectionMode();
            controller.Value.transform.SetParent(_characterSelectionContainer); // when exits that line, has a new input base on the "input order"
            controller.Value.transform.position = Vector3.zero;
            controller.Value.transform.localScale = Vector3.one;
        }
    }

    private void SwitchCtrlersToCtrlSelectMode()
    {
        foreach (var controller in _controllers)
        {
            controller.Value.GetComponent<PlayerInputHandler>().Controller.ControllerSelectionMode();
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

        // Player Input Handler Creation
        InputDevice inputDevice = context.control.device;

        PlayerInput playerInput = PlayerInput.Instantiate(_playerInputHandlerPrefab,
            -1,
            null,
            -1,
            inputDevice);

        GameObject playerInputHandlerGo = playerInput.gameObject;
        playerInputHandlerGo.transform.SetParent(gameObject.transform);

        // Init Controller.cs file
        PlayerInputHandler playerInputHandler = playerInputHandlerGo.GetComponent<PlayerInputHandler>();
        
        switch (inputDevice)
        {
            case Joystick:
                playerInputHandler.Controller = Instantiate(_joystickPrefab, _controllerSelectionContainer);
                break;
            case Gamepad:
                playerInputHandler.Controller = Instantiate(_gamepadPrefab, _controllerSelectionContainer);
                break;
            case Keyboard:
                playerInputHandler.Controller =  Instantiate(_keyboardPrefab, _controllerSelectionContainer);
                break;
        }

        playerInputHandler.Controller.ControllerSelectionMode();
        playerInputHandler.Controller.PlayerInput = playerInput;
        
        //UI Stuff of the controller prefab
        playerInputHandler.Controller.gameObject.transform.localScale = Vector3.one;
        playerInputHandler.Controller.gameObject.transform.position = Vector3.zero;
        
        // Save of the playerInputHandler base on his device id
        _controllers.Add(inputDevice.deviceId, playerInputHandlerGo);
    }
    #endregion
}
