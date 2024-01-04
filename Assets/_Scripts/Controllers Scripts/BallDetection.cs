using UnityEngine;

public class BallDetection : MonoBehaviour
{
    #region PRIVATE FIELDS

    [SerializeField] private AgentTrainingManager _trainingManager;
    [SerializeField] private BoxCollider _boxCollider;
    [SerializeField] private float _risingForceMinimumFactor;
    [SerializeField] private float _risingForceMaximumFactor;
    [SerializeField] private float _risingForceNormalFactor;

    private AIBall _ball;
    private bool _isBallInHitZone;
    private ControllersParent _controller;

    #endregion

    #region ACCESSORS

    public bool IsBallInHitZone { get { return _isBallInHitZone; } }
    public AIBall Ball { get { return _ball; } }
    public BoxCollider BoxCollider { get { return _boxCollider; } }

    #endregion

    #region UNITY METHODS

    private void Start()
    {
        _isBallInHitZone = false;
        _ball = null;
        _controller = transform.parent.gameObject.GetComponent<ControllersParent>();
    }

    private void Update()
    {
        if (_ball == null && _isBallInHitZone)
        {
            _isBallInHitZone = false;
        }

        if (_ball != null && _controller is AgentController && _ball.LastPlayerToApplyForce != _controller && _controller.PlayerState != PlayerStates.SERVE)
        {
            ((AgentController)_controller).AgentDoesntHitTheBall();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<AIBall>(out AIBall ball))
        {
            _isBallInHitZone = true;
            _ball = ball;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<AIBall>(out AIBall ball))
        {
            _isBallInHitZone = false;
            _ball = null;
        }
    }

    #endregion

    /// <summary>
    /// Calculates the rising force factor to apply on the ball considering the altitude of the ball compared to the altitude of the player.
    /// </summary>
    /// <returns></returns>
    public float GetRisingForceFactor(HitType hitType)
    {
        if (hitType != HitType.Lob)
        {
            if (_ball.gameObject.transform.position.y >= transform.position.y)
            {
                return _risingForceNormalFactor + (_risingForceMinimumFactor - _risingForceNormalFactor) *
                    ((_ball.gameObject.transform.position.y - transform.position.y) / (_boxCollider.bounds.size.y / 2f));
            }

            return _risingForceNormalFactor + (_risingForceMaximumFactor - _risingForceNormalFactor) * ((transform.position.y - _ball.gameObject.transform.position.y) / (_boxCollider.bounds.size.y / 2f));
        }

        return _risingForceNormalFactor;
    }

    /// <summary>
    /// Set the ball variable at service beginning for the bot.
    /// </summary>
    public void SetBallAtService()
    {
        _ball = _trainingManager.BallInstance.GetComponent<AIBall>();
        _isBallInHitZone = true;
    }
}
