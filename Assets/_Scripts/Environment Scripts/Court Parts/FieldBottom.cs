using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldBottom : FieldGroundPart
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
            else if (ball.ReboundsCount == 1)
            {
                // This is the first rebound of the ball.
                // If the player hits its own part of the field or serves in the opposite bottom part, it is a fault.
                if (_ownerPlayer == ball.LastPlayerToApplyForce || GameManager.Instance.GameState == GameState.SERVICE )
                {
                    // If it was the first service, the player can proceed to his second service.
                    // Otherwise it is counted as a fault.
                    if (ball.LastPlayerToApplyForce.ServicesCount == 0 && GameManager.Instance.GameState == GameState.SERVICE)
                    {
                        ball.LastPlayerToApplyForce.ServicesCount++;
                        ball.LastPlayerToApplyForce.BallServiceDetectionArea.gameObject.SetActive(true);
                        GameManager.Instance.SideManager.SetSidesInSimpleMatch(GameManager.Instance.Controllers, GameManager.Instance.ServiceManager.ServeRight,
                            !GameManager.Instance.ServiceManager.ChangeSides);
                        GameManager.Instance.ServiceManager.EnableLockServiceColliders();
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
}
