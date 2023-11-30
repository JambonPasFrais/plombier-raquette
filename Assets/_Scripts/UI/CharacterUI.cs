using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour
{
    [SerializeField] private Image _charactersFace;
    [SerializeField] private Image _backgroundColor;
    [SerializeField] private GameObject _isSelectedImage;
    private CharacterData _character;
    private bool _isSelected;

    public bool IsSelected => _isSelected;

    public CharacterData Character => _character;

    public void SetVisual(CharacterData character)
    {
        _character = character;
        _charactersFace.sprite = character.Picture;
        _backgroundColor.color = character.CharacterColor;
    }

    public void SetSelected(bool isSelected)
    {
        _isSelected = isSelected;
        _isSelectedImage.SetActive(isSelected);
    }
}
