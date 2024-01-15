using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerCameraController : MonoBehaviour
{
    [Header("Instances")]
    [SerializeField] private GameObject _firstPersonCamera;
    [SerializeField] private Transform _ballSmashPosition;
    
    [Header("Camera Parameters for Smash")]
    [SerializeField] private float _rotationSpeed = 10f;
    [SerializeField] private float _zoomFOV = 40f;
    [SerializeField] private float _normalFOV = 60f; 
    [SerializeField] private float _zoomDuration = 0.5f;
    
    // Instances
    private Camera _firstPersonCameraComponent;
    private GameObject _smashTargetGo;
    private Ball _ballInstance;
    
    // Logic variables
    private bool _isFirstPersonView;
    private Vector2 _targetMovements;
    
    #region GETTERS

    public GameObject FirstPersonCamera => _firstPersonCamera;
    public bool IsFirstPersonView => _isFirstPersonView;
    
    #endregion

    #region UNITY FUNCTIONS
    
    void Start()
    {
        _firstPersonCameraComponent = _firstPersonCamera.GetComponent<Camera>();
        _smashTargetGo = GameManager.Instance.SmashTargetGo;
    }

    private void Update()
    {
        if (_ballInstance == null && GameManager.Instance.BallInstance != null) 
        {
            _ballInstance = GameManager.Instance.BallInstance.GetComponent<Ball>();
        }
    }

    void FixedUpdate()
    {
        if (_isFirstPersonView && GameManager.Instance.GameState != GameState.BEFOREGAME &&
            GameManager.Instance.GameState != GameState.ENDPOINT && GameManager.Instance.GameState != GameState.ENDMATCH)
        {
            if (!_ballInstance.Rb.isKinematic)
            {
                _ballInstance.Rb.isKinematic = !_ballInstance.Rb.isKinematic;
            }

            _ballInstance.gameObject.transform.position = _ballSmashPosition.position;

            GameManager.Instance.photonView.RPC("OnlineBallPositionSettingDuringSmash", RpcTarget.Others, _ballSmashPosition.position);

            Vector3 rotation = new Vector3(-_targetMovements.y, _targetMovements.x, 0);
            
            _firstPersonCamera.transform.Rotate(rotation * _rotationSpeed * Time.deltaTime);

            float currentXRotation = _firstPersonCamera.transform.eulerAngles.x;
            if (currentXRotation > 90 && currentXRotation < 180)
            {
                currentXRotation = 90;
            }
            else if (currentXRotation > 180 && currentXRotation < 270)
            {
                currentXRotation = 270;
            }

            _firstPersonCamera.transform.eulerAngles = new Vector3(currentXRotation, _firstPersonCamera.transform.eulerAngles.y, 0);
        }
    }

    #endregion
    
    #region FUNCTIONS CALLED EXTERNALLY
    
    public void ToggleFirstPersonView()
    {
        Vector3 horizontalBallDirection = Vector3.Project(_ballInstance.Rb.velocity, Vector3.forward) +
            Vector3.Project(_ballInstance.Rb.velocity, Vector3.right);
        Vector3 cameraLookingDirection = -horizontalBallDirection;
        _firstPersonCamera.transform.forward = cameraLookingDirection;
        
        _isFirstPersonView = !_isFirstPersonView;

        _firstPersonCamera.SetActive(_isFirstPersonView);
        _smashTargetGo.SetActive(_isFirstPersonView);
        
        Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
        
        if (_isFirstPersonView)
        {
            StartCoroutine(ZoomIn());
        }
        else
        {
            _firstPersonCameraComponent.fieldOfView = _normalFOV;
        }
    }
    
    public void AimSmashTarget(InputAction.CallbackContext context)
    {
        if (!_isFirstPersonView)
            return;

        _targetMovements = context.ReadValue<Vector2>();
    }

    #endregion
    
    #region INTERN FUNCTIONS
    
    private IEnumerator ZoomIn()
    {
        float timer = 0f;
        float initialFOV = _firstPersonCameraComponent.fieldOfView;
        
        while (timer < _zoomDuration)
        {
            float t = timer / _zoomDuration;
            _firstPersonCameraComponent.fieldOfView = Mathf.Lerp(initialFOV, _zoomFOV, t);

            timer += Time.deltaTime;
            yield return null;
        }

        _firstPersonCameraComponent.fieldOfView = _zoomFOV;
    }
    
    #endregion
}
