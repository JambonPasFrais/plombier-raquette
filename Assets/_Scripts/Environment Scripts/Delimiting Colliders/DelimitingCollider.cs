using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelimitingCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<AgentController>(out AgentController agent))
        {
            agent.TouchedForbiddenCollider();
        }
    }
}
