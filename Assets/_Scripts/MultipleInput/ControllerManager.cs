using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class ControllerManager : MonoBehaviour
{
    public static ControllerManager Instance => _instance;
    
    [Header("Parameters")]
    [SerializeField] private InputAction _joinPlayerAction;

    [Header("Instances")]
    [SerializeField] private Controller _gamepadPrefab;
    [SerializeField] private Controller _keyboardPrefab;
    [SerializeField] private Controller _joystickPrefab;
    [SerializeField] private GameObject _playerInputHandlerPrefab;
    [SerializeField] private string _menuActionMapName;
    [SerializeField] private string _gameActionMapName;
    [SerializeField] private TextMeshProUGUI _numberOfControllersConnectedOnMenu;

    [SerializeField] private int _maxPlayerCount;
    private Dictionary<int, PlayerInputHandler> _controllers = new Dictionary<int, PlayerInputHandler>();
    private static ControllerManager _instance;
    private CharacterSelectionMenu _characterSelectionMenu;
    private ControllerSelectionMenu _controllerSelectionMenu;
    private Coroutine _currentDeleteCtrlCoroutine;

    #region Getters

    public CharacterSelectionMenu CharacterSelectionMenu => _characterSelectionMenu;
    public static Dictionary<int, PlayerInputHandler> Controllers => _instance._controllers;
    
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
        
        //CreateControllersDict();//TODO : comment when finish testing
    }
    #endregion

    #region CALLED EXTERNALLY

    public void DeletePlayerFromControllerSelection(PlayerInput playerInput)
    {
        //Use coroutine in order to delay the delete and await the end of the deviceLost event
        _currentDeleteCtrlCoroutine = StartCoroutine(DeleteControllerCoroutine(playerInput.devices[0].deviceId));
    }
    
    public void Init(CharacterSelectionMenu characterSelectionMenuRef, ControllerSelectionMenu controllerSelectionMenu)
    {
        _maxPlayerCount = GameParameters.Instance.LocalNbPlayers;
        _characterSelectionMenu = characterSelectionMenuRef;
        _controllerSelectionMenu = controllerSelectionMenu;
        _controllers = new Dictionary<int, PlayerInputHandler>();
        _numberOfControllersConnectedOnMenu.text = $"0 out of {_maxPlayerCount} controllers connected";
	}
    
    public void SwitchCtrlersToCharSelectMode(Transform charSelectionContainer)
    {
        foreach (var controller in _controllers)
        {
            controller.Value.Controller.gameObject.transform.SetParent(charSelectionContainer);
            controller.Value.Controller.CharacterSelectionMode();
        }
    }

    public void SwitchCtrlersToCtrlSelectMode(Transform cltrSelectionContainer)
    {
        foreach (var controller in _controllers)
        {
            controller.Value.Controller.gameObject.transform.SetParent(cltrSelectionContainer);
			controller.Value.Controller.ControllerSelectionMode();
            controller.Value.Controller.ReturnOnControllerSelectionMenu();
		}
    }
    
    public void ControllerCanBeAdded()
    {
        _joinPlayerAction.performed += PlayerTriesToJoin;
    }
    
    public void ControllerCantBeAdded()
    {
        _joinPlayerAction.performed -= PlayerTriesToJoin;
    }

    public void DestroyControllers()
    {
        if (_controllers.Count > 0)
        {
            foreach (var pair in _controllers)
            {
                Destroy(pair.Value.Controller.gameObject);
                Destroy(pair.Value.gameObject);
            }
        }
        
        _controllers.Clear();
        _numberOfControllersConnectedOnMenu.text = $"0 out of {_maxPlayerCount} controllers connected";
    }

    public void ResetControllers()
    {
        foreach (var controller in _controllers)
        {
            controller.Value.Controller.ResetView();
        }
    }

    public void ChangeCtrlersActMapToGame()
    {
        foreach (var playerInputHandler in _controllers.Values)
        {
            playerInputHandler.ChangeInputActionMap(_gameActionMapName);
        }
    }

    public void ChangeCtrlersActMapToMenu()
    {
        foreach (var playerInputHandler in Controllers.Values)
        {
            playerInputHandler.ChangeInputActionMap(_menuActionMapName);
            Destroy(playerInputHandler.gameObject);
        }
        _controllers.Clear();
    }
    
    #endregion
    
    #region Unclassable functions

    private IEnumerator DeleteControllerCoroutine(int deviceId)
    {
        yield return new WaitForSeconds(.1f);
        AudioManager.Instance.PlaySfx("ControllerDisconnected");
        Destroy(_controllers[deviceId].Controller.gameObject);
        Destroy(_controllers[deviceId].gameObject);
        _controllers.Remove(deviceId);

        List<PlayerInputHandler> _playerInputHandlersFormControllers = _controllers.Values.ToList<PlayerInputHandler>();

        for(int i = 0; i < _playerInputHandlersFormControllers.Count; i++)
        {
            _playerInputHandlersFormControllers[i].Controller.SetPlayerIndex(i + 1);
        }
        
        _controllerSelectionMenu.MakeValidationButtonNotInteractable();

        _numberOfControllersConnectedOnMenu.text = $"{_controllers.Count} out of {_maxPlayerCount} controllers connected";
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
                playerInputHandler.Controller = Instantiate(_joystickPrefab, _controllerSelectionMenu.ControllerSelectionContainer);
                break;
            case Gamepad:
                playerInputHandler.Controller = Instantiate(_gamepadPrefab, _controllerSelectionMenu.ControllerSelectionContainer);
				break;
            case Keyboard:
                playerInputHandler.Controller =  Instantiate(_keyboardPrefab, _controllerSelectionMenu.ControllerSelectionContainer);
				break;
        }

        AudioManager.Instance.PlaySfx("ControllerConnected");

		playerInputHandler.Controller.SetPlayerIndex(_controllers.Count + 1);
		playerInputHandler.Controller.SetColorVisual(_controllers.Count);
		playerInputHandler.Controller.ControllerSelectionMode();
        playerInputHandler.Controller.PlayerInput = playerInput;
        
        //UI Stuff of the controller prefab
        playerInputHandler.Controller.gameObject.transform.localScale = Vector3.one;
        playerInputHandler.Controller.gameObject.transform.position = Vector3.zero;
        
        // Save of the playerInputHandler base on his device id
        _controllers.Add(inputDevice.deviceId, playerInputHandler);

        if (_controllers.Count >= _maxPlayerCount)
            _controllerSelectionMenu.MakeValidationButtonInteractable();
		
        _numberOfControllersConnectedOnMenu.text = $"{_controllers.Count} out of {_maxPlayerCount} controllers connected";
	}
    #endregion
    
    #region LocalMultiplayer implementation test simplification
    //TODO : comment when local multiplayer implementation is finished
    
    /*public List<PlayerInputHandler> _playerInputHandlers;

    public void CreateControllersDict()
    {
        _controllers = new Dictionary<int, PlayerInputHandler>();
        
        foreach (var pih in _playerInputHandlers)
        {
            if (pih.PlayerInput.devices.Count <= 0)
                continue;
            _controllers.Add(pih.PlayerInput.devices[0].deviceId, pih);
        }
    }*/
    
    #endregion

}
