using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCard : MonoBehaviour
{
    #region PUBLIC FIELDS

    public string PlayerName => _name.text;
    public bool IsReady => _isReady;

    #endregion

    #region PROTECTED FIELDS

    [SerializeField] protected TextMeshProUGUI _name;
    [SerializeField] protected GameObject _characterUIPrefab;
    protected bool _isReady;

    #endregion

    public virtual void Initialize(string playerName, CharacterData selectedCharacter) { }
}