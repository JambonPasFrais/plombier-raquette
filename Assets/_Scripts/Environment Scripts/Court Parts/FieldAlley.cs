using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldAlley : FieldGroundPart
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
            }
            // If the player hits an alley on the first rebound, it is fault.
            else if (ball.ReboundsCount == 1)
            {
                // If it was the first service, the player can proceed to his second service.
                // Otherwise it is counted as a fault.
                if (ball.LastPlayerToApplyForce.ServicesCount == 0)
                {
                    ball.LastPlayerToApplyForce.ServicesCount++;
                    ball.ResetBall();
                }
                else
                {
                    ball.LastPlayerToApplyForce.ServicesCount = 0;
                    GameManager.Instance.EndOfPoint();
                    Teams otherTeam = (Teams)(Enum.GetValues(typeof(Teams)).GetValue(((int)ball.LastPlayerToApplyForce.PlayerTeam + 1) % Enum.GetValues(typeof(Teams)).Length));
                    GameManager.Instance.ScoreManager.AddPoint(otherTeam);
                    ball.ResetBall();
                }
            }
        }
    }
}
