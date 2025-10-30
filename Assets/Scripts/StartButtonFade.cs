using UnityEngine;
using System.Collections;

public class StartButtonFade : MonoBehaviour
{
    public SpriteRenderer sr;
    public float fadeTime = 1f;

    void Start()
    {
        StartCoroutine(FadeLoop());
    }

    IEnumerator FadeLoop()
    {
        while (true)
        {
            yield return Fade(0f, 1f); // fade in
            yield return new WaitForSeconds(0.5f);
            yield return Fade(1f, 0f); // fade out
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        Color c = sr.color;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, t / fadeTime);
            sr.color = c;
            yield return null;
        }
    }
}