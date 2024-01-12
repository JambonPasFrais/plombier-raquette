using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGameAnimator : PlayerAnimator
{
    #region COMMUNICATIONS

    public void VictoryAnimation()
    {
        ChangeAnimationState(AnimationNames.Victory_2.ToString());
    }

    public void DefeatAnimation()
    {
        ChangeAnimationState(AnimationNames.Defeat_3.ToString());
    }
    
    #endregion
    
    #region SHOTS
    
    public void StrikeAnimation()
    {
        ChangeAnimationState(Random.Range(0,2) < 1 ? AnimationNames.Strike1.ToString() : AnimationNames.Strike2.ToString());
    }

    public void SmashAnimation()
    {
        ChangeAnimationState(AnimationNames.Smash.ToString());
    }

    public void BackhandAnimation()
    {
        ChangeAnimationState(AnimationNames.Backhand.ToString());
    }

    public void RotateBackhandAnimation()
    {
        ChangeAnimationState(Random.Range(0,2) < 1 ? AnimationNames.RotateBackhand1.ToString() : AnimationNames.RotateBackhand2.ToString());
    }
    
    public void ServiceAnimation()
    {
        ChangeAnimationState(AnimationNames.Service.ToString());
    }
    
    #endregion
    
    #region MOVEMENTS

    public void RunAnimation()
    {
        ChangeAnimationState(AnimationNames.Run.ToString());
    }

    #endregion
}
