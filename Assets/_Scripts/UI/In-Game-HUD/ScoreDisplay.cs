using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private Image _background;
    [SerializeField] private TextMeshProUGUI _score;

	public void Initialize(Color color, string score)
	{
		_background.color = color;
        SetScore(score);
	}

    public void SetScore(string score)
    {
        _score.text = score.ToString();
    }
}
