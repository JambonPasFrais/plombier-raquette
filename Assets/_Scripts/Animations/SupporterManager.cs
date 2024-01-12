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
        AddSupporters();
        
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

    private void AddSupporters()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            for (int j = 0; j < transform.GetChild(i).childCount; j++)
            {
                for (int k = 0; k < transform.GetChild(i).GetChild(j).childCount; k++)
                {
                    for (int l = 0; l < transform.GetChild(i).GetChild(j).GetChild(k).childCount; l++)
                    {
                        _supporters.Add(transform.GetChild(i).GetChild(j).GetChild(k).GetChild(l).gameObject.GetComponent<PlayerUIAnimator>());
                    }
                }
            }
        }
    }
    
    private void RandomlyAnimateSupporters()
    {
        foreach (var supporter in _supporters)
        {
            int randomValue = Random.Range(0, 4);
            if (randomValue >= 2)
            {
                if(randomValue >= 3)
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

    public void AnimateSupportersAfterPoint()
    {
        foreach (var supporter in _supporters)
        {
            if (Random.Range(0, 2) >= 1)
            {
                if (Random.Range(0, 2) >= 1)
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
                if (Random.Range(0, 2) >= 1)
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
