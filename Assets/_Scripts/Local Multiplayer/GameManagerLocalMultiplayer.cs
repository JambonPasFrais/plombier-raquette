using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

//[RequireComponent(typeof(GameManager))]
public class GameManagerLocalMultiplayer : MonoBehaviour
{
    [Header("Instances")] 
    [SerializeField] private GameManager _gameManager;

    [SerializeField] private CharacterCreator _characterCreator;
    
    // Singleton
    private static GameManagerLocalMultiplayer _instance;

    #region Getters
    public static GameManagerLocalMultiplayer Instance => _instance;
    #endregion
    
    #region Unity Functions
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        //_gameManager = GetComponent<GameManager>();
        _characterCreator.InitCharacters();
        foreach (var t in _characterCreator.Characters)
        {
            _gameManager.AddControllers(t.GetComponent<ControllersParent>());
        }

        AudioManager.Instance.LaunchGameMusicCoroutine();

        _gameManager.Init();
    }

    #endregion

    public void TestScene()
    {
        //SceneManager.LoadScene("Local_Multiplayer");
        GameManager.Instance.EndOfGame(0);
    }
}
