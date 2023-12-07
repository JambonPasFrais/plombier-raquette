using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(fileName = "Character", menuName = "ScriptableObjects/Character/Character")]
public class CharacterData : ScriptableObject
{
    public string Name;
    public Sprite Picture;
    public GameObject Model3D;
    public Color CharacterColor;
    public Sprite CharactersLogo;
}
