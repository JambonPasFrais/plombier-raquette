using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldBottom : FieldGroundPart
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<Ball>(out Ball ball))
        {
            ball.Rebound();

            // If it is the second rebound of the ball, then it is point for the hitting player.
            if (ball.ReboundsCount == 2)
            {
                GameManager.Instance.EndOfPoint(ball.LastPlayerToApplyForce.PlayerTeam);
                GameManager.Instance.ScoreManager.AddPoint(ball.LastPlayerToApplyForce.PlayerTeam);
                ball.ResetBall();
            }
            else if (ball.ReboundsCount == 1)
            {
                // This is the first rebound of the ball.
                // If the player hits its own part of the field or serves in the opposite bottom part, it is a fault.
                if (OwnerPlayer == ball.LastPlayerToApplyForce || GameManager.Instance.GameState == GameState.SERVICE)
                {
                    // If it was the first service, the player can proceed to his second service.
                    // Otherwise it is counted as a fault.
                    if (ball.LastPlayerToApplyForce.ServicesCount == 0 && GameManager.Instance.GameState == GameState.SERVICE)
                    {
                        ball.LastPlayerToApplyForce.ServicesCount++;
                        ball.LastPlayerToApplyForce.BallServiceDetectionArea.gameObject.SetActive(true);
                        ball.LastPlayerToApplyForce.ResetLoadedShotVariables();
                        GameManager.Instance.SideManager.SetSides(GameManager.Instance.Controllers, GameManager.Instance.ServiceManager.ServeRight,
                            !GameManager.Instance.ServiceManager.ChangeSides);
                        GameManager.Instance.ServiceManager.EnableLockServiceColliders();
                        ball.ResetBall();
                    }
                    else
                    {
                        Teams otherTeam = (Teams)(Enum.GetValues(typeof(Teams)).GetValue(((int)ball.LastPlayerToApplyForce.PlayerTeam + 1) % 2));
                        GameManager.Instance.EndOfPoint(otherTeam);
                        GameManager.Instance.ScoreManager.AddPoint(otherTeam);
                        ball.ResetBall();
                    }
                }
            }
        }
    }
}
