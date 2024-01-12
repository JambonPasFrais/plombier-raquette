using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SupporterManager : MonoBehaviour
{
    [Header("Instances")]
    [SerializeField] private List<PlayerUIAnimator> _supporters;

    [Header("GA")] [SerializeField] private float _timeBetweenAnimationWaves;

    private float _internClock;
    
    private void Start()
    {
        _internClock = 0;
        RandomlyAnimateSupporters();
    }

    private void FixedUpdate()
    {
        _internClock += Time.deltaTime;

        if (_internClock >= _timeBetweenAnimationWaves)
        {
            RandomlyAnimateSupporters();
            _internClock = 0;
        }    
    }

    private void RandomlyAnimateSupporters()
    {
        foreach (var supporter in _supporters)
        {
            float randomValue = Random.Range(0, 1);
            if (randomValue >= 0.5f)
            {
                if(randomValue is >= 0.75f)
                {
                    supporter.CheeringAnimation();
                }
                else
                {
                    supporter.ClappingAnimation();
                }
                
            }
            else
            {
                supporter.IdleAnimation();
            }
        }
    }

    private void AnimateSupportersAfterPoint()
    {
        foreach (var supporter in _supporters)
        {
            if (Random.Range(0, 1) >= 0.5f)
            {
                if (Random.Range(0, 1) >= 0.5f)
                {
                    supporter.DefeatAnimation();
                }
                else
                {
                    supporter.ShakingHeadAnimation();
                }
            }
            else
            {
                if (Random.Range(0, 1) >= 0.5f)
                {
                    supporter.VictoryAnimation();
                }
                else
                {
                    supporter.TakeTheLAnimation();
                }
                
                
            }
        }

        _internClock = 0;
    }
}
