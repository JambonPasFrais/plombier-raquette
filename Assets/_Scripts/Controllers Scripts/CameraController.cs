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
            // G�rer la rotation de la cam�ra � la premi�re personne avec la souris
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            // Ajouter ici la logique pour la rotation de la cam�ra avec la souris

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

        // Activer ou d�sactiver les cam�ras en cons�quence
        _mainCamera.SetActive(!_isFirstPersonView);
        _firstPersonCamera.SetActive(_isFirstPersonView);

        // R�initialiser la position et la rotation de la cam�ra � la premi�re personne
        //if (_isFirstPersonView)
        //{
        //    _firstPersonCamera.transform.position = _player.transform.position;
        //    _firstPersonCamera.transform.rotation = _player.transform.rotation;
        //}
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
    IEnumerator ResetToThirdPersonView()
    {
        yield return new WaitForSeconds(0.5f); // Ajustez la dur�e selon vos besoins

        // Revenir � la vue � la troisi�me personne
        _firstPersonCamera.SetActive(false);
        _mainCamera.SetActive(true);
        
    }

}
