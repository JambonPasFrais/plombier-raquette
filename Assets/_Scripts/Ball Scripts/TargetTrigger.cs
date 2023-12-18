using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetTrigger : MonoBehaviour
{
    [SerializeField] private float _fadeDuration = 0.6f;
    [SerializeField] private float _rotationSpeed = 45f; 

    private SpriteRenderer spriteRenderer;

    #region UNITY METHODS

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartFadeOut();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.GetComponent<PlayerCameraController>())
        {
            other.GetComponent<PlayerCameraController>().SetCanSmash(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other != null && other.GetComponent<PlayerCameraController>())
        {
            other.GetComponent<PlayerCameraController>().SetCanSmash(false);
        }
    }

    #endregion

    private void StartFadeOut()
    {
        if (spriteRenderer != null)
        {
            StartCoroutine(FadeOutAndRotateCoroutine());
        }
    }

    private IEnumerator FadeOutAndRotateCoroutine()
    {
        Color currentColor = spriteRenderer.color;

        float elapsedTime = 0f;

        while (elapsedTime < _fadeDuration)
        {
            float alphaPercentage = elapsedTime / _fadeDuration;

            currentColor.a = Mathf.Lerp(1f, 0f, alphaPercentage);

            spriteRenderer.color = currentColor;

            transform.Rotate(Vector3.forward, _rotationSpeed * Time.deltaTime);

            yield return null;

            elapsedTime += Time.deltaTime;
        }

        currentColor.a = 0f;
        spriteRenderer.color = currentColor;
    }
}
