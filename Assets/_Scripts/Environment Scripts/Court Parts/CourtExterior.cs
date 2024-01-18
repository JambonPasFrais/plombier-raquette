using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CourtExterior : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Ball ball))
        {
            ball.Rebound();

            // If it is the second rebound of the ball, then it is point for the hitting player.
            if (ball.ReboundsCount == 2)
            {
                if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
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
            // If the player hits a part of the exterior court on the first rebound, it is fault.
            else if (ball.ReboundsCount == 1)
            {
                ControllersParent otherController = GameManager.Instance.Controllers[0] == ball.LastPlayerToApplyForce ?
                    GameManager.Instance.Controllers[1] : GameManager.Instance.Controllers[0];

                // If it was the first service, the player can proceed to his second service.
                // Otherwise it is counted as a fault.
                if (ball.LastPlayerToApplyForce.ServicesCount == 0 && GameManager.Instance.GameState == GameState.SERVICE)
                {
                    if (PhotonNetwork.IsConnected && otherController.gameObject.GetPhotonView().IsMine)
                    {
                        GameManager.Instance.SideManager.OnlineWrongFirstService(GameManager.Instance.ServiceManager.ServeRight, !GameManager.Instance.ServiceManager.ChangeSides);
                    }
                    else if (!PhotonNetwork.IsConnected)
                    {
                        GameManager.Instance.SideManager.SetSides(GameManager.Instance.Controllers, GameManager.Instance.ServiceManager.ServeRight, !GameManager.Instance.ServiceManager.ChangeSides);
                        GameManager.Instance.ServingPlayerResetAfterWrongFirstService();
                    }
                }
                else
                {
                    if (PhotonNetwork.IsConnected && otherController.gameObject.GetPhotonView().IsMine)
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