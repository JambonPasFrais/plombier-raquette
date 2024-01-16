using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControllersParent : Agent
{
    #region PUBLIC FIELDS

    public bool IsServing;
    public PlayerStates PlayerState;
    public int ServicesCount;
    public bool IsOnOriginalSide;

    #endregion

    #region PROTECTED FIELDS

    [SerializeField] protected AgentTrainingManager _trainingManager;
    [SerializeField] protected ActionParameters _actionParameters;
    [SerializeField] protected Teams _playerTeam;
    [SerializeField] protected BallServiceDetection _ballServiceDetectionArea;
    [SerializeField] protected Transform _serviceBallInitializationPoint;
    [SerializeField] protected float _minimumShotForce;
    [SerializeField] protected float _maximumShotForce;
    [SerializeField] protected float _extremeShootingDirectionLateralDistanceMargin;

    [Header("Force Clamping")]
    [SerializeField] protected float _maximumDistanceToNet;
    [SerializeField] protected float _forceMinimumClampFactor;
    [SerializeField] protected float _forceMaximumClampFactor;

    protected float _hitKeyPressedTime;
    protected bool _isCharging;

    #endregion

    #region GETTERS

    public Teams PlayerTeam { get { return _playerTeam; } }
    public ActionParameters ActionParameters { get { return _actionParameters; } }
    public BallServiceDetection BallServiceDetectionArea { get { return _ballServiceDetectionArea; } }
    public Transform ServiceBallInitializationPoint { get { return _serviceBallInitializationPoint; } }

    #endregion

    public void ServiceThrow(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (this is AgentController)
            {
                ((AgentController)this).ActionIndex = 1;
            }
            else
            {
                ThrowBall();
            }
        }
    }

    protected void ThrowBall()
    {
        Rigidbody ballRigidBody = _trainingManager.BallInstance.GetComponent<Rigidbody>();

        if (_trainingManager.GameState == GameState.SERVICE && ballRigidBody.isKinematic)
        {
            ballRigidBody.isKinematic = false;
            ballRigidBody.AddForce(Vector3.up * _actionParameters.ServiceThrowForce);
        }
    }

    /// <summary>
    /// Calculates the distance between the player and the net, clamped bewteen 0m and the maximum distance to the net which corresponds the bottom line of the field.
    /// </summary>
    /// <returns></returns>
    private float CaluclateClampedDistanceToNet()
    {
        float actualDistanceToNet = Vector3.Project(_trainingManager.Net.transform.position - gameObject.transform.position, Vector3.forward).magnitude;
        float clampedDistanceToNet = Mathf.Clamp(actualDistanceToNet, 0f, _maximumDistanceToNet);
        return clampedDistanceToNet;
    }

    /// <summary>
    /// Calculates the extreme shooting direction according to 
    /// </summary>
    /// <param name="rightSideIsTargeted"></param>
    /// <param name="forceToDistanceFactor"></param>
    /// <param name="actualforce"></param>
    /// <returns></returns>
    private Vector3 CalculateExtremeShootingDirection(bool rightSideIsTargeted, float forceToDistanceFactor, float actualforce)
    {
        float distanceToFirstReboundPosition = forceToDistanceFactor * actualforce;
        Vector3 forwardVector = Vector3.Project(_trainingManager.CameraTransform.forward, Vector3.forward);
        Vector3 rightVector = _trainingManager.CameraTransform.right;
        float maximumLateralDistance;

        if (rightSideIsTargeted)
        {
            maximumLateralDistance = Mathf.Abs(_trainingManager.FaultLinesXByTeam[this.PlayerTeam][1] - transform.position.x) - _extremeShootingDirectionLateralDistanceMargin;
        }
        else
        {
            maximumLateralDistance = Mathf.Abs(_trainingManager.FaultLinesXByTeam[this.PlayerTeam][0] - transform.position.x) - _extremeShootingDirectionLateralDistanceMargin;
        }

        float maximumForwardDistance = Mathf.Sqrt(Mathf.Pow(distanceToFirstReboundPosition, 2) + Mathf.Pow(maximumLateralDistance, 2));
        return rightVector.normalized * (rightSideIsTargeted ? 1 : -1) * maximumLateralDistance + forwardVector.normalized * maximumForwardDistance;
    }

    /// <summary>
    /// Calculates the actual force to apply on the ball, depending to the distance bewteen the player and the net.
    /// The more close the player is to the net, the less force is applied to the ball for a same loading time.
    /// </summary>
    /// <param name="hitForce"></param>
    /// <returns></returns>
    protected float CalculateActualForce(float hitForce)
    {
        float clampedDistanceToNet = CaluclateClampedDistanceToNet();
        float forceFactor = (clampedDistanceToNet / _maximumDistanceToNet) * (_forceMaximumClampFactor - _forceMinimumClampFactor) + _forceMinimumClampFactor;
        return forceFactor * hitForce;
    }

    /// <summary>
    /// Calculates the actual shooting direction according to the wanted shooting direction, the actual force applied by the player on the ball and the distance between
    /// the player and the net.
    /// This "actual shooting direction" system has been created to make the game more casual by avoiding direct faults as much as possible.
    /// </summary>
    /// <param name="wantedHorizontalDirection"></param>
    /// <param name="actualForce"></param>
    /// <returns></returns>
    public Vector3 CalculateActualShootingDirection(Vector3 wantedDirection, float forceToDistanceFactor, float actualforce)
    {
        float rotationSign = Mathf.Sign(Vector3.Dot(wantedDirection, Vector3.Project(_trainingManager.CameraTransform.right, Vector3.right)));
        Vector3 forwardVector = Vector3.Project(_trainingManager.CameraTransform.forward, Vector3.forward);
        Vector3 extremeShootingDirection = CalculateExtremeShootingDirection(rotationSign > 0 ? true : false, forceToDistanceFactor, actualforce);

        if (Vector3.Angle(forwardVector, wantedDirection) > Vector3.Angle(forwardVector, extremeShootingDirection))
        {
            return extremeShootingDirection;
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
}
