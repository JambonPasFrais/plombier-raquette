using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator _animator;
    private string _currentState;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    private void ChangeAnimationState(string newState)
    {
        if (_currentState != null && _currentState == newState)
            return;
        
        _animator.Play(newState);
    }

    public void IdleAnimation()
    {
        ChangeAnimationState(PlayerAnimations.Idle.ToString());
    }
    
    public void StrikeAnimation()
    {
        ChangeAnimationState(Random.Range(0,2) < 1 ? PlayerAnimations.Strike1.ToString() : PlayerAnimations.Strike2.ToString());
    }

    public void SmashAnimation()
    {
        ChangeAnimationState(PlayerAnimations.Smash.ToString());
    }

    public void BackhandAnimation()
    {
        ChangeAnimationState(PlayerAnimations.Backhand.ToString());
    }

    public void RotateBackhandAnimation()
    {
        ChangeAnimationState(Random.Range(0,2) < 1 ? PlayerAnimations.RotateBackhand1.ToString() : PlayerAnimations.RotateBackhand2.ToString());
    }
    
    public void WalkAnimation()
    {
        ChangeAnimationState(PlayerAnimations.Walk.ToString());
    }

    public void RunAnimation()
    {
        ChangeAnimationState(PlayerAnimations.Run.ToString());
    }
}
