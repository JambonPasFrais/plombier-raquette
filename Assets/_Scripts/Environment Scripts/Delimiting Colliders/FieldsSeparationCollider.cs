using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldsSeparationCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<AgentController>(out AgentController agent))
        {
            agent.TouchedForbiddenCollider();
        }
    }
}
