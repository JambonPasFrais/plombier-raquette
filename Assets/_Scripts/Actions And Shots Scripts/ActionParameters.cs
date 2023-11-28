using UnityEngine;

[CreateAssetMenu(fileName = "ActionParameters", menuName = "ScriptableObjects/BallParameters/ActionParameters")]
public class ActionParameters : ScriptableObject
{
    #region PRIVATE FIELDS
    
    [SerializeField] private float _slowTimeScaleFactor;
    [SerializeField] private float _technicalShotMovementLength;

    #endregion

    #region GETTERS

    public float SlowTimeScaleFactor { get { return _slowTimeScaleFactor; } }
    public float TechnicalShotMovementLength { get { return _technicalShotMovementLength; } }

    #endregion
}
