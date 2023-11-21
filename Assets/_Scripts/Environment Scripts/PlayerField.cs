using UnityEngine;

public class PlayerField : MonoBehaviour
{
    [SerializeField] private string _playerName;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Ball>(out Ball ball)) 
        {
            ball.Rebound();

            if (ball.PointNotFinished)
            {
                string lastPlayerToApplyForceName = GameManager.Instance.GetPlayerName(ball.LastPlayerToApplyForce);

                // Si le joueur a touché sa propre partie du terrain au premier rebond ou si un deuxième rebond a lieu sans interception, alors le point est fini.
                if ((ball.ReboundsCount == 1 && _playerName.Equals(lastPlayerToApplyForceName)) || ball.ReboundsCount == 2)
                {
                    ball.PointNotFinished = false;

                    GameManager.Instance.PointFinished(ball.ReboundsCount, ball.LastPlayerToApplyForce);
                }
            }
        }
    }
}
