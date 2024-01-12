using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallServiceDetection : MonoBehaviour
{
	[SerializeField] private ControllersParent _player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<Ball>(out Ball ball) && _player.PlayerState == PlayerStates.SERVE)  
        {
            if (PhotonNetwork.IsConnected && _player.gameObject.GetPhotonView().IsMine)
            {
                GameManager.Instance.photonView.RPC("ResetBall", RpcTarget.AllViaServer);
            }
            else if (!PhotonNetwork.IsConnected)
            {
                ball.ResetBall();
            }
        }
    }
}
