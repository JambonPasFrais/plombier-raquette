using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MyPlayerCard : PlayerCard
{
    #region PRIVATE FIELDS

    [SerializeField] private Sprite _clickRdySprite;
    [SerializeField] private Sprite _cancelRdySprite;
    [SerializeField] private TextMeshProUGUI _rdyButtonText;
    [SerializeField] private Image _rdyButtonImage;
    [SerializeField] private Button _readyButton;
    #endregion

    public override void Initialize(string playerName, CharacterData selectedCharacter)
    {
        _name.text = playerName;
        _characterUIPrefab.GetComponent<CharacterUI>().SetVisual(selectedCharacter);
        _readyButton = OnlineManager.Instance.ReadyButton;
        _rdyButtonText = OnlineManager.Instance.ReadyButton.GetComponentInChildren<TextMeshProUGUI>();
        SetButtonToNotReady();
        _isReady = false;
        _readyButton.onClick.AddListener(OnReadyButtonClicked);
    }

    private void OnReadyButtonClicked()
    {
        if (_isReady)
        {
            _isReady = false;
            SetButtonToNotReady();
        }
        else
        {
            _isReady = true;
            SetButtonToReady();
        }

        OnlineManager.Instance.ReadyButtonClicked();
    }

    private void SetButtonToReady()
    {
        _rdyButtonText.text = "cancel";
        //_rdyButtonImage.sprite = _cancelRdySprite;
    }

    private void SetButtonToNotReady()
    {
        _rdyButtonText.text = "ready";
        //_rdyButtonImage.sprite = _clickRdySprite;
    }
}