using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator _animator;
    private string _currentState;

    // Start is called before the first frame update
    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    protected void ChangeAnimationState(string newState)
    {
        if (_currentState != null && _currentState == newState)
            return;
        
        _animator.Play(newState);
    }
    
    public void IdleAnimation()
    {
        ChangeAnimationState(AnimationNames.Idle.ToString());
    }
}
