using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Serialization;

[RequireComponent(typeof(PlayerAnimator))]
public class ControllersParent : MonoBehaviour
{
    #region PUBLIC FIELDS

    public bool IsServing;
    public PlayerStates PlayerState;
    public int ServicesCount=0;
    public Teams PlayerTeam;
    public bool IsInOriginalSide;

    #endregion

    #region PROTECTED FIELDS
    
    [SerializeField] protected GameObject _chargingShotGo;
    [SerializeField] protected ActionParameters _actionParameters;
    [SerializeField] protected BallServiceDetection _ballServiceDetectionArea;
    [SerializeField] protected Transform _serviceBallInitializationPoint;
    [SerializeField] protected float _minimumShotForce;
    [SerializeField] protected float _maximumShotForce;

    [Header("Force Clamping")]
    [SerializeField] protected float _maximumDistanceToNet;
    [SerializeField] protected float _forceMinimumClampFactor;
    [SerializeField] protected float _forceMaximumClampFactor;

    protected float _hitKeyPressedTime;
    protected bool _isCharging;

    #region Animations
    // TODO : comment serializeField and Header after tests
    [Header("Tests")]
    [SerializeField] protected PlayerAnimator _playerAnimator;
    [SerializeField] protected bool _isShooting;
    [SerializeField] protected bool _isSmashing;
    [SerializeField] protected bool _isCommunicating;

    #endregion

    #endregion
    
    #region UNITY FUNCTIONS

    private void Awake()
    {
        _playerAnimator = GetComponent<PlayerAnimator>();
    }
    
    #endregion
    
    #region GETTERS
    
    public ActionParameters ActionParameters { get { return _actionParameters; } }
    public BallServiceDetection BallServiceDetectionArea { get { return _ballServiceDetectionArea; } }
    public Transform ServiceBallInitializationPoint { get { return _serviceBallInitializationPoint; } }
    public float MaximumShotForce { get { return _maximumShotForce; } }
    
    public PlayerAnimator PlayerAnimator => _playerAnimator;

    #endregion

    /// <summary>
    /// Calculates the distance between the player and the net, clamped bewteen 0m and the maximum distance to the net which corresponds the bottom line of the field.
    /// </summary>
    /// <returns></returns>
    private float CalculateClampedDistanceToNet()
    {
        float actualDistanceToNet = Vector3.Project(GameManager.Instance.Net.transform.position - gameObject.transform.position, Vector3.forward).magnitude;
        float clampedDistanceToNet = Mathf.Clamp(actualDistanceToNet, 0f, _maximumDistanceToNet);
        return clampedDistanceToNet;
    }

    /// <summary>
    /// Calculates the extreme shooting direction according to 
    /// </summary>
    /// <param name="rightSideIsTargeted"></param>
    /// <param name="forceToDistanceFactor"></param>
    /// <param name="actualForce"></param>
    /// <param name="forwardVector"></param>
    /// <returns></returns>
    private Vector3 CalculateExtremeShootingDirection(bool rightSideIsTargeted, float forceToDistanceFactor, float actualForce, Vector3 forwardVector)
    {
        float distanceToFirstReboundPosition = forceToDistanceFactor * actualForce;
        //Debug.Log($"Predicted distance travelled until first rebound : {distanceToFirstReboundPosition}");
        Vector3 rightVector = GameManager.Instance.CameraManager.GetActiveCameraTransformBySide(IsInOriginalSide).right;
        float maximumLateralDistance;

        if (rightSideIsTargeted)
        {
            maximumLateralDistance = Mathf.Abs(GameManager.Instance.FaultLinesXByTeam[PlayerTeam][1] - transform.position.x);
        }
        else
        {
            maximumLateralDistance = Mathf.Abs(GameManager.Instance.FaultLinesXByTeam[PlayerTeam][0] - transform.position.x);
        }

        float maximumForwardDistance = Mathf.Sqrt(Mathf.Pow(distanceToFirstReboundPosition, 2) + Mathf.Pow(maximumLateralDistance, 2));
        return (rightVector.normalized * ((rightSideIsTargeted ? 1 : -1) * maximumLateralDistance) + forwardVector.normalized * maximumForwardDistance).normalized;
    }

    /// <summary>
    /// Calculates the actual force to apply on the ball, depending to the distance bewteen the player and the net.
    /// The more close the player is to the net, the less force is applied to the ball for a same loading time.
    /// </summary>
    /// <param name="hitForce"></param>
    /// <returns></returns>
    protected float CalculateActualForce(float hitForce)
    {
        float clampedDistanceToNet = CalculateClampedDistanceToNet();
        float forceFactor = (clampedDistanceToNet / _maximumDistanceToNet) * (_forceMaximumClampFactor - _forceMinimumClampFactor) + _forceMinimumClampFactor;
        return forceFactor * hitForce;
    }

    /// <summary>
    /// Calculates the actual shooting direction according to the wanted shooting direction, the actual force applied by the player on the ball and the distance between
    /// the player and the net.
    /// This "actual shooting direction" system has been created to make the game more casual by avoiding direct faults as much as possible.
    /// </summary>
    /// <param name="wantedDirection"></param>
    /// <param name="actualForce"></param>
    /// <param name="forceToDistanceFactor"></param>
    /// <returns></returns>
    public Vector3 CalculateActualShootingDirection(Vector3 wantedDirection, float forceToDistanceFactor, float actualForce)
    {
        float rotationSign = Mathf.Sign(Vector3.Dot(wantedDirection, Vector3.Project(GameManager.Instance.CameraManager.GetActiveCameraTransformBySide(IsInOriginalSide).right, Vector3.right)));
        Vector3 forwardVector = Vector3.Project(GameManager.Instance.CameraManager.GetActiveCameraTransformBySide(IsInOriginalSide).forward, Vector3.forward);
        Vector3 extremeShootingDirection = CalculateExtremeShootingDirection(rotationSign > 0 ? true : false, forceToDistanceFactor, actualForce, forwardVector);

        // Idea to fix the prob : create a vector3 wantedDirectionWithNulY = (wantedDir.x, 0, wantedDir.z);

        Vector3 wantedDirectionHorizontal = new Vector3(wantedDirection.x, 0, wantedDirection.z);
        
        if (Vector3.Angle(forwardVector, wantedDirectionHorizontal) > Vector3.Angle(forwardVector, extremeShootingDirection))
        {
            return new Vector3(extremeShootingDirection.x, wantedDirection.y, extremeShootingDirection.z);
        }
        else
        {
            return wantedDirection;
        }
    }

    public void ResetAtService()
    {
        PlayerState = PlayerStates.IDLE;
        ServicesCount = 0;
        ResetLoadedShotVariables();

        if (_ballServiceDetectionArea != null)
            _ballServiceDetectionArea.gameObject.SetActive(true);
    }

    public void ResetLoadedShotVariables()
    {
        _hitKeyPressedTime = 0;
        _isCharging = false;
    }

    protected void ThrowBall()
    {
        if (GameManager.Instance.Controllers[GameManager.Instance.ServerIndex] != this)
            return;
        
        if (GameManager.Instance.Controllers[GameManager.Instance.ServerIndex].PlayerState != PlayerStates.SERVE)
            return;
        
        if (!GameManager.Instance.Controllers[GameManager.Instance.ServerIndex].IsServing)
            return;
        
        if (GameManager.Instance.GameState != GameState.SERVICE)
            return;
        
        Rigidbody ballRigidBody = GameManager.Instance.BallInstance.GetComponent<Rigidbody>();

        if (!ballRigidBody.isKinematic)
            return;
        
        ballRigidBody.isKinematic = false;
        ballRigidBody.AddForce(Vector3.up * GameManager.Instance.Controllers[GameManager.Instance.ServerIndex].ActionParameters.ServiceThrowForce);

        
        /*if (GameManager.Instance.Controllers[GameManager.Instance.ServerIndex].PlayerState == PlayerStates.SERVE && GameManager.Instance.Controllers[GameManager.Instance.ServerIndex].IsServing && GameManager.Instance.GameState == GameState.SERVICE && ballRigidBody.isKinematic)
        {
            ballRigidBody.isKinematic = false;
            ballRigidBody.AddForce(Vector3.up * GameManager.Instance.Controllers[GameManager.Instance.ServerIndex].ActionParameters.ServiceThrowForce);
        }*/
    }
    
    #region ANIMATIONS

    protected void EndALlAnims()
    {
        ShootingAnimationEnd();
        SmashAnimationEnd();
        ComAnimationEnd();
    }
    
    public void ShootingAnimationEnd()
    {
        _isShooting = false;
    }

    public void SmashAnimationEnd()
    {
        _isSmashing = false;
    }

    public void LaunchCelebration()
    {
        EndALlAnims();
        
        _isCommunicating = true;
        _playerAnimator.VictoryAnimation();
    }

    public void LaunchDepreciation()
    {
        EndALlAnims();

        _isCommunicating = true;
        _playerAnimator.DefeatAnimation();
    }
    
    public void ComAnimationEnd()
    {
        _isCommunicating = false;
        // Explanation : since "Victory" and "Defeat" animations are loops, when they are finished, we start another anim
        //_playerAnimator.IdleAnimation();
    }
    
    #endregion
}
