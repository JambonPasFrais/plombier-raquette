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
    
    #region COMMUNICATIONS

    public void VictoryAnimation()
    {
        int random = Random.Range(0, 3);
        switch (random)
        {
            case 0:
                ChangeAnimationState(PlayerAnimations.Victory_1.ToString());
                break;
            case 1:
                ChangeAnimationState(PlayerAnimations.Victory_2.ToString());
                break;
            case 2:
                ChangeAnimationState(PlayerAnimations.Victory_3.ToString());
                break;
        }
    }

    public void DefeatAnimation()
    {
        int random = Random.Range(0, 3);
        switch (random)
        {
            case 0:
                ChangeAnimationState(PlayerAnimations.Defeat_1.ToString());
                break;
            case 1:
                ChangeAnimationState(PlayerAnimations.Defeat_2.ToString());
                break;
            case 2:
                ChangeAnimationState(PlayerAnimations.Defeat_3.ToString());
                break;
        }
    }
    
    #endregion
    
    #region SHOTS
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
    
    public void ServiceAnimation()
    {
        ChangeAnimationState(PlayerAnimations.Service.ToString());
    }
    #endregion
    
    #region MOVEMENTS
    public void MoveFrontAnimation()
    {
        ChangeAnimationState(PlayerAnimations.ForwardMove.ToString());
    }

    public void RunAnimation()
    {
        ChangeAnimationState(PlayerAnimations.Run.ToString());
    }

    public void MoveBackwardAnimation()
    {
        ChangeAnimationState(PlayerAnimations.BackwardMove.ToString());
    }

    public void MoveLeftAnimation()
    {
        ChangeAnimationState(PlayerAnimations.LeftStrafe.ToString());
    }

    public void MoveRightAnimation()
    {
        ChangeAnimationState(PlayerAnimations.RightStrafe.ToString());
    }
    
    #endregion
}
