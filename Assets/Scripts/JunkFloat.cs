using UnityEngine;

public class JunkFloat : MonoBehaviour
{
    [SerializeField] private float bobAmplitude = 0.1f;  // how far it moves up/down
    [SerializeField] private float bobFrequency = 1f;    // how fast it bobs
    [SerializeField] private float swayAmplitude = 0.05f; // optional side-to-side motion
    [SerializeField] private float swayFrequency = 0.5f;  // how fast it sways

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float t = Time.time;
        float bob = Mathf.Sin(t * bobFrequency) * bobAmplitude;
        float sway = Mathf.Sin(t * swayFrequency) * swayAmplitude;

        transform.localPosition = startPos + new Vector3(sway, bob, 0f);
    }
}
