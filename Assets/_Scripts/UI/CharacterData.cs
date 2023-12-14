using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable, CreateAssetMenu(fileName = "Character", menuName = "ScriptableObjects/Character/Character")]
public class CharacterData : ScriptableObject
{
    [Header("GA")]
    public string Name;
    public Sprite Picture;
    public Sprite CharactersLogo;
    public Color CharacterColor;
    
    [Header("Instance")]
    public GameObject Prefab;
    
    [Header("GD")]
    public CharacterTypes CharacterType;
}
