using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour
{
    [SerializeField] private Image _charactersFace;
    [SerializeField] private Image _backgroundColor;
    private CharacterData _character;

    public void SetVisual(CharacterData character)
    {
        _character = character;
        _charactersFace.sprite = character.Picture;
        _backgroundColor.color = character.CharacterColor;
    }

	private void OnMouseDown()
	{
        Debug.Log($"Je clique sur {_character.Name} !");
	}
}
