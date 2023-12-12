using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementServiceLockingCollider : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<AgentController>(out AgentController agent))
        {
            agent.TouchedForbiddenCollider();
        }
    }
}
