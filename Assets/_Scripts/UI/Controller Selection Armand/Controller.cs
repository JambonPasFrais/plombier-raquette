using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerInput))]
public class Controller : MonoBehaviour
{
    [Header("Instances")]
    [SerializeField] private GameObject _controllerMenuIcon;
    [SerializeField] private GameObject _characterSelectionIcon;
    [SerializeField] private Image _imgCharSelectionIcon;
    [SerializeField] private PlayerInput _playerInput;

    //[Header("Parameters")] private int _playerIndex;

    [Header("Game Feel")]
    [SerializeField] private float _speed;
    
    
    private Vector2 _movementDir;
    [HideInInspector] public bool IsSelectingCharacter;
    private bool _characterSelected;

    #region UNITY FUNCTIONS
    private void Update()
    {
        transform.Translate(_movementDir * Time.deltaTime * _speed);
    }
    #endregion

    #region PLAYER INPUT COMPONENT FUNCTIONS
    public void OnPunch(InputAction.CallbackContext context)
    {
        //Debug.Log(_playerInput.playerIndex);
        
        if (IsSelectingCharacter)
            return;
        
        if (context.performed)
        {
            transform.DOComplete();
            transform.DOPunchScale(Vector3.one * .1f, .2f);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!IsSelectingCharacter)
            return;
        
        _movementDir = context.ReadValue<Vector2>();
    }

    public void OnSelect(InputAction.CallbackContext context)
    {
        if (!IsSelectingCharacter)
            return;
        
        if (_characterSelected)
            return;
        
        if (context.performed)
        {
            Ray ray = Camera.main.ScreenPointToRay(Camera.main.WorldToScreenPoint(transform.position));
            if (ControllerManager.Instance.CharacterSelectionMenu.HandleCharacterSelectionInput(ray, _playerInput.playerIndex))
            {
                _characterSelected = true;
                _imgCharSelectionIcon.color /= 2;
            }
        }
    }

    public void OnDeselect(InputAction.CallbackContext context)
    {
        if (!IsSelectingCharacter)
            return;
        
        if (!_characterSelected)
            return;

        if (context.performed)
        {
            if (ControllerManager.Instance.CharacterSelectionMenu.HandleCharacterDeselectionInput(_playerInput.playerIndex))
            {
                _characterSelected = false;
                _imgCharSelectionIcon.color = Color.white;
            }
        }
    }
    #endregion

    #region EVENT LISTENERS
    
    public void OnDeviceLost(PlayerInput playerInput)
    {
        ControllerManager.Instance.DeletePlayerFromControllerSelection(playerInput);
    }
    
    #endregion
    
    #region CALLED EXTERNALLY
    public void ControllerSelectionMode()
    {
        IsSelectingCharacter = false;
        
        _controllerMenuIcon.SetActive(true);
        _characterSelectionIcon.SetActive(false);
    }

    public void CharacterSelectionMode()
    {
        IsSelectingCharacter = true;
        
        _controllerMenuIcon.SetActive(false);
        _characterSelectionIcon.SetActive(true);
    }
    #endregion
}
