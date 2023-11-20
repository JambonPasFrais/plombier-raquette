using Unity.VisualScripting;
using UnityEngine;

public class BallTester : ControllersParent
{
    #region PUBLIC FIELDS

    public Transform LeftOpposedCorner;
    public Transform RightOpposedCorner;

    #endregion

    #region PRIVATE FIELDS

    [SerializeField] private float _hitForce;

    #endregion

    private void OnCollisionEnter(Collision collision)
    {
        // The wall hit the ball back in a random direction between the two opposite corners of the field.
        if (collision.gameObject.TryGetComponent<Ball>(out Ball ball)) 
        {
            Vector3 targetPoint = LeftOpposedCorner.position + Random.Range(0f, 1f) * (RightOpposedCorner.position - LeftOpposedCorner.position);
            Vector3 direction = Vector3.Project(targetPoint - collision.contacts[0].point, Vector3.forward) + Vector3.Project(targetPoint - collision.contacts[0].point, Vector3.right);
            ball.ApplyForce(_hitForce, 1, direction.normalized, this);
        }
    }
}
