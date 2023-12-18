using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCameraController : MonoBehaviour
{
    [Header("Camera For Smash Attacks")]
    [SerializeField] private GameObject _firstPersonCamera;
    [SerializeField] private GameObject _ballPrefab;
    [SerializeField] private PlayerController _player;
    [SerializeField] private Transform _ballSpawnPoint;
    [SerializeField] private float _rotationSpeed = 10f;
    [SerializeField] private float _zoomFOV = 40f;
    [SerializeField] private float _normalFOV = 60f; 
    [SerializeField] private float _zoomDuration = 0.5f;
    [SerializeField] private Image _smashImage;

    [SerializeField] private bool _canSmash;
    [SerializeField] private float _distanceToBall = 5f;
    private Camera _firstPersonCameraComponent;

    public bool _isFirstPersonView;

    private Quaternion _cameraOriginalRot;

    void Start()
    {
        _firstPersonCameraComponent = _firstPersonCamera.GetComponent<Camera>();
        _cameraOriginalRot = _firstPersonCamera.transform.rotation;
        _ballPrefab = GameManager.Instance.BallInstance;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && !_isFirstPersonView && GameManager.Instance.GameState == GameState.PLAYING && _canSmash) 
        {
            ToggleFirstPersonView();
            _ballPrefab.transform.rotation = Quaternion.identity;
        }

        if (_isFirstPersonView)
        {
            if (!_ballPrefab.GetComponent<Rigidbody>().isKinematic)
            {
                _ballPrefab.GetComponent<Rigidbody>().isKinematic = !_ballPrefab.GetComponent<Rigidbody>().isKinematic;
            }
            _ballPrefab.transform.position = _ballSpawnPoint.position;
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

            if (Input.GetMouseButtonDown(0)) 
            {
                _ballPrefab.GetComponent<Rigidbody>().isKinematic = false;
                _ballPrefab.GetComponent<Ball>().InitializePhysicsMaterial(NamedPhysicMaterials.GetPhysicMaterialByName(_player.PossiblePhysicMaterials, "Normal"));
                _ballPrefab.GetComponent<Ball>().InitializeActionParameters(NamedActions.GetActionParametersByName(_player.PossibleActions, "Smash"));
                _ballPrefab.GetComponent<Ball>().ApplyForce(_player.MaximumShotForce, 0f, _firstPersonCamera.transform.forward, _player);
                ToggleFirstPersonView();
            }
        }
    }

    private void ToggleFirstPersonView()
    {
        GetComponent<PlayerController>().SetSmash();
        _firstPersonCamera.transform.rotation = _cameraOriginalRot;
        _isFirstPersonView = !_isFirstPersonView;
        GameManager.Instance.SideManager.ActiveCameraTransform.gameObject.SetActive(!_isFirstPersonView);
        _firstPersonCamera.SetActive(_isFirstPersonView);
        _smashImage.gameObject.SetActive(_isFirstPersonView);
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

    public void setCanSmash(bool canSmash)
    {
        _canSmash = canSmash;
    }
}
