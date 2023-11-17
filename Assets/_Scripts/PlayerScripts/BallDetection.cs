using UnityEngine;

public class BallDetection : MonoBehaviour
{
    #region PRIVATE FIELDS

    [SerializeField] private BoxCollider _boxCollider;
    [SerializeField] private float _risingForceMinimumFactor;
    [SerializeField] private float _risingForceMaximumFactor;
    [SerializeField] private float _risingForceNormalFactor;

    private Ball _ball;
    private bool _isBallInHitZone;
    private float _risingForceFactor;

    #endregion

    #region ACCESSORS

    public bool IsBallInHitZone { get { return _isBallInHitZone; } }
    public Ball Ball { get { return _ball; } }

    #endregion

    #region UNITY METHODS

    private void Start()
    {
        _isBallInHitZone = false;
        _ball = null;
    }

    private void Update()
    {
        if (_ball == null && _isBallInHitZone)
        {
            _isBallInHitZone = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<Ball>(out Ball ball))
        {
            _isBallInHitZone = true;
            _ball = ball;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<Ball>(out Ball ball))
        {
            _isBallInHitZone = false;
            _ball = null;
            _risingForceFactor = 0f;
        }
    }

    #endregion

    public float GetRisingForceFactor()
    {
        if (_ball.gameObject.transform.position.y >= transform.position.y)
        {
            _risingForceFactor = _risingForceNormalFactor + (_risingForceMinimumFactor - _risingForceNormalFactor) * ((_ball.gameObject.transform.position.y - transform.position.y) / (_boxCollider.bounds.size.y / 2f));
        }
        else
        {
            _risingForceFactor = _risingForceNormalFactor + (_risingForceMaximumFactor - _risingForceNormalFactor) * ((transform.position.y - _ball.gameObject.transform.position.y) / (_boxCollider.bounds.size.y / 2f));
        }

        return _risingForceFactor;
    }
}
