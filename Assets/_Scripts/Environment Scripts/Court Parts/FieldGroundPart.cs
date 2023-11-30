using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldGroundPart : MonoBehaviour
{
    [SerializeField] protected ControllersParent _ownerPlayer;

    public ControllersParent OwnerPlayer { set { _ownerPlayer = value; } }
}
