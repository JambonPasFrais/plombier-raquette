using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUIAnimator : PlayerAnimator
{
    #region "Neutral"

    public void ClappingAnimation()
    {
        ChangeAnimationState(AnimationNames.Clapping.ToString());
    }

    public void CheeringAnimation()
    {
        ChangeAnimationState(AnimationNames.Cheering.ToString());
    }

    #endregion
    
    #region Positive

    public void TakeTheLAnimation()
    {
        ChangeAnimationState(AnimationNames.TakeTheL.ToString());
    }
    
    public void CrowdVictoryAnimation()
    {
        int random = Random.Range(0, 4);
        switch (random)
        {
            case 0:
                ChangeAnimationState(AnimationNames.Victory_1.ToString());
                break;
            case 1:
                ChangeAnimationState(AnimationNames.Victory_2.ToString());
                break;
            case 2:
                ChangeAnimationState(AnimationNames.Victory_3.ToString());
                break;
        }
    }
    
    public void CharacterVictoryAnimation()
    {
        int random = Random.Range(0, 4);
        switch (random)
        {
            case 0:
                ChangeAnimationState(AnimationNames.Victory_1.ToString());
                break;
            case 1:
                ChangeAnimationState(AnimationNames.Victory_2.ToString());
                break;
            case 2:
                ChangeAnimationState(AnimationNames.Victory_3.ToString());
                break;
            case 3:
				ChangeAnimationState(AnimationNames.TakeTheL.ToString());
				break;
        }
    }

    #endregion
    
    #region Negative

    public void ShakingHeadAnimation()
    {
        ChangeAnimationState(AnimationNames.ShakingHeadNo.ToString());
    }
    
    public void CrowdDefeatAnimation()
    {
        int random = Random.Range(0, 4);
        switch (random)
        {
            case 0:
                ChangeAnimationState(AnimationNames.Defeat_1.ToString());
                break;
            case 1:
                ChangeAnimationState(AnimationNames.Defeat_2.ToString());
                break;
            case 2:
                ChangeAnimationState(AnimationNames.Defeat_3.ToString());
                break;
		}
    }
    
    public void CharacterDefeatAnimation()
    {
        int random = Random.Range(0, 4);
        switch (random)
        {
            case 0:
                ChangeAnimationState(AnimationNames.Defeat_1.ToString());
                break;
            case 1:
                ChangeAnimationState(AnimationNames.Defeat_2.ToString());
                break;
            case 2:
                ChangeAnimationState(AnimationNames.Defeat_3.ToString());
                break;
            case 3:
				ChangeAnimationState(AnimationNames.ShakingHeadNo.ToString());
                break;
		}
    }
    
    #endregion
}
