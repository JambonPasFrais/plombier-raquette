using UnityEngine;

[CreateAssetMenu(fileName = "ShotParameters", menuName = "ScriptableObjects/BallParameters/ShotParameters")] 
public class ShotParameters : ScriptableObject
{
    #region PRIVATE FIELDS

    [Header("Ball Physical Parameters")] 
    [SerializeField] private HitType _hitType;
    [SerializeField] private float _shotForceFactor;
    [SerializeField] private float _risingForce;
    [SerializeField] private float _timeBeforeGoingDown;
    [SerializeField] private float _decreasingForcePhaseTime;
    [SerializeField] private float _decreasingForce;
    [SerializeField] private float _addedForceInSameDirection;
    [SerializeField] private float _inAirCurvingForce;
    [SerializeField] private float _afterReboudCurvingForce;
    [SerializeField] private float _afterReboudCurvingEffectDuration;
    [SerializeField] private float _forceToDistanceFactor;

    #endregion

    #region GETTERS

    public HitType HitType => _hitType;
    public float ShotForceFactor { get { return _shotForceFactor; } }
    public float RisingForce { get { return _risingForce; } }
    public float TimeBeforeGoingDown { get { return _timeBeforeGoingDown; } }
    public float DecreasingForcePhaseTime { get { return _decreasingForcePhaseTime; } }
    public float DecreasingForce { get { return _decreasingForce; } }
    public float AddedForceInSameDirection { get { return _addedForceInSameDirection; } }
    public float InAirCurvingForce { get { return _inAirCurvingForce; } }
    public float AfterReboudCurvingForce { get { return _afterReboudCurvingForce; } }
    public float AfterReboudCurvingEffectDuration { get { return _afterReboudCurvingEffectDuration; } }
    public float ForceToDistanceFactor { get { return _forceToDistanceFactor; } }

    #endregion
}
