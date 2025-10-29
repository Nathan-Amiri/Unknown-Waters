using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class WaterRippleUI : MonoBehaviour
{
    public float speedX = 0.07f;
    public float speedY = 0.03f;
    public float scale = 1.4f;   // >1 shows more tiles
    public float baseAlpha = 0.28f;
    public float alphaPulse = 0.04f;

    RawImage overlay;
    Vector2 uvOffset;

    void Awake()
    {
        overlay = GetComponent<RawImage>();
        if (overlay.texture != null) overlay.texture.wrapMode = TextureWrapMode.Repeat;
        var c = overlay.color; c.a = baseAlpha; overlay.color = c;
        // Force an obvious starting rect so you can see it move
        overlay.uvRect = new Rect(0, 0, scale, scale);
    }

    void Update()
    {
        uvOffset.x += speedX * Time.deltaTime;
        uvOffset.y += speedY * Time.deltaTime;
        overlay.uvRect = new Rect(uvOffset.x, uvOffset.y, scale, scale);

        var c = overlay.color;
        c.a = baseAlpha + Mathf.Sin(Time.time * 0.7f) * alphaPulse;
        overlay.color = c;
    }
}
