using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActionsParameters", menuName = "ScriptableObjects/ActionsParameters", order = 0)] 
public class ActionParameters : ScriptableObject
{
    [Header("Ball Physical Parameters")]
    [SerializeField] private float _hitForceFactor;
    [SerializeField] private float _risingForce;
    [SerializeField] private float _timeBeforeGoingDown;
    [SerializeField] private float _decreasingForcePhaseTime;
    [SerializeField] private float _decreasingForce;
    [SerializeField] private float _dynamicFriction;

    public float HitForceFactor { get { return _hitForceFactor; } }
    public float RisingForce { get { return _risingForce; } }
    public float TimeBeforeGoingDown { get { return _timeBeforeGoingDown; } }
    public float DecreasingForcePhaseTime { get { return _decreasingForcePhaseTime; } }
    public float DecreasingForce { get { return _decreasingForce; } }
    public float DynamicFriction { get { return _dynamicFriction; } }
}
