using UnityEngine;

[CreateAssetMenu(fileName = "ActionParameters", menuName = "ScriptableObjects/ActionParameters", order = 1)]
public class ActionParameters : ScriptableObject
{
    #region PRIVATE FIELDS
    
    [SerializeField] private float _serviceThrowForce;
    [SerializeField] private float _slowTimeScaleFactor;
    [SerializeField] private float _technicalShotMovementLength;

    #endregion

    #region GETTERS

    public float ServiceThrowForce { get { return _serviceThrowForce; } }
    public float SlowTimeScaleFactor { get { return _slowTimeScaleFactor; } }
    public float TechnicalShotMovementLength { get { return _technicalShotMovementLength; } }

    #endregion
}
