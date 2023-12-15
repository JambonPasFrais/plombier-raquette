using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmashTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Ball>())
        {
            Ball ball = other.GetComponent<Ball>();
            ball.SetCanSmash(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Ball>())
        {
            Ball ball = other.GetComponent<Ball>();
            ball.SetCanSmash(false);
        }
    }
}
