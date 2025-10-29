using UnityEngine;

public class FogDepthController : MonoBehaviour
{
    [SerializeField] private FishingMinigame fishingMinigame;
    [SerializeField] private SpriteRenderer fogRenderer;

    private float minAlpha = 210f / 255f; // converted to normalized alpha
    private float maxAlpha = 255f / 255f;
    private float topY = 0f;              // surface
    private float bottomY = -35f;         // max depth (matches FishingMinigame)

    private void Update()
    {
        if (fishingMinigame == null || fogRenderer == null) return;

        float depth = fishingMinigame.transform.position.y;

        // Normalize depth between 0 (top) and 1 (bottom)
        float t = Mathf.InverseLerp(topY, bottomY, depth);
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

        Color c = fogRenderer.color;
        c.a = alpha;
        fogRenderer.color = c;
    }
}
