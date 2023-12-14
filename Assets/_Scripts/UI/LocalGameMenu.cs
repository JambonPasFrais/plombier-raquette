using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalGameMenu : MonoBehaviour
{
    [SerializeField] private List<Sprite> _charactersSprites = new List<Sprite>();
	[SerializeField] private Image _characterImage;

	private void Start()
	{
		System.Random rand = new System.Random();

		_characterImage.sprite = _charactersSprites[rand.Next(0, _charactersSprites.Count)];
	}

	private void OnEnable()
	{
		System.Random rand = new System.Random();

		_characterImage.sprite = _charactersSprites[rand.Next(0, _charactersSprites.Count)];
	}
}
