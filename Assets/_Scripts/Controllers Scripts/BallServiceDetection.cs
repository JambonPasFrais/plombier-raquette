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
            ball.ResetBall();
        }
        else if (other.gameObject.TryGetComponent<AIBall>(out AIBall aiBall) && _player.PlayerState == PlayerStates.SERVE)
        {
            aiBall.ResetBall();
        }
    }
}
