using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CharacterUI : MonoBehaviour
{
    #region PRIVATE FIELDS

    [SerializeField] private Image _charactersFace;
    [SerializeField] private Image _backgroundColor;
    [SerializeField] private GameObject _isSelectedImage;
    private CharacterSelectionMenu _characterSelectionMenu;
    private CharacterSelectionSoloMenu _characterSelectionSoloMenu;
    private CharacterData _character;
    private bool _isSelected;

    #endregion

    #region GETTERS

    public bool IsSelected => _isSelected;
    public CharacterData Character => _character;
    
    #endregion

    public void SetCharacterSelectionMenu(CharacterSelectionMenu characterSelectionMenu)
    {
        _characterSelectionMenu = characterSelectionMenu;
    }  
      
    public void SetCharacterSelectionSoloMenu(CharacterSelectionSoloMenu characterSelectionSoloMenu)
    {
        _characterSelectionSoloMenu = characterSelectionSoloMenu;
    }

    public void SetVisual(CharacterData character)
    {
        _character = character;
        _charactersFace.sprite = character.Picture;
        _backgroundColor.color = character.CharacterPrimaryColor;
		gameObject.name = _character.Name;
	}

	public void SetSelected(bool isSelected)
    {
        _isSelected = isSelected; 
        _isSelectedImage.SetActive(isSelected);
    }
    
    public void Select()
    {
        if(_characterSelectionMenu != null)
        {
            _characterSelectionMenu.HandleCharacterSelectionInput(this);
        }
        else if (_characterSelectionSoloMenu != null)
        {
            _characterSelectionSoloMenu.HandleCharacterSelectionSoloMenu(this);
        }
    }
}
