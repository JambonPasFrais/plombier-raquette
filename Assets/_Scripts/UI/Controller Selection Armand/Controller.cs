using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    [Header("Instances")]
    [SerializeField] private GameObject _controllerMenuIcon;
    [SerializeField] private GameObject _characterSelectionIcon;
    [SerializeField] private Image _imgCharSelectionIcon;

    [HideInInspector] public PlayerInput PlayerInput;

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

    #region PLAYER INPUT COMPONENT FUNCTIONS (called externally)
    public void TryPunch()
    {
        Debug.Log(PlayerInput.playerIndex);
        
        if (IsSelectingCharacter)
            return;
        
        transform.DOComplete();
        transform.DOPunchScale(Vector3.one * .1f, .2f);
    }

    public void TryMove(Vector2 readValue)
    {
        if (!IsSelectingCharacter)
            return;

        _movementDir = readValue;
    }

    public void TrySelect()
    {
        if (!IsSelectingCharacter)
            return;
        
        if (_characterSelected)
            return;
        
        Ray ray = Camera.main.ScreenPointToRay(Camera.main.WorldToScreenPoint(transform.position));
        if (ControllerManager.Instance.CharacterSelectionMenu.HandleCharacterSelectionInput(ray, PlayerInput.playerIndex))
        {
            _characterSelected = true;
            _imgCharSelectionIcon.color /= 2;
        }
    }

    public void TryDeselect()
    {
        if (!IsSelectingCharacter)
            return;
        
        if (!_characterSelected)
            return;

        if (ControllerManager.Instance.CharacterSelectionMenu.HandleCharacterDeselectionInput(PlayerInput.playerIndex))
        {
            _characterSelected = false;
            _imgCharSelectionIcon.color = Color.white;
        }
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
