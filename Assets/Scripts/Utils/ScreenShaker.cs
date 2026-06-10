using System.Collections;
using Singleton;
using UnityEngine;

public class ScreenShaker : SingletonBase<ScreenShaker>
{

    private Vector3 _originalPosition;
    private Coroutine _coroutine;

    void OnEnable()
    {
        _originalPosition = transform.localPosition;
    }

    public void Shake(float duration, float magnitude)
    {
        if (_coroutine != null)
               StopCoroutine(_coroutine);

        _coroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        _originalPosition = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Vector2 randomPoint = Random.insideUnitCircle * magnitude;
            transform.localPosition = _originalPosition + (Vector3)randomPoint;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = _originalPosition;
    }

    public void StopShake()
    {
        if (_coroutine != null)
            StopCoroutine(_coroutine);
    }
}