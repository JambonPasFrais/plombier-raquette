using UnityEngine;

public class PlayerField : MonoBehaviour
{
    #region PRIVATE FIELDS

    [SerializeField] private string _playerName;

    [Header("Player Field Limitation Points")]
    [SerializeField] private Transform _frontPointTransform;
    [SerializeField] private Transform _backPointTransform;
    [SerializeField] private Transform _rightPointTransform;
    [SerializeField] private Transform _leftPointTransform;

    #endregion

    #region GETTERS

    public string PlayerName { get { return _playerName; } }
    public Transform FrontPointTransform { get { return _frontPointTransform; } }
    public Transform BackPointTransform { get { return _backPointTransform; } }
    public Transform RightPointTransform { get { return _rightPointTransform; } }
    public Transform LeftPointTransform { get { return _leftPointTransform; } }

    #endregion

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Ball>(out Ball ball)) 
        {
            ball.Rebound();

/*            if (ball.PointNotFinished)
            {
                string lastPlayerToApplyForceName = GameManager.Instance.GetPlayerName(ball.LastPlayerToApplyForce);

                // Si le joueur a touché sa propre partie du terrain au premier rebond ou si un deuxième rebond a lieu sans interception, alors le point est fini.
                if ((ball.ReboundsCount == 1 && _playerName.Equals(lastPlayerToApplyForceName)) || ball.ReboundsCount == 2)
                {
                    ball.PointNotFinished = false;

                    GameManager.Instance.PointFinished(ball.ReboundsCount, ball.LastPlayerToApplyForce);
                }
            }*/
        }
    }
}
