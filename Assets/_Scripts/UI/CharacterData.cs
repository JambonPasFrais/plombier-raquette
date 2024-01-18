using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable, CreateAssetMenu(fileName = "Character", menuName = "ScriptableObjects/Character/Character")]
public class CharacterData : ScriptableObject
{
    [Header("GA")]
    public string Name;
    public Sprite Picture;
    public Sprite CharactersLogo;
    public Color CharacterPrimaryColor;
    public Color CharacterSecondaryColor;
    
    [Header("Instance")]
    public GameObject HumanControllerPrefab;
    public GameObject AiControllerPrefab;
    public GameObject BasicModel;
    
    [Header("GD")]
    public CharacterParameters CharacterParameter;
    public CharacterCategory Category;

    [Header("Sounds")]
    public List<SoundData> CharacterSounds = new List<SoundData>();

    private Dictionary<string, SoundData> _characterSoundsDict;

    private System.Random _random = new System.Random();

	public void Init()
	{
        if (_characterSoundsDict != null)
            return;

        _characterSoundsDict = new Dictionary<string, SoundData>();
        
		foreach(var sound in CharacterSounds)
        {
            _characterSoundsDict.Add(sound.Name, sound);
        }
	}

    public void PlaySound(string soundName)
    {
        AudioManager.Instance.PlaySfx(_characterSoundsDict[soundName].Clips[random.Next(0, _characterSoundsDict[soundName].Clips.Count)]);
    }
}
