using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OtherPlayerCard : PlayerCard
{
    #region PRIVATE FIELDS

    [SerializeField] private Sprite _readySprite;
    [SerializeField] private Sprite _notReadySprite;
    [SerializeField] private Image _readyStateImg;
    [SerializeField] private CharacterUI _characterUI;


    #endregion

    public override void Initialize(string playerName, CharacterData selectedCharacter)
    {
        _name.text = playerName;
        _characterUI.SetVisual(selectedCharacter);
    }

    public void ReadyStateChanged()
    {
        _isReady = !_isReady;
        _readyStateImg.sprite = _isReady ? _readySprite : _notReadySprite;
    }
}
