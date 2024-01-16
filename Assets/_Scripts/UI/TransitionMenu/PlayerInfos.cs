using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfos : MonoBehaviour
{
    [SerializeField] private Image _background;
    [SerializeField] private TextMeshProUGUI _playerType;
    [SerializeField] private TextMeshProUGUI _charactersName;

	public void Init(CharacterData data, string playerType)
	{
		_background.color = data.CharacterPrimaryColor;
		_charactersName.text = data.Name;
		_playerType.text = playerType;
	}
}
