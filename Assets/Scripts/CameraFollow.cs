using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10);
    public float topY = -4f;        // camera must not go above this
    public float followStartY = 0f; // when to start following

    bool startFollowing = false;

    void Start()
    {
        // lock start Y and Z
        var p = transform.position;
        p.y = topY;
        p.z = offset.z;
        transform.position = p;
    }

    void LateUpdate()
    {
        if (!player) return;

        if (!startFollowing && player.position.y <= followStartY)
            startFollowing = true;

        if (!startFollowing) return;

        // desired Y based on player + offset
        float desiredY = player.position.y + offset.y;

        // allow moving down, but never above topY
        float clampedY = Mathf.Min(desiredY, topY);

        // smooth only the Y; keep X fixed (vertical follow only)
        float newY = Mathf.Lerp(transform.position.y, clampedY, smoothSpeed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
