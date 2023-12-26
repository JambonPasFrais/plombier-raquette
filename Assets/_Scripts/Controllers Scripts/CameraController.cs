using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    [Header("Camera For Smash Attacks")]
    [SerializeField] private GameObject _mainCamera;
    [SerializeField] private GameObject _firstPersonCamera;
    [SerializeField] private GameObject _ballPrefab;
    [SerializeField] private PlayerController _player;
    [SerializeField] private Transform _ballSpawnPoint;
    [SerializeField] private float _ballSpeed = 10f;
    [SerializeField] private float _rotationSpeed = 10f;
    [SerializeField] private float _zoomFOV = 40f;
    [SerializeField] private float _normalFOV = 60f; 
    [SerializeField] private float _zoomDuration = 0.5f;
    [SerializeField] private Image _smashImage;
    private Camera _firstPersonCameraComponent;
    public bool IsSmashing => _isSmashing;

    public bool _isFirstPersonView;
    [SerializeField] private bool _isSmashing = false;

    private Quaternion _cameraOriginalRot;

    void Start()
    {
        _firstPersonCameraComponent = _firstPersonCamera.GetComponent<Camera>();
        _cameraOriginalRot = _firstPersonCamera.transform.rotation;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)&&!_isFirstPersonView)
        {
            ToggleFirstPersonView();
        }

        if (_isFirstPersonView)
        {

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

            // Appliquer la rotation verticale limit�e � la cam�ra
            _firstPersonCamera.transform.eulerAngles = new Vector3(currentXRotation, _firstPersonCamera.transform.eulerAngles.y, 0);

            if (Input.GetMouseButtonDown(0))
            {
                ShootSmash();
            }
        }
    }


    private void ShootSmash()
    {
        GameObject ball = Instantiate(_ballPrefab, _ballSpawnPoint.position, _firstPersonCamera.transform.rotation);
        ball.GetComponent<Rigidbody>().velocity = _firstPersonCamera.transform.forward * _ballSpeed;
        ToggleFirstPersonView();
    }

    private void ToggleFirstPersonView()
    {
        _isSmashing = !_isSmashing;
        _firstPersonCamera.transform.rotation = _cameraOriginalRot;
        _isFirstPersonView = !_isFirstPersonView;
        _mainCamera.SetActive(!_isFirstPersonView);
        _firstPersonCamera.SetActive(_isFirstPersonView);
        _smashImage.gameObject.SetActive(_isFirstPersonView);
        Cursor.visible = !_isFirstPersonView;
        if (_isFirstPersonView)
        {
            StartCoroutine(ZoomIn());
        }
        else
        {
            _firstPersonCameraComponent.fieldOfView = _normalFOV;
        }

    }

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
}