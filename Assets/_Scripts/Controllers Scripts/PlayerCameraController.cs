using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCameraController : MonoBehaviour
{
    [Header("Instances")]
    [SerializeField] private GameObject _firstPersonCamera;
    [SerializeField] private Transform _ballSpawnPoint;
    
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
    
    #region GETTERS

    public GameObject FirstPersonCamera => _firstPersonCamera;
    public bool IsFirstPersonView => _isFirstPersonView;
    
    #endregion

    #region UNITY FUNCTIONS
    
    void Start()
    {
        _firstPersonCameraComponent = _firstPersonCamera.GetComponent<Camera>();
        _ballInstance = GameManager.Instance.BallInstance.GetComponent<Ball>();
        _smashTargetGo = GameManager.Instance.SmashTargetGo;
    }

    void Update()
    {
        if (_isFirstPersonView)
        {
            if (!_ballInstance.Rb.isKinematic)
            {
                _ballInstance.Rb.isKinematic = !_ballInstance.Rb.isKinematic;
            }
            _ballInstance.gameObject.transform.position = _ballSpawnPoint.position;
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            Vector3 rotation = new Vector3(-mouseY, mouseX, 0); 
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
        
        // TODO : fix it for 2v2
        //GameManager.Instance.SideManager.ActiveCameraTransform.gameObject.SetActive(!_isFirstPersonView);
        
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

    #endregion
    
    #region INTERN FUNCTIONS
    
    private IEnumerator ZoomIn()
    {
        float timer = 0f;
        float initialFOV = _firstPersonCamera.GetComponent<Camera>().fieldOfView;
        Debug.Log(initialFOV);
        while (timer < _zoomDuration)
        {
            float t = timer / _zoomDuration;
            _firstPersonCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(initialFOV, _zoomFOV, t);

            timer += Time.deltaTime;
            yield return null;
        }

        _firstPersonCamera.GetComponent<Camera>().fieldOfView = _zoomFOV;
    }
    
    #endregion
}
