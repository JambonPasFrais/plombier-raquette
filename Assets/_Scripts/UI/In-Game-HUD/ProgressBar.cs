using Google.Protobuf.WellKnownTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
	private Slider _progressBarSlider;

	public void Start()
	{
		_progressBarSlider = GetComponent<Slider>();
		_progressBarSlider.value = 0;
	}

	private void Update()
	{
		_progressBarSlider.value += Time.deltaTime * 0.1f;

		if(_progressBarSlider.value >= 1)
		{
			SceneManager.LoadScene("Clean_UI_Final");
		}
	}
}
