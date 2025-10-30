using System.Collections;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    [SerializeField] private AnimationCurve curve;

    private Coroutine continuousRoutine;

    // Starts a single shake for a given duration and strength
    public void StartShake(float duration, float strength)
    {
        StartCoroutine(ShakeRoutine(duration, strength));
    }

    // Starts continuous shaking until StopContinuous() is called
    public void StartContinuous(float strength, float segment = 0.2f, float overlap = 0.05f)
    {
        if (continuousRoutine != null)
            StopCoroutine(continuousRoutine);

        continuousRoutine = StartCoroutine(ContinuousRoutine(strength, segment, overlap));
    }

    // Stops continuous shaking and resets the transform
    public void StopContinuous()
    {
        if (continuousRoutine != null)
        {
            StopCoroutine(continuousRoutine);
            continuousRoutine = null;
        }

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    // Handles a single shake
    private IEnumerator ShakeRoutine(float duration, float strength)
    {
        Vector3 startPosition = transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float curveStrength = curve.Evaluate(elapsedTime / duration);
            transform.localPosition = startPosition + curveStrength * strength * Random.insideUnitSphere;
            yield return null;
        }

        transform.localPosition = startPosition;
    }

    // Repeatedly triggers short shakes so it feels seamless
    private IEnumerator ContinuousRoutine(float strength, float segment, float overlap)
    {
        var wait = new WaitForSeconds(Mathf.Max(0.01f, segment - overlap));
        while (true)
        {
            StartShake(segment, strength);
            yield return wait;
        }
    }

    private void OnDisable()
    {
        StopContinuous();
    }
}
