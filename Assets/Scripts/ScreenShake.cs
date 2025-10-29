using System.Collections;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    [SerializeField] private AnimationCurve curve;

    public void StartShake(float duration, float strength)
    {
        StartCoroutine(ShakeRoutine(duration, strength));
    }

    private IEnumerator ShakeRoutine(float duration, float strength)
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float curveStrength = curve.Evaluate(elapsedTime / duration);
            transform.position = startPosition + curveStrength * strength * Random.insideUnitSphere;

            yield return null;
        }

        transform.position = startPosition;
    }
}