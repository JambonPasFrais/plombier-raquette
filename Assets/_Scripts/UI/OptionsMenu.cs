using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
	#region PRIVATE FIELDS

	[SerializeField] private Transform _navigationButtonsParent;	

	[Header("Display Istances")]
	[SerializeField] private TMP_Dropdown _resolutionDropdown;
	[SerializeField] private TMP_Dropdown _fpsDropdown;
	[SerializeField] private TMP_Dropdown _qualityDropdown;

	[Header("Audio Instances")]
	[SerializeField] private Slider _globalVolumeSlider;
	[SerializeField] private TextMeshProUGUI _globalVolumeText;
	[SerializeField] private Slider _sfxVolumeSlider;
	[SerializeField] private TextMeshProUGUI _sfxVolumeText;
	[SerializeField] private Slider _musicVolumeSlider;
	[SerializeField] private TextMeshProUGUI _musicVolumeText;
	[SerializeField] private AudioMixer _audioMixer;

	[Header("First Menu Element Instances")]
	[SerializeField] private Selectable _firstDisplayButton;
	[SerializeField] private Selectable _firstAudioButton;
	[SerializeField] private Selectable _firstControlsButton;

	[Header("Option Menus")]
	[SerializeField] private GameObject _displayOptions;
	[SerializeField] private GameObject _audioOptions;
	[SerializeField] private GameObject _controlsOptions;

	[Header("Parameters")]
	[SerializeField] private List<int> _maxFps = new List<int>
	{
		30,
		60,
		90,
		120,
		-1
	};
	[SerializeField] private List<string> _qualities = new List<string>
	{
		"Very Low",
		"Low",
		"Medium",
		"High",
		"Ultra"
	};

	private Resolution[] _resolutions;
	private List<string> _maxFpsDropdownOptions;
	private List<Button> _navigationButtons = new List<Button>();

	#endregion

	private void Start()
	{
		_audioMixer = AudioManager.AudioMixer;

		for(int i = 0; i < _navigationButtonsParent.childCount; i++) 
		{ 
			_navigationButtons.Add(_navigationButtonsParent.GetChild(i).gameObject.GetComponent<Button>());
		}

		SetMenuNavigation(_firstDisplayButton);

		ShowDisplayParameters();

		CreateResolutionsItems();
		CreateMaxFPSItems();
		CreateQualityItems();
		UpdateSliders();
	}

	#region SLIDERS ITEMS CREATION

	private void CreateResolutionsItems()
	{
		_resolutions = Screen.resolutions;
		_resolutionDropdown.ClearOptions();

		List<string> options = new List<string>();

		int currentResolutionIndex = 0, cpt = 0;

		foreach(var resolution in _resolutions)
		{
			options.Add(resolution.width + " x " + resolution.height);

			if (resolution.width == Screen.currentResolution.width && resolution.height == Screen.currentResolution.height)
				currentResolutionIndex = cpt;

			cpt++;
		}

		options.Reverse();

		_resolutionDropdown.AddOptions(options);
		_resolutionDropdown.value = options.Count - currentResolutionIndex - 1;
		_resolutionDropdown.RefreshShownValue();
	}

	private void CreateMaxFPSItems()
	{
		_fpsDropdown.ClearOptions();

		_maxFpsDropdownOptions = new List<string>();

		foreach (var fps in _maxFps)
		{
			if (fps == -1) // -1 is equivalent to an unlimited framerate for Application.targetFrameRate
				_maxFpsDropdownOptions.Add("Unlimited");
			else
				_maxFpsDropdownOptions.Add(fps.ToString());
		}

		_fpsDropdown.AddOptions(_maxFpsDropdownOptions);
		_fpsDropdown.value = 0;
		_fpsDropdown.RefreshShownValue();
	}

	private void CreateQualityItems()
	{
		QualitySettings.GetQualitySettings();
		_qualityDropdown.ClearOptions();
		_qualityDropdown.AddOptions(_qualities);
		_qualityDropdown.value = 0;
		_qualityDropdown.RefreshShownValue();
	}

	private void UpdateSliders()
	{
		float volume;

		_audioMixer.GetFloat("Master_Volume", out volume);
		_globalVolumeSlider.value = Mathf.Pow(10, volume / 20f);

		_audioMixer.GetFloat("SFX_Volume", out volume);
		_sfxVolumeSlider.value = Mathf.Pow(10, volume / 20f);
		_sfxVolumeSlider.value = Mathf.Pow(10, volume / 20f);

		_audioMixer.GetFloat("Music_Volume", out volume);
		_musicVolumeSlider.value = Mathf.Pow(10, volume / 20f);

		_globalVolumeText.text = $"{Math.Round(_globalVolumeSlider.value, 2) * 100}";
		_sfxVolumeText.text = $"{Math.Round(_sfxVolumeSlider.value, 2) * 100}";
		_musicVolumeText.text = $"{Math.Round(_musicVolumeSlider.value, 2) * 100}";
	}

	#endregion

	#region AUDIO SETTINGS

	public void SetGlobalVolume(float volume)
	{
		_audioMixer.SetFloat("Master_Volume", Mathf.Log10(volume) * 20);
		_globalVolumeText.text = (Math.Round(volume, 2) * 100).ToString();
	}

	public void SetSFXVolume(float volume)
	{
		_audioMixer.SetFloat("SFX_Volume", Mathf.Log10(volume) * 20);
		_sfxVolumeText.text = (Math.Round(volume, 2) * 100).ToString();
	}

	public void SetMusicVolume(float volume)
	{
		_audioMixer.SetFloat("Music_Volume", Mathf.Log10(volume) * 20);
		_musicVolumeText.text = (Math.Round(volume, 2) * 100).ToString();
	}

	#endregion

	#region DISPLAY SETTINGS

	public void SetQuality(int qualityIndex)
	{
		QualitySettings.SetQualityLevel(qualityIndex);
	}

	public void SetFullScreen(bool isFullscreen)
	{
		Screen.fullScreen = isFullscreen;
	}
	public void SetResolution(int resolutionIndex)
	{
		Resolution resolution = _resolutions[resolutionIndex];
		Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
	}

	public void SetFPSMax(int fpsIndex)
	{
		int fps = _maxFps[fpsIndex];
		Application.targetFrameRate = fps;
	}

	#endregion

	#region MENU DISPLAYING

	public void ShowControlsParameters()
	{
		_controlsOptions.SetActive(true);
		_audioOptions.SetActive(false);
		_displayOptions.SetActive(false);
	}

	public void ShowDisplayParameters()
	{
		MenuManager.Instance.CurrentEventSystem.SetSelectedGameObject(_firstDisplayButton.gameObject);
		SetMenuNavigation(_firstDisplayButton);
		_controlsOptions.SetActive(false);
		_audioOptions.SetActive(false);
		_displayOptions.SetActive(true);
	}
	
	public void ShowAudioParameters()
	{
		MenuManager.Instance.CurrentEventSystem.SetSelectedGameObject(_firstAudioButton.gameObject);
		SetMenuNavigation(_firstAudioButton);
		_controlsOptions.SetActive(false);
		_audioOptions.SetActive(true);
		_displayOptions.SetActive(false);
	}

	public void ResetMenu()
	{
		SetMenuNavigation(_firstDisplayButton);
		_controlsOptions.SetActive(false);
		_audioOptions.SetActive(false);
		_displayOptions.SetActive(true);
	}

	#endregion

	private void SetMenuNavigation(Selectable firstMenuElement)
	{
		foreach (var button in _navigationButtons)
		{
			MenuManager.Instance.SetButtonNavigation(button, button.navigation.selectOnLeft, firstMenuElement, button.navigation.selectOnUp, button.navigation.selectOnDown);
		}
	}
}
