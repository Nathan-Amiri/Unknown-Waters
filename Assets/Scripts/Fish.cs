using System.Collections;
using UnityEngine;

public class Fish : MonoBehaviour
{
    // PREFAB REFERENCE:
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Rigidbody2D rb;

    [SerializeField] private SpriteRenderer destinationSR;
    [SerializeField] private Transform destination;

    // CONSTANT:
    private readonly float swimSpeed = 2.4f;
    private readonly float pause = 1.8f;

    // DYNAMIC:
    private Vector2 startPosition;
    private Vector2 endPosition;
    private float swimTime;

    private void Awake()
    {
        destinationSR.enabled = false;

        startPosition = transform.position;
        endPosition = destination.position;

        swimTime = Vector2.Distance(startPosition, endPosition) / swimSpeed;
    }

    private void Start()
    {
        StartSwimming();
    }

    private void Update()
    {
        // Sprite flip
        if (rb.linearVelocity.x > 0)
            sr.flipX = false;
        if (rb.linearVelocity.x < 0)
            sr.flipX = true;
    }

    private void StartSwimming()
    {
        StartCoroutine(SwimRoutine());
    }

    private IEnumerator SwimRoutine()
    {
        while (true)
        {
            rb.linearVelocity = swimSpeed * (endPosition - startPosition).normalized;

            yield return new WaitForSeconds(swimTime);

            rb.linearVelocity = Vector2.zero;

            // Snap
            transform.position = endPosition;

            yield return new WaitForSeconds(pause);

            rb.linearVelocity = swimSpeed * (startPosition - endPosition).normalized;

            yield return new WaitForSeconds(swimTime);

            rb.linearVelocity = Vector2.zero;

            // Snap
            transform.position = startPosition;

            yield return new WaitForSeconds(pause);
        }
    }

    public void StopSwimming()
    {
        StopAllCoroutines();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }
}