using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Bot/BotDifficulty")]
public class BotDifficulty : ScriptableObject
{
    public string DifficultyName;
    
    [Header("Restrictions")]
    public bool IsMovementRestricted;
    public bool CanChargeShots;
    public bool CanSmash;
    
    [Header("Forces")]
    public float ServiceForce;
    public float MinimumShotForce;
    public float MaximumShotForce;
    
    [Header("Actions")]
    public List<NamedActions> PossibleActions;
    public List<NamedPhysicMaterials> PossiblePhysicMaterials;
}
