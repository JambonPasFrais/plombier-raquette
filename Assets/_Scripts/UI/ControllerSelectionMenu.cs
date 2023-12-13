using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllerSelectionMenu : MonoBehaviour
{
    [SerializeField] private Transform _controllerSelectionContainer;
    [SerializeField] private Transform _characterSelectionContainer;
    [SerializeField] private CharacterSelectionMenu _characterSelectionMenu;
    [SerializeField] private Button _validationButton;
    
    #region Getters

    public Transform ControllerSelectionContainer => _controllerSelectionContainer;
    
    #endregion

    #region Listeners
    public void OnControllerSelectionLoad()
    {   
        ControllerManager.Instance.Init(_characterSelectionMenu, this);
        ControllerManager.Instance.ControllerCanBeAdded();
    }

    public void OnBackToControllerSelection()
    {
        ControllerManager.Instance.ControllerCanBeAdded();
        ControllerManager.Instance.SwitchCtrlersToCtrlSelectMode(_controllerSelectionContainer);
    }
    
    public void OnExitControllerSelection()
    {
        ControllerManager.Instance.ControllerCantBeAdded();
        OnResetControllers();
    }

    public void OnResetControllers()
    {
        ControllerManager.Instance.DestroyControllers();
        MakeValidationButtonNotInteractable();
    }

    public void OnValidateControllerSelection()
    {
        ControllerManager.Instance.ControllerCantBeAdded();
        ControllerManager.Instance.SwitchCtrlersToCharSelectMode(_characterSelectionContainer);
    }
    #endregion

    public void MakeValidationButtonNotInteractable()
    {
        _validationButton.interactable = false;
    }

    public void MakeValidationButtonInteractable()
    {
        _validationButton.interactable = true;
    }
}
