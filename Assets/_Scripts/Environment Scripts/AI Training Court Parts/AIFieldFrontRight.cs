using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIFieldFrontRight : AIFieldGroundPart
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<AIBall>(out AIBall ball))
        {
            ball.Rebound();

            // If it is the second rebound of the ball, then it is point for the hitting player.
            if (ball.ReboundsCount == 2)
            {
                // If the scoring player is the agent, it gains reward points.
                if (ball.LastPlayerToApplyForce is AgentController)
                {
                    ((AgentController)ball.LastPlayerToApplyForce).ScoredPoint();
                }

                _trainingManager.EndOfPoint();
                ball.ResetBall();
            }
            else if (ball.ReboundsCount == 1)
            {
                // This is the first rebound of the ball.
                // If the player hits its own part of the field or serve in the opposite right front part while he should have served in the opposite left front part, it is a fault.
                if (OwnerPlayer == ball.LastPlayerToApplyForce || (_trainingManager.GameState == GameState.SERVICE && !_trainingManager.ServeRight))
                {
                    // If it was the first service, the player can proceed to his second service.
                    // Otherwise it is counted as a fault.
                    if (ball.LastPlayerToApplyForce.ServicesCount == 0 && _trainingManager.GameState == GameState.SERVICE)
                    {
                        // If the wrong first service has been realised by the agent, it loses reward points.
                        if (ball.LastPlayerToApplyForce is AgentController)
                        {
                            ((AgentController)ball.LastPlayerToApplyForce).WrongFirstService();
                        }

                        ball.LastPlayerToApplyForce.ServicesCount++;
                        ball.LastPlayerToApplyForce.BallServiceDetectionArea.gameObject.SetActive(true);
                        ball.LastPlayerToApplyForce.ResetLoadedShotVariables();

                        _trainingManager.PlacingPlayers();

                        ball.ResetBall();
                    }
                    else
                    {
                        // If the player that lost the point is the agent, it loses reward points.
                        if (ball.LastPlayerToApplyForce is AgentController)
                        {
                            ((AgentController)ball.LastPlayerToApplyForce).MadeFault();
                        }

                        ball.LastPlayerToApplyForce.ServicesCount = 0;
                        _trainingManager.EndOfPoint();
                        ball.ResetBall();
                    }
                }
                else if (ball.LastPlayerToApplyForce.gameObject.TryGetComponent<AgentController>(out AgentController agent)) 
                {
/*                    if (_trainingManager.GameState == GameState.SERVICE)
                    {
                        agent.ProperService();
                    }*/

                    agent.BallTouchedFieldWithoutProvokingFault();
                }
            }
        }
    }
}
