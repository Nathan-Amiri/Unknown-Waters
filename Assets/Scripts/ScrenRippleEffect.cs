using UnityEngine;

[ExecuteAlways]
public class ScreenRippleEffect : MonoBehaviour
{
    public Texture noiseTex;                   // use your ripple texture (set Wrap=Repeat)
    [Range(0f, 0.1f)] public float strength = 0.02f;
    public float speed = 0.5f;
    public float tiling = 2.0f;

    Material mat;
    void OnEnable()
    {
        var sh = Shader.Find("Hidden/RipplePost");
        if (sh != null) mat = new Material(sh);
        if (noiseTex != null) noiseTex.wrapMode = TextureWrapMode.Repeat;
        Apply();
    }
    void Update() { Apply(); }
    void Apply()
    {
        if (mat == null) return;
        if (noiseTex) mat.SetTexture("_NoiseTex", noiseTex);
        mat.SetFloat("_Strength", strength);
        mat.SetFloat("_Speed", speed);
        mat.SetFloat("_Tiling", tiling);
    }
    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (mat == null) { Graphics.Blit(src, dst); return; }
        Graphics.Blit(src, dst, mat);
    }
}
