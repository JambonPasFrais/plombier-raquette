using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour
{
    public void OnPunch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            transform.DOComplete();
            transform.DOPunchScale(Vector3.one * .1f, .2f);
        }
    }
}
