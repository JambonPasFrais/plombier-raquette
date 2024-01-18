using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControllerSelectionMenu : MonoBehaviour
{
    [Header("Instances")]
    [SerializeField] private Transform _controllerSelectionContainer;
    [SerializeField] private Transform _characterSelectionContainer;
    [SerializeField] private CharacterSelection _characterSelectionMenu;
    [SerializeField] private Button _validationButton;
    [SerializeField] private TextMeshProUGUI _controllerCountSentence;
    
    #region Getters

    public Transform ControllerSelectionContainer => _controllerSelectionContainer;
    
    #endregion

    #region Listeners

    public void OnControllerSelectionLoad()
    {   
        ControllerManager.Instance.Init(_characterSelectionMenu, this);
        ControllerManager.Instance.ControllerCanBeAdded();
        MenuManager.Instance.CurrentEventSystem.SetSelectedGameObject(null);
        MakeValidationButtonNotInteractable();
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

    #region CALLED EXTERNALLY
    
    public void MakeValidationButtonNotInteractable()
    {
        _validationButton.interactable = false;
    }

    public void MakeValidationButtonInteractable()
    {
        _validationButton.interactable = true;
        MenuManager.Instance.CurrentEventSystem.SetSelectedGameObject(_validationButton.gameObject);
    }

    public void UpdateControllerCountSentence(int nbControllersConnected, int nbControllersTotal)
    {
        _controllerCountSentence.text = $"{nbControllersConnected} out of {nbControllersTotal} controllers connected";
    }
    
    #endregion
}
