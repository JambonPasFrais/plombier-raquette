using UnityEngine;

[CreateAssetMenu(fileName = "ActionParameters", menuName = "ScriptableObjects/ActionParameters", order = 1)]
public class ActionParameters : ScriptableObject
{
    #region PRIVATE FIELDS
    
    [SerializeField] private float _slowTimeScaleFactor;

    #endregion

    #region GETTERS

    public float SlowTimeScaleFactor { get { return _slowTimeScaleFactor; } }

    #endregion
}
