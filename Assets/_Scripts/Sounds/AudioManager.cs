using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;

	#region PRIVATE FIELDS

	[Header("References")]
	[SerializeField] private List<SoundData> _everySound = new List<SoundData>();
	[SerializeField] public AudioMixer _audioMixer;
    [SerializeField] private AudioMixerGroup _audioMixerMaster;
    [SerializeField] private AudioMixerGroup _audioMixerSFX;
    [SerializeField] private AudioMixerGroup _audioMixerMusic;
	[SerializeField] private Transform _audioContainer;

	private AudioSource _currentMusic;
	private SoundData _currentSoundData;
	private Coroutine _musicCoroutine;
	private Dictionary<string, List<AudioClip>> _sfxSounds = new Dictionary<string, List<AudioClip>>();
	private Dictionary<string, SoundData> _musicSounds = new Dictionary<string, SoundData>();

	#endregion

	#region GETTERS

	public static AudioMixer AudioMixer => _instance._audioMixer;

	public static AudioManager Instance => _instance;

	#endregion

	#region UNITY FUNCTIONS

	private void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
			DontDestroyOnLoad(gameObject);

			InitSoundsAndDict();
		}
		else
			Destroy(gameObject);
	}

	private void Start()
	{
		_currentMusic = null;
		_musicCoroutine = null;

		StartMainMenuMusicCoroutine();
	}

	#endregion

	private void InitSoundsAndDict()
	{
		foreach (var sound in _everySound)
		{
			sound.ShuffleAudioClips();

			switch (sound.AudioType)
			{
				case AudioType.SFX:
					_sfxSounds.Add(sound.Name, sound.Clips);
					break;
				case AudioType.MUSIC:
					_musicSounds.Add(sound.Name, sound);
					break;
				default:
					Debug.Log("Error audio type not recognized");
					break;
			}
		}
	}

	#region MUSIC LAUNCH FUNCTIONS

	public void StartMainMenuMusicCoroutine()
	{
		if (_musicCoroutine != null)
			StopCoroutine(_musicCoroutine);

		_musicCoroutine = StartCoroutine(MainMenuMusicLoop());
	}

	public void LaunchGameMusicCoroutine()
	{
		if (_musicCoroutine != null)
		{
			StopCoroutine(_musicCoroutine);
		}

		_musicCoroutine = StartCoroutine(GameMusicLoop());
	}

	public void LaunchEndGameMusic()
	{
		if (_musicCoroutine != null)
		{
			StopCoroutine(_musicCoroutine);
		}

		_musicCoroutine = StartCoroutine(EndGameMusicLoop());
	}

	#endregion

	#region PLAYING SOUNDS FUNCTIONS

	public void PlayMusic(string musicName)
	{
		if(_currentMusic != null)
			_currentMusic.Stop();

		_currentSoundData = _musicSounds[musicName];

		_currentMusic = PlaySound(_currentSoundData.Clips[_currentSoundData.CurrentClipIndex], musicName, _audioMixerMusic, false);

		_currentSoundData.CurrentClipIndex = (_currentSoundData.CurrentClipIndex + 1) % _currentSoundData.Clips.Count;
	}
	public void PlaySfx(string sfxName)
	{
		PlaySound(_sfxSounds[sfxName][UnityEngine.Random.Range(0, _sfxSounds[sfxName].Count)], sfxName, _audioMixerSFX, false);
	}

	public void PlaySfx(AudioClip clip)
	{
		PlaySound(clip, clip.name, _audioMixerSFX, false);
	}

	private AudioSource PlaySound(AudioClip clip, string musicName, AudioMixerGroup audioMixerGroup, bool isLooping)
	{
		if(clip != null)
		{
			GameObject go = new GameObject(musicName);
			go.transform.parent = _audioContainer;

			AudioSource audioSource = go.AddComponent<AudioSource>();
			audioSource.clip = clip;
			audioSource.outputAudioMixerGroup = audioMixerGroup;
			audioSource.loop = isLooping;
			audioSource.Play();

			Destroy(go, audioSource.clip.length);

			return audioSource;
		}

		return null;
	}

	#endregion

	#region COROUTINES

	private IEnumerator MainMenuMusicLoop()
	{
		PlayMusic("MainMenuMusics");
		yield return new WaitForSeconds(_currentMusic.clip.length);

		StartCoroutine(MainMenuMusicLoop());
	}
	
	private IEnumerator GameMusicLoop()
	{
		PlayMusic("MatchMusics");
		yield return new WaitForSeconds(_currentMusic.clip.length);

		StartCoroutine(GameMusicLoop());
	}
	
	private IEnumerator EndGameMusicLoop()
	{
		PlayMusic("win");
		yield return new WaitForSeconds(_currentMusic.clip.length);

		StartCoroutine(EndGameMusicLoop());
	}

	#endregion
}
