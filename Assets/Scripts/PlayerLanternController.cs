using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerLanternController : MonoBehaviour
{

    Rigidbody2D rb;
    Animator anim;

    Vector2 input;
    Vector2 lastFacing = Vector2.down;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Read actual motion coming from GameManager
        Vector2 vel = rb.linearVelocity;

        // Are we moving?
        bool moving = vel.sqrMagnitude > 0.0001f;

        // Use current velocity for facing, else hold last facing for idle
        Vector2 dir = moving ? vel.normalized : lastFacing;

        // Snap to 4-way so the correct clip plays (no diagonal blending)
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            dir = new Vector2(Mathf.Sign(dir.x), 0f);
        }
        else
        {
            dir = new Vector2(0f, Mathf.Sign(dir.y));
        }

        if (moving)
            lastFacing = dir;

        anim.SetFloat("MoveX", dir.x);
        anim.SetFloat("MoveY", dir.y);
        anim.SetFloat("Speed", moving ? 1f : 0f); // clean on/off for transitions
    }

}
