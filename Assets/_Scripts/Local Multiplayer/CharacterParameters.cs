using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Character parameters refers to the different types of characters that exist such as "Heavy", "Agile", "Mixed", ...
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObjects/Character/CharacterParameters", fileName = "CharacterParameters")]
public class CharacterParameters : ScriptableObject
{
    public float MovSpeed;
    public float MinShotForce;
    public float MaxShotForce;
    public float MinHitKeyPressTimeToIncrementForce;
    public float MaxHitKeyPressTime;
}
