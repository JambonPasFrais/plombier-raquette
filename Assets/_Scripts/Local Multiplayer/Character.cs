using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Instances")] [SerializeField] private CharacterTypes _type;
    [SerializeField] private PlayerAnimator _playerAnimator;
}
