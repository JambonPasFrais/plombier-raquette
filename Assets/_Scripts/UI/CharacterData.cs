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
    public Color CharacterBackgroundColor;
    public Color CharacterNameBackgroundColor;
    public Color CharacterNameTextColor;
    
    [Header("Instance")]
    public GameObject HumanControllerPrefab;
    public GameObject AiControllerPrefab;
    public GameObject BasicModel;
    
    [Header("GD")]
    public CharacterParameters CharacterParameter;
    public CharacterCategory Category;

}
