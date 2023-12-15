using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable, CreateAssetMenu(fileName = "TrophyModel", menuName = "ScriptableObjects/Tournament/TrophyModel")]
public class TrophyModel : ScriptableObject
{
    public string TrophyName;
    public GameObject TrophyPrefab;
    public Vector3 Rotation;
    public Vector3 Scale;
    public Vector3 MaxScale;
    public Vector3 MinScale;
}
