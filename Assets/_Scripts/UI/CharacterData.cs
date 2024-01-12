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

    private Dictionary<string, SoundData> CharacterSoundsDict = new Dictionary<string, SoundData>();

    private System.Random random = new System.Random();

	public void Init()
	{
		foreach(var sound in CharacterSounds)
        {
            CharacterSoundsDict.Add(sound.Name, sound);
        }
	}

    public void PlaySound(string name)
    {
        AudioManager.Instance.PlaySfx(CharacterSoundsDict[name].Clips[random.Next(0, CharacterSoundsDict[name].Clips.Count)]);
    }
}
