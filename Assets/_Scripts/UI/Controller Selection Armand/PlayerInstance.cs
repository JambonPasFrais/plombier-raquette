using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInstance : MonoBehaviour
{
    [Header("GA")] [SerializeField] private float _punchStrength;
    [SerializeField] private float _punchDuration;
    
    public InputDevice Device;
    public PlayerInput PlayerInput;
    
    private Vector3 _originalScale;

    private void Start()
    {
        PlayerInput = GetComponent<PlayerInput>();
        _originalScale = transform.localScale;
    }

    public void OnBump(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        transform.DOComplete();
        transform.DOPunchScale(Vector3.one * _punchStrength, _punchDuration);
    }
}
