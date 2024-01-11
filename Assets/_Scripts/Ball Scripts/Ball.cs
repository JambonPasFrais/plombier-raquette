using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public class Ball : MonoBehaviour
{
    #region PRIVATE FIELDS
    
    [Header("Star Parameters")]
    [SerializeField] private GameObject _starPrefab;
    [SerializeField] private float _raycastLength;
    [SerializeField] private float _horizontalOffset;
    [SerializeField] private float _verticalOffsetFromGround;

    [Header("Components")]
    [SerializeField] private Rigidbody _rigidBody;
    [SerializeField] private TrailRenderer _trailRenderer;
    [SerializeField] private SphereCollider _sphereCollider;

    [Header("Observed Variables")]
    [SerializeField] private ShotParameters _shotParameters;
    [SerializeField] private ControllersParent _lastPlayerToApplyForce;
    [SerializeField] private int _reboundsCount;
    [SerializeField] private GameObject _smashStarInstance;

    [Header("GA")] [SerializeField] private Gradient _dropColor;
    [SerializeField] private Gradient _lobColor;
    [SerializeField] private Gradient _topSpinColor;
    [SerializeField] private Gradient _sliceColor;
    [SerializeField] private Gradient _flatColor;
    [SerializeField] private Gradient _smashColor;
    [SerializeField] private ParticleSystem _hitEffect;
    [SerializeField] private ParticleSystem _reboundEffect;

    private float _risingForceFactor;
    private Coroutine _currentMovementCoroutine;
    private Coroutine _currentCurvingEffectCoroutine;
    private Dictionary<HitType, Gradient> _colorGradientByHitType;
    private Coroutine _currentEffectCoroutine;

    #endregion

    #region GETTERS

    public int ReboundsCount { get { return _reboundsCount; } }
    public ControllersParent LastPlayerToApplyForce { get { return _lastPlayerToApplyForce; } }

    public Rigidbody Rb => _rigidBody;

    #endregion

    #region UNITY METHODS

    private void Start()
    {
        _reboundsCount = 0;
        _sphereCollider = GetComponent<SphereCollider>();
        _trailRenderer = GetComponent<TrailRenderer>();
        _rigidBody = GetComponent<Rigidbody>();
        
        CreateColorGradientByHitTypeDict();
        
        #region Effects
        _hitEffect.Stop();
        _reboundEffect.Stop();
        #endregion
    }

    private void Update()
    {
        if (transform.position.y < -1)
        {
            GameManager.Instance.EndOfPointWithoutPointWinner();
            ResetBall();
        }

        if (_rigidBody.isKinematic && GameManager.Instance.GameState == GameState.SERVICE)
        {
            transform.position = GameManager.Instance.ServiceBallInitializationPoint.position;
        }
        else if (!_rigidBody.isKinematic) 
        {
            DrawSmashStar();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.GetComponent<ControllersParent>() && !_rigidBody.isKinematic)
        {
            GameManager.Instance.EndOfPoint(_lastPlayerToApplyForce.PlayerTeam);
            GameManager.Instance.ScoreManager.AddPoint(_lastPlayerToApplyForce.PlayerTeam);
            ResetBall();
        }
    }

    #endregion

    #region PHYSICS BEHAVIOR METHODS

    public void InitializePhysicsMaterial(PhysicMaterial physicMaterial)
    {
        if (_sphereCollider.material != physicMaterial)
        {
            _sphereCollider.sharedMaterial = physicMaterial;
        }
    }

    public void InitializeActionParameters(ShotParameters shotParameters)
    {
        _shotParameters = shotParameters;
        ModifyTrailRendererColorByHitType(shotParameters.HitType);
    }

    public void ApplyForce(float force, float risingForceFactor, Vector3 normalizedDirection, ControllersParent playerToApplyForce)
    {
        PlayEffect(_hitEffect);
        
        _rigidBody.velocity = Vector3.zero;

        if (_currentMovementCoroutine != null)
        {
            StopCoroutine(_currentMovementCoroutine);
        }

        if (_currentCurvingEffectCoroutine != null)
        {
            StopCoroutine(_currentCurvingEffectCoroutine);
        }

        _risingForceFactor = risingForceFactor;
        float actualHorizontalForce = _shotParameters.ShotForceFactor * force;

        Vector3 curvingDirection = Vector3.Project(playerToApplyForce.gameObject.transform.position - transform.position, Vector3.right);
        Vector3 actualNormalizedDirection;
        if (playerToApplyForce is PlayerController && _shotParameters.ForceToDistanceFactor != 0) 
        {
            actualNormalizedDirection = playerToApplyForce.CalculateActualShootingDirection(normalizedDirection, _shotParameters.ForceToDistanceFactor, actualHorizontalForce);
        }
        else
        {
            actualNormalizedDirection = normalizedDirection;
        }

        _currentMovementCoroutine = StartCoroutine(BallMovement(actualHorizontalForce, actualNormalizedDirection.normalized, curvingDirection));

        _lastPlayerToApplyForce = playerToApplyForce;
    }

    private IEnumerator BallMovement(float actualHorizontalForce, Vector3 actualNormalizedHorizontalDirection, Vector3 curvingDirection)
    {
        _reboundsCount = 0;

        //Debug.Log($"Direction {actualNormalizedHorizontalDirection} - Force {actualHorizontalForce}");

        _rigidBody.AddForce(actualNormalizedHorizontalDirection * actualHorizontalForce);
        _rigidBody.AddForce(Vector3.up * _shotParameters.RisingForce * _risingForceFactor);

        _currentCurvingEffectCoroutine = StartCoroutine(CurvingEffect(curvingDirection));

        yield return new WaitForSeconds(_shotParameters.TimeBeforeGoingDown);

        float countdown = 0;
        while (countdown < _shotParameters.DecreasingForcePhaseTime)
        {
            _rigidBody.AddForce(-Vector3.up * _shotParameters.DecreasingForce);

            yield return new WaitForSeconds(Time.deltaTime);

            countdown += Time.deltaTime;
        }
    }

    private IEnumerator CurvingEffect(Vector3 curvingDirection)
    {
        float afterReboundCurvingEffectTime = 0f;

        while (true)
        {
            if (_reboundsCount == 0)
            {
                _rigidBody.AddForce(curvingDirection.normalized * _shotParameters.InAirCurvingForce);
            }
            else
            {
                _rigidBody.AddForce(curvingDirection.normalized * _shotParameters.AfterReboudCurvingForce);
                afterReboundCurvingEffectTime += Time.deltaTime;

                if (afterReboundCurvingEffectTime >= _shotParameters.AfterReboudCurvingEffectDuration)
                {
                    break;
                }
            }

            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    public void Rebound()
    {
        _reboundsCount++;
        
        //PlayEffect(_reboundEffect);

        Vector3 direction = Vector3.Project(_rigidBody.velocity, Vector3.forward) + Vector3.Project(_rigidBody.velocity, Vector3.right);
        _rigidBody.AddForce(direction.normalized * (_shotParameters.AddedForceInSameDirection / _reboundsCount));
    }

    #endregion

    public void ResetBall()
    {
        if (_currentMovementCoroutine != null)
        {
            StopCoroutine(_currentMovementCoroutine);
        }
        if (_currentCurvingEffectCoroutine != null)
        {
            StopCoroutine(_currentCurvingEffectCoroutine);
        }
        if (_currentEffectCoroutine != null)
        {
            StopCoroutine(_currentEffectCoroutine);
        }
        
        _hitEffect.Stop();
        _reboundEffect.Stop();
        
        _reboundsCount = 0;
        _lastPlayerToApplyForce = null;
        _rigidBody.velocity = Vector3.zero;
        _rigidBody.isKinematic = true;
        DestroySmashStar();

        GameManager.Instance.GameState = GameState.SERVICE;
        GameManager.Instance.BallServiceInitialization();
    }

    #region SMASH STAR MANAGEMENT

    private void DrawSmashStar()
    {
        if (_lastPlayerToApplyForce != null)
        {
            Ray ray = new Ray(transform.position, Vector3.down);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, _raycastLength) && hit.collider.gameObject.TryGetComponent<FieldGroundPart>(out FieldGroundPart fieldGroundPart))
            {
                if (fieldGroundPart.OwnerPlayer != _lastPlayerToApplyForce)
                {
                    Vector3 horizontalBallDirection = Vector3.Project(_rigidBody.velocity, Vector3.forward) + Vector3.Project(_rigidBody.velocity, Vector3.right);
                    Vector3 starPosition = hit.point + Vector3.up * _verticalOffsetFromGround + horizontalBallDirection.normalized * _horizontalOffset;

                    if (_smashStarInstance == null)
                    {
                        InstantiateSmashStar(starPosition);
                    }
                    else
                    {
                        MoveSmashStar(starPosition);
                    }
                }
            }
            else
            {
                DestroySmashStar();
            }
        }
    }

    private void MoveSmashStar(Vector3 position)
    {
        _smashStarInstance.transform.position = position;
    }

    public void DestroySmashStar()
    {
        if (_smashStarInstance != null)
        {
            Destroy(_smashStarInstance);
            _smashStarInstance = null;
        }
    }

    private void InstantiateSmashStar(Vector3 position)
    {
        if (_starPrefab != null)
        {
            if (_smashStarInstance != null)
            {
                Destroy(_smashStarInstance);
            }

            _smashStarInstance = Instantiate(_starPrefab, position, Quaternion.Euler(90, 0, 0));
        }
    }

    #endregion
    
    #region ART

    private void CreateColorGradientByHitTypeDict()
    {
        _colorGradientByHitType = new Dictionary<HitType, Gradient>
        {
            { HitType.Drop, _dropColor },
            { HitType.Lob, _lobColor },
            { HitType.TopSpin, _topSpinColor },
            { HitType.Slice, _sliceColor },
            { HitType.Flat, _flatColor },
            { HitType.Smash, _smashColor }
        };
    }
    
    private void ModifyTrailRendererColorByHitType(HitType hitType)
    {
        _trailRenderer.colorGradient = _colorGradientByHitType[hitType];
    }

    private void PlayEffect(ParticleSystem effect)
    {
        /*if (_currentEffectCoroutine != null)
            StopCoroutine(_currentEffectCoroutine);*/
        
        _currentEffectCoroutine = StartCoroutine(EffectCoroutine(effect));
    }
    
    private IEnumerator EffectCoroutine(ParticleSystem effect)
    {
        effect.Play();

        yield return new WaitForSeconds(effect.main.duration);
        
        effect.Stop();
    }
    
    #endregion
}


