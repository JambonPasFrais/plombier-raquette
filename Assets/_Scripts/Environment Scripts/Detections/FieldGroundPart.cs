using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldGroundPart : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Ball>(out Ball ball))
        {
            ball.Rebound();
        }
    }
}
