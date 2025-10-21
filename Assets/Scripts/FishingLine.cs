using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class FishingLine : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D hookRB;                 // your hook rigidbody (the one you already have)
    public Transform customAnchor;             // optional: set to rod tip; leave null to use screen top

    [Header("Rope Physics")]
    public bool useJoint = true;               // add a DistanceJoint2D on the hook
    public float ropeLength = 8f;              // rest length
    public bool limitOnly = true;              // if true, acts like a leash (max distance only)
    public float slack = 0f;                   // extra length if you want a bit of slack

    [Header("Visuals")]
    public float lineWidth = 0.05f;

    LineRenderer lr;
    DistanceJoint2D joint;
    Camera cam;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = lr.endWidth = lineWidth;
        cam = Camera.main;
    }

    void Start()
    {
        if (useJoint && hookRB)
        {
            joint = hookRB.gameObject.AddComponent<DistanceJoint2D>();
            joint.autoConfigureConnectedAnchor = false;
            joint.enableCollision = false;
            joint.maxDistanceOnly = limitOnly;

            // initial anchor and distance
            Vector2 anchor = GetAnchorWorld();
            joint.connectedBody = null;              // connect to a fixed world point
            joint.connectedAnchor = anchor;

            // set rope length
            float d = Vector2.Distance(anchor, hookRB.position) + slack;
            joint.distance = (ropeLength > 0f) ? ropeLength : d;
        }
    }

    void LateUpdate()
    {
        if (!hookRB) return;

        Vector2 anchor = GetAnchorWorld();

        // keep the joint anchored to the top-of-screen point
        if (joint)
        {
            joint.connectedAnchor = anchor;

            // ensure the rope can extend to the configured length
            if (limitOnly)
            {
                // acts like a leash: hook can be closer than this but not farther
                joint.distance = ropeLength;
            }
            else
            {
                // fixed-length rope (optional): keep constant length + slack
                float d = Vector2.Distance(anchor, hookRB.position) + slack;
                joint.distance = Mathf.Max(0.01f, d);
            }
        }

        // draw the line
        lr.SetPosition(0, anchor);
        lr.SetPosition(1, hookRB.position);
    }

    Vector2 GetAnchorWorld()
    {
        if (customAnchor) return customAnchor.position;

        // Anchor at TOP of current screen, directly above hook’s X (so it always hangs “down”)
        // Top-of-screen Y in world for orthographic camera:
        float topY = cam.transform.position.y + cam.orthographicSize;
        return new Vector2(hookRB.position.x, topY);
    }
}
