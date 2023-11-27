using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RulesMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _COMDifficultyText;
	[SerializeField]
	private List<string> _difficulties = new List<string>
	{
		"Easy",
		"Medium",
		"Hard",
		"Expert",
		"Invincible"
	};
	private int _currentDifficulty;

	private void Start()
	{
		_currentDifficulty = 1;
		_COMDifficultyText.text = _difficulties[_currentDifficulty];
	}

	public void ModifyCOMDifficulty(int value)
    {
        _currentDifficulty = Mathf.Clamp(_currentDifficulty + value, 0, _difficulties.Count - 1);
        _COMDifficultyText.text = _difficulties[_currentDifficulty];
    }
    
    /*public void DecreaseCOMDifficulty()
    {
        _currentDifficulty = (_currentDifficulty + 1) % _difficulties.Count;
        _COMDifficultyText.text = _difficulties[_currentDifficulty];
    }*/
}
