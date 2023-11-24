using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
	#region PRIVATE FIELDS

	[Header("Instance")]
	[SerializeField] private TMP_Dropdown _resolutionDropdown;
	[SerializeField] private TMP_Dropdown _fpsDropdown;
	[SerializeField] private TMP_Dropdown _qualityDropdown;
	[SerializeField] private Slider _globalVolumeSlider;
	[SerializeField] private TextMeshProUGUI _globalVolumeText;
	[SerializeField] private Slider _sfxVolumeSlider;
	[SerializeField] private TextMeshProUGUI _sfxVolumeText;
	[SerializeField] private Slider _musicVolumeSlider;
	[SerializeField] private TextMeshProUGUI _musicVolumeText;
	[SerializeField] private AudioMixer _audioMixer;

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

	#endregion

	private void Start()
	{
		CreateResolutionsItems();
		CreateMaxFpsItems();
		CreateQualityItems();
		UpdateSliders();
	}

	#region SLIDERS ITEMS CREATION

	private void CreateResolutionsItems()
	{
		_resolutions = Screen.resolutions;
		_resolutionDropdown.ClearOptions();

		List<string> options = new List<string>();

		int currentResolutionIndex = 0;
		int cpt = 0;

		foreach(var resolution in _resolutions)
		{
			options.Add(resolution.width + " x " + resolution.height);

			if (resolution.width == Screen.currentResolution.width && resolution.height == Screen.currentResolution.height)
				currentResolutionIndex = cpt;
		}

		_resolutionDropdown.AddOptions(options);
		_resolutionDropdown.value = currentResolutionIndex;
		_resolutionDropdown.RefreshShownValue();
	}

	private void CreateMaxFpsItems()
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
		_fpsDropdown.value = _maxFpsDropdownOptions.Count - 1;
		_fpsDropdown.RefreshShownValue();
	}

	private void CreateQualityItems()
	{
		QualitySettings.GetQualitySettings();
		_qualityDropdown.ClearOptions();
		_qualityDropdown.AddOptions(_qualities);
		_qualityDropdown.value = QualitySettings.GetQualityLevel();
		_qualityDropdown.RefreshShownValue();
	}

	private void UpdateSliders()
	{
		_globalVolumeText.text = $"{Math.Round(_globalVolumeSlider.value, 2) * 100}";
		_sfxVolumeText.text = $"{Math.Round(_sfxVolumeSlider.value, 2) * 100}";
		_musicVolumeText.text = $"{Math.Round(_musicVolumeSlider.value, 2) * 100}";
	}

	#endregion

	#region AUDIO SETTINGS

	public void SetGlobalVolume(float volume)
	{
		_audioMixer.SetFloat("MAIN_Volume", Mathf.Log10(volume) * 20);
		_globalVolumeText.text = (Math.Round(volume, 2) * 100).ToString();
	}

	public void SetSfxVolume(float volume)
	{
		_audioMixer.SetFloat("SFX_Volume", Mathf.Log10(volume) * 20);
		_sfxVolumeText.text = (Math.Round(volume, 2) * 100).ToString();
	}

	public void SetMusicVolume(float volume)
	{
		_audioMixer.SetFloat("MUSIC_Volume", Mathf.Log10(volume) * 20);
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

	public void SetFpsMax(int fpsIndex)
	{
		int fps = _maxFps[fpsIndex];
		Application.targetFrameRate = fps;
	}

	#endregion
}
