using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CourtExterior : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Ball>(out Ball ball))
        {
            ball.Rebound();

            // If it is the second rebound of the ball, then it is point for the hitting player.
            if (ball.ReboundsCount == 2)
            {
                GameManager.Instance.EndOfPoint();
                GameManager.Instance.ScoreManager.AddPoint(ball.LastPlayerToApplyForce.PlayerTeam);
                ball.ResetBall();

                // If the scoring player is the agent, it gains reward points.
                if (ball.LastPlayerToApplyForce is AgentController)
                {
                    ((AgentController)ball.LastPlayerToApplyForce).ScoredPoint();
                }
            }
            // If the player hits a part of the exterior court on the first rebound, it is fault.
            else if (ball.ReboundsCount == 1)
            {
                // If it was the first service, the player can proceed to his second service.
                // Otherwise it is counted as a fault.
                if (ball.LastPlayerToApplyForce.ServicesCount == 0 && GameManager.Instance.GameState == GameState.SERVICE)
                {
                    ball.LastPlayerToApplyForce.ServicesCount++;
                    ball.LastPlayerToApplyForce.BallServiceDetectionArea.gameObject.SetActive(true);
                    ball.LastPlayerToApplyForce.ResetLoadedShotVariables();
                    GameManager.Instance.SideManager.SetSidesInSimpleMatch(GameManager.Instance.Controllers, GameManager.Instance.ServiceManager.ServeRight,
                        !GameManager.Instance.ServiceManager.ChangeSides);
                    GameManager.Instance.ServiceManager.EnableLockServiceColliders();
                    ball.ResetBall();

                    // If the wrong first service has been realised by the agent, it loses reward points.
                    if (ball.LastPlayerToApplyForce is AgentController)
                    {
                        ((AgentController)ball.LastPlayerToApplyForce).WrongFirstService();
                    }
                }
                else
                {
                    GameManager.Instance.EndOfPoint();
                    Teams otherTeam = (Teams)(Enum.GetValues(typeof(Teams)).GetValue(((int)ball.LastPlayerToApplyForce.PlayerTeam + 1) % Enum.GetValues(typeof(Teams)).Length));
                    GameManager.Instance.ScoreManager.AddPoint(otherTeam);
                    ball.ResetBall();

                    // If the player that lost the point is the agent, it loses reward points.
                    if (ball.LastPlayerToApplyForce is AgentController)
                    {
                        ((AgentController)ball.LastPlayerToApplyForce).LostPoint();
                    }
                }
            }
        }
    }
}
