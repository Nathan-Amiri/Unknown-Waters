using System.Collections;
using UnityEngine;

public class Fade : MonoBehaviour
{
    private SpriteRenderer sr;

    private float alpha;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void StartFade()
    {
        StopAllCoroutines();
        StartCoroutine(FadeRoutine());
    }
    private IEnumerator FadeRoutine()
    {
        alpha = 0;

        while (alpha < 1)
        {
            alpha += Time.deltaTime * 1.4f;
            yield return null;
        }

        alpha = 1;

        yield return new WaitForSeconds(.2f);

        while (alpha > 0)
        {
            alpha -= Time.deltaTime * 1.4f;
            yield return null;
        }

        alpha = 0;
    }
    private void Update()
    {
        Color newColor = Color.black;
        newColor.a = alpha;
        sr.color = newColor;
    }
}