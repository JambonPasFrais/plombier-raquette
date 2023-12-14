using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CourtExterior : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Ball ball))
        {
            ball.Rebound();

            // If it is the second rebound of the ball, then it is point for the hitting player.
            if (ball.ReboundsCount == 2)
            {
                GameManager.Instance.EndOfPoint();
                GameManager.Instance.ScoreManager.AddPoint(ball.LastPlayerToApplyForce.PlayerTeam);
                ball.ResetBall();
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
                    
                    if (PhotonNetwork.IsConnected)
                    {
                        GameManager.Instance.SideManager.SetSideOnline(GameManager.Instance.ServiceManager.ServeRight,
             !GameManager.Instance.ServiceManager.ChangeSides);
                    }
                    else
                    {
                        GameManager.Instance.SideManager.SetSides(GameManager.Instance.Controllers, GameManager.Instance.ServiceManager.ServeRight,
                        !GameManager.Instance.ServiceManager.ChangeSides);
                    }
                    
                    GameManager.Instance.ServiceManager.EnableLockServiceColliders();
                }
                else
                {
                    GameManager.Instance.EndOfPoint();
                    Teams otherTeam = ball.LastPlayerToApplyForce.PlayerTeam == Teams.TEAM1 ? Teams.TEAM2 : Teams.TEAM1;
                    //Teams otherTeam = (Teams)(Enum.GetValues(typeof(Teams)).GetValue(((int)ball.LastPlayerToApplyForce.PlayerTeam + 1) % 2));
                    GameManager.Instance.ScoreManager.AddPoint(otherTeam);
                }
                
                ball.ResetBall();
            }
        }
    }
}