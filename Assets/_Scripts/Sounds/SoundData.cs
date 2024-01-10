using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class SoundData
{
    public string Name;
    public AudioType AudioType;
    public List<AudioClip> Clips;
    public int CurrentClipIndex = 0;

    private System.Random _random = new System.Random();

	public void ShuffleAudioClips()
	{
        Clips = Clips.OrderBy(a => _random.Next()).ToList();
	}
}
