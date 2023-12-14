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

        // Create Human Characters
        for (int playerIndex = 0; playerIndex < GameParameters.LocalNbPlayers; playerIndex++)
        {
            
            
            // Init Controllers
            foreach (var playerInputHandler in ControllerManager.Controllers.Values)
            {
                if (playerInputHandler.PlayerInput.playerIndex == playerIndex)
                {
                    // Instantiation after CharParameters initialization because we use this variable in the Start() of the Character.cs file
                    playerInputHandler.Character.SetCharParameters(playersCharacter[playerIndex].CharacterParameter);

                    InitPlayerGo(playersCharacter[playerIndex].HumanControllerPrefab);
                    // Last character added is the character instantiated above using "InitPlayerGo()"
                    playerInputHandler.Character = _characters[^1].GetComponent<Character>();
                    break;
                }
            }
        }
        
        // Create AI Characters
        for (int aiIndex = GameParameters.LocalNbPlayers - 1; aiIndex < playersCharacter.Count; aiIndex++)
        {
            InitPlayerGo(playersCharacter[aiIndex].AiControllerPrefab);
        }
    }

    
    /// <summary>
    /// Function that will instantiate the prefab in parameter, will set his local scale and position and
    /// will add it to the _characters list !
    /// </summary>
    /// <param name="prefab"></param>
    private void InitPlayerGo(GameObject prefab)
    {
        GameObject aiGo = Instantiate(prefab, _charContainer);
        aiGo.transform.localScale = _characterLocalScaleModified;
        aiGo.transform.position = Vector3.zero;
            
        _characters.Add(aiGo);
    }
}