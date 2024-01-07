using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private Image _background;
    [SerializeField] private TextMeshProUGUI _score;

	public void Initialize(Color color, int score)
	{
		_background.color = color;
		_score.text = score.ToString();
	}

	public void SetColor(Color color)
    {
        _background.color = color;
    }

    public void SetScore(int score)
    {
        _score.text = score.ToString();
    }
}
