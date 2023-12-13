using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scripts created to manage the instantiation of the characters in the Local Multiplayer Game
/// This script will need to create each character using the character.cs file (prefab), using the right parameters
/// Then it will need to add the right controller based on the player type (cpu or human)
/// </summary>
public class CharacterCreator : MonoBehaviour
{
    [Header("Instances")] 
    //[SerializeField] private PlayerController _playerControllerPrefab;
    [SerializeField] private Character _characterPrefab;
    [SerializeField] private Transform _charContainer;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}