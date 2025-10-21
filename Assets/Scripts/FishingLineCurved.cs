using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class FishingLineCurved : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D hookRB;
    public Transform customAnchor; // if null -> top of screen above hook X

    [Header("Rope physics")]
    public bool useJoint = true;
    public bool limitOnly = true;  // leash (max distance only)
    public float ropeLength = 25f;

    [Header("Visual curve")]
    [Range(4, 64)] public int segments = 6;
    public float sagMultiplier = 0.4f;
    public float swayAmplitude = 0.2f;
    public float swayFrequency = 0.2f;
    public float swayPhaseAlong = 1f;

    [Tooltip("How many points near the hook are rigid (no sway/sag). 1 = just the tip.")]
    [Range(1, 3)] public int lockPointsNearHook = 1;


    [Header("Appearance")]
    public float lineWidth = 0.05f;


    [Header("Hook attach offset (world units)")]
    public float hookEndYOffset = 0.35f;

    [Header("Slack gating (reduce bagginess near top)")]
    public float sagStartSlack = 0.4f;  // no sag below this slack
    public float sagFullSlack = 1.2f;  // full sag at/above this
    public float swayStartSlack = 0.4f;  // no sway below this slack
    public float swayFullSlack = 1.2f;  // full sway at/above this

    LineRenderer lr;
    DistanceJoint2D joint;
    Camera cam;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = Mathf.Max(4, segments);
        lr.startWidth = lr.endWidth = Mathf.Max(0.0001f, lineWidth);
        cam = Camera.main;
    }

    void OnValidate()
    {
        if (!lr) lr = GetComponent<LineRenderer>();
        if (lr) lr.positionCount = Mathf.Max(4, segments);
    }

    void Start()
    {
        if (useJoint && hookRB)
        {
            joint = hookRB.gameObject.AddComponent<DistanceJoint2D>();
            joint.autoConfigureConnectedAnchor = false;
            joint.enableCollision = false;
            joint.maxDistanceOnly = limitOnly;
            joint.distance = Mathf.Max(0.01f, ropeLength);
        }
    }

    void LateUpdate()
    {
        if (!hookRB) return;

        Vector2 anchor = customAnchor ? (Vector2)customAnchor.position : GetTopOfScreenAboveHook();
        Vector2 end = (Vector2)hookRB.position + Vector2.up * hookEndYOffset;

        // keep joint at current anchor
        if (joint)
        {
            joint.connectedBody = null;
            joint.connectedAnchor = anchor;
            joint.distance = Mathf.Max(0.01f, ropeLength);
        }

        // slack + gated sag/sway
        float currentDist = Vector2.Distance(anchor, end);
        float slack = Mathf.Max(0f, ropeLength - currentDist);

        float sagScale = Mathf.InverseLerp(sagStartSlack, sagFullSlack, slack);
        float swayScale = Mathf.InverseLerp(swayStartSlack, swayFullSlack, slack);

        float sagAmt = Mathf.Max(0.05f, sagMultiplier * (slack + 0.1f)) * sagScale;

        int last = lr.positionCount - 1;
        float twoPi = Mathf.PI * 2f;

        for (int i = 0; i < lr.positionCount; i++)
        {
            float t = i / (float)last;
            Vector2 p;

            if (i == 0)
            {
                p = anchor; // hard anchor
            }
            else if (i >= lr.positionCount - lockPointsNearHook)
            {
                p = end;    // rigid near hook
            }
            else
            {
                p = Vector2.Lerp(anchor, end, t);

                float along = Mathf.Sin(Mathf.PI * t); // 0..1..0
                p += Vector2.down * (sagAmt * along);

                float sway = swayAmplitude * swayScale *
                             Mathf.Sin(Time.time * twoPi * swayFrequency + t * swayPhaseAlong);
                p.x += sway * along;
            }

            lr.SetPosition(i, p);
        }

        lr.SetPosition(last, end); // ensure exact contact at hook
    }

    Vector2 GetTopOfScreenAboveHook()
    {
        float topY = cam.transform.position.y + cam.orthographicSize;
        return new Vector2(hookRB.position.x, topY);
    }
}
