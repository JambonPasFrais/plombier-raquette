using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class Controller : MonoBehaviour
{
    [SerializeField] private float _speed;
    private Vector2 _movementDir;
    public bool IsSelectingCharacter;

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
        }
    }
    #endregion

    private void Update()
    {
        transform.Translate(_movementDir * Time.deltaTime * _speed);
    }
}
