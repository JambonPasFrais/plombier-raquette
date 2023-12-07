using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class Controller : MonoBehaviour
{
    [Header("Instances")]
    [SerializeField] private GameObject _controllerMenuIcon;
    [SerializeField] private GameObject _characterSelectionIcon;
    
    [Header("Game Feel")]
    [SerializeField] private float _speed;
    
    
    private Vector2 _movementDir;
    [HideInInspector] public bool IsSelectingCharacter;

    #region PLAYER INPUT COMPONENT FUNCTIONS
    public void OnPunch(InputAction.CallbackContext context)
    {
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
        
        if (context.performed)
        {
            //Select character (ray cast to character)
            Debug.Log("Selection");
        }
    }
    #endregion

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
    
    private void Update()
    {
        transform.Translate(_movementDir * Time.deltaTime * _speed);
    }
}
