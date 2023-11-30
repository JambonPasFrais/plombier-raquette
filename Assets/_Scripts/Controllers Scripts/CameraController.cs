using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera For Smash Attacks")]
    [SerializeField] private GameObject _mainCamera;
    [SerializeField] private GameObject _firstPersonCamera;
    [SerializeField] private GameObject _ballPrefab;
    [SerializeField] private PlayerController _player;
    [SerializeField] private Transform _ballSpawnPoint;
    [SerializeField] private float _ballSpeed = 10f;

    private bool _isFirstPersonView;
    // Start is called before the first frame update
    void Start()
    {
        
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
            // Gérer la rotation de la caméra à la première personne avec la souris
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            // Ajouter ici la logique pour la rotation de la caméra avec la souris

            // Tirer le smash
            if (Input.GetMouseButtonDown(0))
            {
                ShootSmash();
            }
        }
    }
    void ToggleFirstPersonView()
    {
        _isFirstPersonView = !_isFirstPersonView;

        // Activer ou désactiver les caméras en conséquence
        _mainCamera.SetActive(!_isFirstPersonView);
        _firstPersonCamera.SetActive(_isFirstPersonView);

        // Réinitialiser la position et la rotation de la caméra à la première personne
        //if (_isFirstPersonView)
        //{
        //    _firstPersonCamera.transform.position = _player.transform.position;
        //    _firstPersonCamera.transform.rotation = _player.transform.rotation;
        //}
    }

    public void ShootSmash()
    {
        // Instancier la balle au point de départ avec la direction de la caméra à la première personne
        GameObject ball = Instantiate(_ballPrefab, _ballSpawnPoint.position, _firstPersonCamera.transform.rotation);
        // Appliquer une force à la balle dans la direction de la caméra
        ball.GetComponent<Rigidbody>().velocity = _firstPersonCamera.transform.forward * _ballSpeed;

        // Attendre un court instant avant de revenir à la vue initiale
        StartCoroutine(ResetToThirdPersonView());
    }
    IEnumerator ResetToThirdPersonView()
    {
        yield return new WaitForSeconds(0.5f); // Ajustez la durée selon vos besoins

        // Revenir à la vue à la troisième personne
        _firstPersonCamera.SetActive(false);
        _mainCamera.SetActive(true);
        
    }

}
