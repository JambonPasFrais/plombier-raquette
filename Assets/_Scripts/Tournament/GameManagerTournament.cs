using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerTournament : MonoBehaviour
{
    [Header("Instances")] 
    [SerializeField] private GameManager _gameManager;

    [SerializeField] private CharacterCreator _characterCreator;
    
    // Singleton
    private static GameManagerTournament _instance;

    #region Getters
    public static GameManagerTournament Instance => _instance;
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
        _characterCreator.InitCharacters();
        
        foreach (var t in _characterCreator.Characters)
        {
            _gameManager.AddControllers(t.GetComponent<ControllersParent>());
        }

        AudioManager.Instance.LaunchGameMusicCoroutine();

        _gameManager.Init();
    }

    #endregion
}
