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
    
    [Header("Bot Variables")]
    [SerializeField] private Transform[] _targets;
    [SerializeField] private Transform[] _firstSideTargetsPositions;
    [SerializeField] private Transform[] _secondSideTargetsPositions;
    
    [Header("GA")]
    [SerializeField] private Vector3 _characterLocalScaleModified;
    
    #region PRIVATE FIELDS
    private List<GameObject> _characters;
    #endregion
    
    #region GETTERS
    public List<GameObject> Characters => _characters;
    #endregion

    public void InitCharacters()
    {
        _characters = new List<GameObject>();

        List<CharacterData> playersCharacter;

        if (GameParameters.IsTournamentMode)
        {
            playersCharacter = new List<CharacterData>(GameParameters.Instance.CurrentTournamentInfos.RoundPlayers[
                GameParameters.Instance.CurrentTournamentInfos.CurrentRound - 1]);
            playersCharacter.RemoveRange(2, playersCharacter.Count - 2);
            GameParameters.Instance.SetCharactersPlayers(playersCharacter);
        }
        else
        {
            playersCharacter = GameParameters.Instance.PlayersCharacter;
        }
        
        //playersCharacter = GameParameters.Instance.PlayersCharacter;

        int nbCharInstantiated = 0;
        bool playerInTeamOne = true;

        // Create Human Characters
        for (int playerIndex = 0; playerIndex < GameParameters.Instance.LocalNbPlayers; playerIndex++)
        {
            // Init Controllers
            foreach (var playerInputHandler in ControllerManager.Controllers.Values)
            {
                if (playerInputHandler.PlayerInput.playerIndex == playerIndex)
                {
                    InitPlayerGo(playersCharacter[playerIndex].HumanControllerPrefab);
                    
                    // Last character added is the character instantiated above using "InitPlayerGo()"
                    //_characters[^1].transform.position = playerOriginalPositions[nbCharInstantiated].position;
                    playerInputHandler.Character = _characters[^1].GetComponent<Character>();
                    
                    playerInputHandler.Character.SetCharParameters(playersCharacter[playerIndex].CharacterParameter);

                    playerInputHandler.Character.PlayerController.PlayerTeam =
                        playerInTeamOne ? Teams.TEAM1 : Teams.TEAM2;
                    
                    playerInputHandler.InitDirectionController();
                    
                    playerInTeamOne = !playerInTeamOne;

                    nbCharInstantiated++;
                    break;
                }
            }
        }
        
        // Create AI Characters
        for (int aiIndex = GameParameters.Instance.LocalNbPlayers; aiIndex < playersCharacter.Count; aiIndex++)
        {
            InitPlayerGo(playersCharacter[aiIndex].AiControllerPrefab);
            //_characters[^1].transform.position = playerOriginalPositions[nbCharInstantiated].position;
            BotBehavior botBehavior = _characters[^1].GetComponent<BotBehavior>();
            botBehavior.InitTargetVariables(_targets, _firstSideTargetsPositions, _secondSideTargetsPositions);
            botBehavior.PlayerTeam =
                playerInTeamOne ? Teams.TEAM1 : Teams.TEAM2;
            playerInTeamOne = !playerInTeamOne; 
            
            // TODO : Init the charParameters here (how?)
            
            nbCharInstantiated++;
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

    public void DestroyCharacters()
    {
        foreach (var character in _characters)
        {
            Destroy(character);
        }
        
        _characters.Clear();
    }
}