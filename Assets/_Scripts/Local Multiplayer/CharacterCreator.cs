using System;
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
    [SerializeField] private Transform _charContainer;
    
    [Header("GA")]
    [SerializeField] private Vector3 _characterLocalScaleModified;
    
    #region PRIVATE FIELDS
    private List<GameObject> _characters;
    #endregion

    private void OnEnable()
    {
        InitCharacters();
    }

    private void InitCharacters()
    {
        _characters = new List<GameObject>();

        List<CharacterData> playersCharacter = GameParameters.PlayersCharacters;

        for (int playerIndex = 0; playerIndex < playersCharacter.Count; playerIndex++)
        {
            // Init Game object
            GameObject playerGo = Instantiate(playersCharacter[playerIndex].Prefab, _charContainer);
            playerGo.transform.localScale = _characterLocalScaleModified;
            playerGo.transform.position = Vector3.zero;
            
            _characters.Add(playerGo);
            
            // Init Controllers
            //ControllerManager.Instance.Controllers[0].Controller.PlayerInput.playerIndex;
        }
    }
}