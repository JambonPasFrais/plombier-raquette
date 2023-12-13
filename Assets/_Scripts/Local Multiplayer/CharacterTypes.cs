using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Character/CharacterTypes", fileName = "CharacterTypes")]
public class CharacterTypes : ScriptableObject
{
    public float MovSpeed;
    public float MinShotForce;
    public float MaxShotForce;
    public float MinHitKeyPressTimeToIncrementForce;
    public float MaxHitKeyPressTime;
}
