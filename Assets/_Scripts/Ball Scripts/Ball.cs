using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Ball : MonoBehaviour
{
    #region PRIVATE FIELDS

    [SerializeField] private ShotParameters _shotParameters;

    [Header("Components")]
    [SerializeField] private Rigidbody _rigidBody;

    [SerializeField]private ControllersParent _lastPlayerToApplyForce;
    private float _risingForceFactor;
    [SerializeField] private int _reboundsCount;
    private Coroutine _currentMovementCoroutine;
    private Coroutine _currentCurvingEffectCoroutine;
    [SerializeField]private bool _canSmash = false;
    [SerializeField] private GameObject _ciblePrefab;
    [SerializeField] private GameObject _targetInstance;
    #endregion

    #region ACCESSORS
    public bool canSmash { get { return _canSmash; } }
    public int ReboundsCount { get { return _reboundsCount; } }
    public ControllersParent LastPlayerToApplyForce { get { return _lastPlayerToApplyForce; } }

    #endregion
    private Vector3 _initialPosition;
    private Vector3 _velocity;
    private float _gravity = 9.81f;
    private LayerMask _groundLayer;
    #region UNITY METHODS

    private void Start()
    {
        _reboundsCount = 0;
        _groundLayer = LayerMask.GetMask("ground");
    }

    private void Update()
    {
        if (transform.position.y < -1)
        {
            GameManager.Instance.EndOfPoint();
            ResetBall();
        }

        if (_rigidBody.isKinematic)
        {
            transform.position = GameManager.Instance.BallInitializationTransform.position;
        }
        if (_canSmash)
        {
            CheckSmash();
        }
    }

    #endregion

    #region PHYSICS BEHAVIOR METHODS

    public void InitializePhysicsMaterial(PhysicMaterial physicMaterial)
    {
        if (gameObject.GetComponent<SphereCollider>().material != physicMaterial)
        {
            gameObject.GetComponent<SphereCollider>().sharedMaterial = physicMaterial;
        }
    }

    public void InitializeActionParameters(ShotParameters shotParameters)
    {
        _shotParameters = shotParameters;
    }

    public void ApplyForce(float force, float risingForceFactor, Vector3 normalizedHorizontalDirection, ControllersParent playerToApplyForce)
    {
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
        Vector3 curvingDirection = Vector3.Project(playerToApplyForce.gameObject.transform.position - transform.position, Vector3.right);

        _currentMovementCoroutine = StartCoroutine(BallMovement(force, normalizedHorizontalDirection, curvingDirection));

        _lastPlayerToApplyForce = playerToApplyForce;
    }

    private IEnumerator BallMovement(float force, Vector3 normalizedDirection, Vector3 curvingDirection)
    {
        _reboundsCount = 0;

        _rigidBody.AddForce(normalizedDirection * force * _shotParameters.ShotForceFactor);
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

        _reboundsCount = 0;
        _lastPlayerToApplyForce = null;
        _rigidBody.velocity = Vector3.zero;
        _rigidBody.isKinematic = true;

        GameManager.Instance.GameState = GameState.SERVICE;
        GameManager.Instance.BallServiceInitialization();
    }
    public void ShootSmash(GameObject camera, float ballSpeed, ControllersParent controllersParent)
    {
        if (_lastPlayerToApplyForce != controllersParent)
        {
            _lastPlayerToApplyForce = controllersParent;
            _rigidBody.AddForce(camera.transform.forward * ballSpeed, ForceMode.VelocityChange);
        }
    }
    public void SetCanSmash(bool canSmash)
    {
        _canSmash = canSmash;
        //Destroy(_cibleInstance);
        //_cibleInstance = null;
    }
    private void CheckSmash()
    {
        float simulationTime = 5f; // Temps de simulation en secondes (ajustez selon vos besoins).
        float timeStep = 0.1f; // Intervalle de temps entre chaque étape de simulation.

        Vector3 simulatedPosition = transform.position;
        Vector3 simulatedVelocity = _rigidBody.velocity;

        for (float t = 0; t < simulationTime; t += timeStep)
        {
            // Calculez la nouvelle position simulée en utilisant les équations de mouvement.
            simulatedPosition += simulatedVelocity * timeStep;
            simulatedVelocity.y += _gravity * timeStep; // Ajoutez la gravité.

            // Vérifiez s'il y a une collision avec le sol (ou un autre objet).
            if (CheckCollisionWithGround(simulatedPosition))
            {
                Debug.Log("Collision avec le sol à la position simulée : " + simulatedPosition+"position balle"+transform.position);
                break;
            }
        }
    }

    private bool CheckCollisionWithGround(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(position, -Vector3.up, out hit, Mathf.Infinity, _groundLayer))
        {
            Debug.Log("Collision avec le sol à la position simulée : " + position+hit);

            // Instancier la cible à la position de la collision
            if (_targetInstance == null)
            {
                _targetInstance = Instantiate(_ciblePrefab, hit.point, Quaternion.identity);
                // Ajoutez d'autres configurations si nécessaires
            }

            // Détruire la cible lorsque _canSmash devient vrai ou lors de la réinitialisation
            if (_canSmash || _rigidBody.isKinematic)
            {
                Destroy(_targetInstance);
                _targetInstance = null;
            }

            // Mettez à jour _canSmash à true à votre convenance
            // _canSmash = true;

            return true;
        }

        return false;
    }
}

