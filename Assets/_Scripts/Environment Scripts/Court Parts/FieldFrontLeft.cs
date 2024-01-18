using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldFrontLeft : FieldGroundPart
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<Ball>(out Ball ball))
        {
            ball.Rebound();

            // If it is the second rebound of the ball, then it is point for the hitting player.
            if (ball.ReboundsCount == 2)
            {
                if (PhotonNetwork.IsConnected && OwnerPlayer.GetComponent<PhotonView>().IsMine)
                {
                    Teams winningPointTeam = ball.LastPlayerToApplyForce.PlayerTeam;
                    GameManager.Instance.photonView.RPC("EndPoint", RpcTarget.AllViaServer, winningPointTeam);
                }
                else if (!PhotonNetwork.IsConnected)
                {
                    GameManager.Instance.EndOfPoint(ball.LastPlayerToApplyForce.PlayerTeam);
                    GameManager.Instance.ScoreManager.AddPoint(ball.LastPlayerToApplyForce.PlayerTeam);
                    ball.ResetBall();
                }
            }
            else if (ball.ReboundsCount == 1)
            {
                // This is the first rebound of the ball.
                // If the player hits its own part of the field or serve in the opposite left front part while he should have served in the opposite right front part, it is a fault.
                if (OwnerPlayer == ball.LastPlayerToApplyForce || (GameManager.Instance.GameState == GameState.SERVICE && GameManager.Instance.ServiceManager.ServeRight))
                {
                    // If it was the first service, the player can proceed to his second service.
                    // Otherwise it is counted as a fault.
                    if (ball.LastPlayerToApplyForce.ServicesCount == 0 && GameManager.Instance.GameState == GameState.SERVICE)
                    {
                        if (PhotonNetwork.IsConnected && OwnerPlayer.GetComponent<PhotonView>().IsMine)
                        {
                            GameManager.Instance.SideManager.OnlineWrongFirstService(GameManager.Instance.ServiceManager.ServeRight, !GameManager.Instance.ServiceManager.ChangeSides);
                        }
                        else if (!PhotonNetwork.IsConnected)
                        {
                            GameManager.Instance.SideManager.SetSides(GameManager.Instance.Controllers, GameManager.Instance.ServiceManager.ServeRight,
                                !GameManager.Instance.ServiceManager.ChangeSides);
                            GameManager.Instance.ServingPlayerResetAfterWrongFirstService();
                        }
                    }
                    else
                    {
                        if (PhotonNetwork.IsConnected && OwnerPlayer.GetComponent<PhotonView>().IsMine)
                        {
                            Teams winningPointTeam = ball.LastPlayerToApplyForce.PlayerTeam == Teams.TEAM1 ? Teams.TEAM2 : Teams.TEAM1;
                            GameManager.Instance.photonView.RPC("EndPoint", RpcTarget.AllViaServer, winningPointTeam);
                        }
                        else if (!PhotonNetwork.IsConnected)
                        {
                            Teams otherTeam = ball.LastPlayerToApplyForce.PlayerTeam == Teams.TEAM1 ? Teams.TEAM2 : Teams.TEAM1;
                            GameManager.Instance.EndOfPoint(otherTeam);
                            GameManager.Instance.ScoreManager.AddPoint(otherTeam);
                            ball.ResetBall();
                        }
                    }
                }
            }
        }
    }
}
