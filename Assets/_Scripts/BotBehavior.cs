using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BotBehavior : ControllersParent
{
    #region Private Fields

    [Header("Instances")] [SerializeField] private Transform[] _targets;
    [SerializeField] private BallDetection _ballDetection;
    
    [Header("GD")]
    [SerializeField] private float _speed;
    [SerializeField] private float _minimumHitForce;
    [SerializeField] private float _maximumHitForce;

    private Ball _ballInstance;
    private Vector3 _targetPosVector3;

    #endregion
    
    #region Unity Methods

    private void Start()
    {
        _targetPosVector3 = transform.position;
        _ballInstance = GameManager.Instance.BallInstance.GetComponent<Ball>();
    }

    private void Update()
    {
        MoveTowardsBallX();

        /*if (_ballDetection.IsBallInHitZone)
        {
            HitBall();
        }*/
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.TryGetComponent<Ball>(out var ball))
        {
            Vector3 targetPoint = _targets[Random.Range(0, _targets.Length)].position;
            Vector3 direction = Vector3.Project(targetPoint - other.contacts[0].point, Vector3.forward) + Vector3.Project(targetPoint - other.contacts[0].point, Vector3.right);
            ball.ApplyForce(Random.Range(_minimumHitForce, _maximumHitForce), direction.normalized, this);
        }
    }

    #endregion
    
    #region Personalised Methods

    private void HitBall()
    {
        Vector3 targetPoint = _targets[Random.Range(0, _targets.Length)].position;
        Vector3 direction = Vector3.Project(targetPoint - _ballInstance.gameObject.transform.position, Vector3.forward) + Vector3.Project(targetPoint - _ballInstance.gameObject.transform.position, Vector3.right);
        
        _ballInstance.ApplyForce(Random.Range(_minimumHitForce, _maximumHitForce), 
            _ballDetection.GetRisingForceFactor(), 
            direction.normalized, 
            this);
    }

    private void MoveTowardsBallX()
    {
        _targetPosVector3.x = _ballInstance.gameObject.transform.position.x;
        transform.position = Vector3.MoveTowards(transform.position, _targetPosVector3, _speed * Time.deltaTime);
    }
    
    #endregion
}

