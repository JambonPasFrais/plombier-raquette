using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldFrontLeft : FieldGroundPart
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
                // If the player hits its own part of the field or serve in the opposite left front part while he should have served in the opposite right front part, it is a fault.
                if (_ownerPlayer == ball.LastPlayerToApplyForce || (GameManager.Instance.GameState == GameState.SERVICE && GameManager.Instance.ServiceManager.ServeRight))
                {
                    GameManager.Instance.EndOfPoint();
                    Teams otherTeam = (Teams)(Enum.GetValues(typeof(Teams)).GetValue(((int)ball.LastPlayerToApplyForce.PlayerTeam + 1) % Enum.GetValues(typeof(Teams)).Length));
                    GameManager.Instance.ScoreManager.AddPoint(otherTeam);
                    ball.ResetBall();
                }
            }
        }
    }
}
