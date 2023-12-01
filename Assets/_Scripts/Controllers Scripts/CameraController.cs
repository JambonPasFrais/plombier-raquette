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
    public bool _isFirstPersonView;
    public bool _isSmashing = false;
    // Start is called before the first frame update
    void Start()
    {
        _firstPersonCameraComponent = _firstPersonCamera.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleFirstPersonView();
        }

        if (_isFirstPersonView)
        {

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            Vector3 rotation = new Vector3(0, mouseX, 0); 
            _firstPersonCamera.transform.Rotate(rotation * _rotationSpeed * Time.deltaTime);

            float verticalRotation = -mouseY * _rotationSpeed * Time.deltaTime;
            _firstPersonCamera.transform.Rotate(verticalRotation, 0, 0);

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


    public void ShootSmash()
    {
        // Instancier la balle au point de d�part avec la direction de la cam�ra � la premi�re personne
        GameObject ball = Instantiate(_ballPrefab, _ballSpawnPoint.position, _firstPersonCamera.transform.rotation);
        // Appliquer une force � la balle dans la direction de la cam�ra
        ball.GetComponent<Rigidbody>().velocity = _firstPersonCamera.transform.forward * _ballSpeed;

        // Attendre un court instant avant de revenir � la vue initiale
        StartCoroutine(ResetToThirdPersonView());
    }

    public void ToggleFirstPersonView()
    {
        _isFirstPersonView = !_isFirstPersonView;
        _mainCamera.SetActive(!_isFirstPersonView);
        _firstPersonCamera.SetActive(_isFirstPersonView);
        _smashImage.gameObject.SetActive(true);
        Cursor.visible = false;
        if (_isFirstPersonView)
        {
            Debug.Log("tu zoom normalement");
            StartCoroutine(ZoomIn());
        }
        else
        {
            StartCoroutine(ResetToThirdPersonView());
        }

    }

    public IEnumerator ZoomIn()
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

    public IEnumerator ResetToThirdPersonView()
    {
        float timer = 0f;
        float initialFOV = _mainCamera.GetComponent<Camera>().fieldOfView;

        while (timer < _zoomDuration)
        {
            float t = timer / _zoomDuration;
            _mainCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(initialFOV, _normalFOV, t);

            timer += Time.deltaTime;
            yield return null;
        }

        _mainCamera.GetComponent<Camera>().fieldOfView = _normalFOV;
        _smashImage.gameObject.SetActive(false);
        _isFirstPersonView = !_isFirstPersonView;
        Cursor.visible = true;
        _firstPersonCamera.SetActive(false);
        _mainCamera.SetActive(true);
        _isSmashing = false;
    }

    public bool GetIsSmashing()
    {
        return _isSmashing;
    }
}
