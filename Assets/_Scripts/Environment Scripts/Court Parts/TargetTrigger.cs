using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other != null)
        {
            if(other.GetComponent<CameraController>())
            {
                other.GetComponent<CameraController>().setCanSmash(true);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other != null)
        {
            if (other.GetComponent<CameraController>())
            {
                other.GetComponent<CameraController>().setCanSmash(false);
            }
        }
    }

}
