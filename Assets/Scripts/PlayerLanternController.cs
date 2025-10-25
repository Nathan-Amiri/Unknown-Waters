using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerLanternController : MonoBehaviour
{
    public float moveSpeed = 5f;

    Rigidbody2D rb;
    Animator anim;

    Vector2 input;
    Vector2 facing = Vector2.down;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        input = new Vector2(x, y);
        if (input.sqrMagnitude > 1f) input.Normalize();

        if (input.sqrMagnitude > 0.0001f)
            facing = input;

        anim.SetFloat("MoveX", facing.x);
        anim.SetFloat("MoveY", facing.y);
        anim.SetFloat("Speed", input.magnitude);
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + input * moveSpeed * Time.fixedDeltaTime);
    }
}
