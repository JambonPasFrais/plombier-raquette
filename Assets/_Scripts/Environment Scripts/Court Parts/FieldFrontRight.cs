using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldFrontRight : FieldGroundPart
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Ball ball))
        {
            ball.Rebound();

            // If it is the second rebound of the ball, then it is point for the hitting player.
            if (ball.ReboundsCount == 2)
            {
                if (PhotonNetwork.IsConnected && _ownerPlayer.GetComponent<PhotonView>().IsMine)
                {
                    GameManager.Instance.photonView.RPC("EndPoint", RpcTarget.AllViaServer, false);
                }
                if (!PhotonNetwork.IsConnected)
                {
                    GameManager.Instance.EndOfPoint();
                    GameManager.Instance.ScoreManager.AddPoint(ball.LastPlayerToApplyForce.PlayerTeam);
                    ball.ResetBall();
                }
            }
            else if(ball.ReboundsCount == 1)
            {
                // This is the first rebound of the ball.
                // If the player hits its own part of the field or serve in the opposite right front part while he should have served in the opposite left front part, it is a fault.
                if (OwnerPlayer == ball.LastPlayerToApplyForce || (GameManager.Instance.GameState == GameState.SERVICE && !GameManager.Instance.ServiceManager.ServeRight)) 
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
                        if (PhotonNetwork.IsConnected && _ownerPlayer.GetComponent<PhotonView>().IsMine)
                        {
                            GameManager.Instance.photonView.RPC("EndPoint", RpcTarget.AllViaServer, true);
                        }
                        if (!PhotonNetwork.IsConnected)
                        {
                            GameManager.Instance.EndOfPoint();
                            Teams otherTeam = (Teams)(Enum.GetValues(typeof(Teams)).GetValue(((int)ball.LastPlayerToApplyForce.PlayerTeam + 1) % 2));
                            GameManager.Instance.ScoreManager.AddPoint(otherTeam);
                        }
                    }
                    
                    ball.ResetBall();
                }
            }
        }
    }
}
