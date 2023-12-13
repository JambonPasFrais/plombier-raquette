using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GameManager))]
public class GameManagerLocalMultiplayer : MonoBehaviour
{
    [Header("Instances")] 
    [SerializeField] private GameManager _gameManager;
    
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
        _gameManager = GetComponent<GameManager>();
    }

    #endregion
}
