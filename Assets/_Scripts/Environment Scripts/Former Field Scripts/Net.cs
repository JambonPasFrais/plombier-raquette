using UnityEngine;

public class Net : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
/*        if (collision.gameObject.TryGetComponent<Ball>(out Ball ball) && ball.PointNotFinished)
        {
            if (ball.PointNotFinished)
            {
                // Si le joueur a touch� le filet au premier rebond ou si un deuxi�me rebond a lieu sans interception, alors le point est fini.
                if (ball.ReboundsCount == 1 || ball.ReboundsCount == 2)
                {
                    ball.PointNotFinished = false;

                    GameManager.Instance.PointFinished(ball.ReboundsCount, ball.LastPlayerToApplyForce);
                }
            }
        }*/
    }
}
