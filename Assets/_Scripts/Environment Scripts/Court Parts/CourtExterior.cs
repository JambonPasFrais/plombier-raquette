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
                if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
                {
                    Teams winningPointTeam = ball.LastPlayerToApplyForce.PlayerTeam;
                    GameManager.Instance.photonView.RPC("EndPoint", RpcTarget.AllViaServer, winningPointTeam);
                }
                if (!PhotonNetwork.IsConnected)
                {
                    GameManager.Instance.EndOfPoint();
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
                    ball.LastPlayerToApplyForce.ServicesCount++;
                    ball.LastPlayerToApplyForce.BallServiceDetectionArea.gameObject.SetActive(true);
                    ball.LastPlayerToApplyForce.ResetLoadedShotVariables();
                    
                    if (PhotonNetwork.IsConnected && otherController.gameObject.GetPhotonView().IsMine)
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
                    if (PhotonNetwork.IsConnected && otherController.gameObject.GetPhotonView().IsMine)
                    {
                        Teams winningPointTeam = (Teams)(Enum.GetValues(typeof(Teams)).GetValue(((int)ball.LastPlayerToApplyForce.PlayerTeam + 1) % Enum.GetValues(typeof(Teams)).Length));
                        GameManager.Instance.photonView.RPC("EndPoint", RpcTarget.AllViaServer, winningPointTeam);
                    }
                    if (!PhotonNetwork.IsConnected)
                    {
                        GameManager.Instance.EndOfPoint();
                        Teams otherTeam = (Teams)(Enum.GetValues(typeof(Teams)).GetValue(((int)ball.LastPlayerToApplyForce.PlayerTeam + 1) % Enum.GetValues(typeof(Teams)).Length));
                        GameManager.Instance.ScoreManager.AddPoint(otherTeam);
                    }
                }
                
                ball.ResetBall();
            }
        }
    }
}