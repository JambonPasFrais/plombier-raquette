using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

public class FieldBorderPointsContainer : MonoBehaviour
{
    #region PRIVATE FIELDS

    [SerializeField] private string _playerName;

    [Header("Player Field Limitation Points")]
    [SerializeField] private Transform _frontPointTransform;
    [SerializeField] private Transform _backPointTransform;
    [SerializeField] private Transform _rightPointTransform;
    [SerializeField] private Transform _leftPointTransform;

    #endregion

    #region GETTERS

    public string PlayerName { get { return _playerName; } }
    public Transform FrontPointTransform { get { return _frontPointTransform; } }
    public Transform BackPointTransform { get { return _backPointTransform; } }
    public Transform RightPointTransform { get { return _rightPointTransform; } }
    public Transform LeftPointTransform { get { return _leftPointTransform; } }

    #endregion
}
