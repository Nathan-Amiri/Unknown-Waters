using UnityEngine;

public class LanternFlicker : MonoBehaviour
{
    SpriteRenderer sr;
    public float baseAlpha;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseAlpha = sr.color.a;
    }

    void Update()
    {
        float flicker = Mathf.PerlinNoise(Time.time * 3f, 0f) * 0.2f; // smooth random
        Color c = sr.color;
        c.a = baseAlpha - 0.1f + flicker; // subtle brightness variation
        sr.color = c;
    }
}