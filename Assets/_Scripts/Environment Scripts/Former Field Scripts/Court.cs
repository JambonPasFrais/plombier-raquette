using UnityEngine;

public class Court : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Ball>(out Ball ball))
        {
            ball.Rebound();

/*            if (ball.PointNotFinished)
            {
                // Si le joueur a touch� l'ext�rieur du terrain au premier rebond ou si un deuxi�me rebond a lieu sans interception, alors le point est fini.
                if (ball.ReboundsCount == 1 || ball.ReboundsCount == 2)
                {
                    ball.PointNotFinished = false;

                    GameManager.Instance.PointFinished(ball.ReboundsCount, ball.LastPlayerToApplyForce);
                }
            }*/
        }
    }
}
