using System.Collections;
using TMPro;
using UnityEngine;

public class FishingMinigame : MonoBehaviour
{
    // SCENE REFERENCE:
    [SerializeField] private GameManager gameManager;

    [SerializeField] private Rigidbody2D hookRB;

    [SerializeField] private TMP_Text clockText;
    [SerializeField] private RectTransform depthMeterBack;
    [SerializeField] private RectTransform depthMeterBar;
    [SerializeField] private RectTransform tensionMeterBack;
    [SerializeField] private RectTransform tensionMeterBar;

    [SerializeField] private TMP_Text fishScoreText;

    // CONSTANT:
    public int timerLength;

    public float fallSpeed;
    public float reelSpeed;
    public float moveSpeed;

    public float fishStrugglePause; // The duration of the wait in between struggling
    public float fishStruggleDuration; // The duration of the struggle
    public float fishStruggleVariance; // The amount the above two variables can change by

    public float fishStruggleStrength;

    public float tensionIncreaseSpeed;
    public float tensionDecreaseSpeed;

    private readonly float xLimit = 8;
    private readonly float yLimit = 39;

    // DYNAMIC:
    private int timer;

    private float moveInput;
    private bool reelInput;

    private Transform hookedItem;

    private float tension;
    private bool fishStruggling;

    private Coroutine fishStruggleRoutine;

    private int fishScore;



    private void OnEnable()
    {
        StartFishing();
    }

    private void StartFishing()
    {
        fishScore = 0;
        fishScoreText.text = "Fish Caught: " + fishScore;

        hookRB.transform.position = Vector2.zero;

        if (gameManager.currentDay < 5)
            StartCoroutine(ClockRoutine());
        else
            clockText.text = string.Empty;
    }

    private IEnumerator ClockRoutine()
    {
        timer = timerLength;
        clockText.text = timer.ToString();

        while (timer > 0)
        {
            yield return new WaitForSeconds(1);

            timer -= 1;
            clockText.text = timer.ToString();
        }

        StopFishing();
    }

    private void StopFishing()
    {
        StopAllCoroutines();
        tension = 0;
        fishStruggling = false;
        hookedItem = null;
        fishStruggleRoutine = null;

        gameManager.StopFishing();
    }

    private void Update()
    {
        // Input
        reelInput = Input.GetButton("Reel");
        moveInput = Input.GetAxisRaw("Horizontal");

        // Set out of bounds (to prevent clipping bugs)
        if (transform.position.x > xLimit)
            transform.position = new(xLimit, transform.position.y);
        if (transform.position.x < -xLimit)
            transform.position = new(-xLimit, transform.position.y);
        if (transform.position.y > 0)
            transform.position = new(transform.position.x, 0);
        if (transform.position.y < -yLimit)
            transform.position = new(transform.position.x, -yLimit);

        // Prevent hook from getting stuck at the surface
        if (transform.position.y >= 0f)
        {
            if (!reelInput)
                hookRB.position = new Vector2(hookRB.position.x, -0.02f); // small nudge down when not reeling
            else
                hookRB.linearVelocity = new Vector2(hookRB.linearVelocity.x, 0f); // stop pushing into the clamp
        }

        // Depth meter
        if (transform.position.y == 0)
            depthMeterBar.transform.localPosition = new(0, depthMeterBack.rect.height / 2);
        else
        {
            float ratio = yLimit / transform.position.y;
            float offset = depthMeterBack.rect.height / 2;
            float barHeight = (depthMeterBack.rect.height / ratio) + offset;
            depthMeterBar.transform.localPosition = new(0, barHeight);
        }

        // Tension meter
        if (reelInput && fishStruggling)
            tension += tensionIncreaseSpeed * Time.deltaTime;
        else
            tension -= tensionDecreaseSpeed * Time.deltaTime;

        if (tension > 100)
            tension = 100;
        if (tension < 0)
            tension = 0;

        // This code block isn't a typo fyi
        {
            float ratio = 100 / tension;
            if (ratio > 100) ratio = 100;
            float offset = tensionMeterBack.rect.height / 2;
            float barHeight = (tensionMeterBack.rect.height / ratio) - offset;
            tensionMeterBar.transform.localPosition = new(0, barHeight);
        }
    }

    private void FixedUpdate()
    {
        // Hook control
        float ySpeed = reelInput ? reelSpeed : -fallSpeed;
        if (fishStruggling)
            ySpeed -= fishStruggleStrength;

        hookRB.linearVelocity = new Vector2(moveSpeed * moveInput, ySpeed);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Fisherman") && hookedItem != null)
        {
            if (hookedItem.TryGetComponent(out Fish fish))
            {
                gameManager.hasFish = true;
                fishScore += 1;
                fishScoreText.text = "Fish Caught: " + fishScore;
                StopCoroutine(fishStruggleRoutine);
                gameManager.obedience += 1;
            }
            else
            {
                if (gameManager.currentDay == 5)
                {
                    StopFishing();
                    return;
                }

                // If catch junk, subtract time
                timer -= 10;
                if (timer < 1)
                    timer = 1;
                clockText.text = timer.ToString();
            }

            fishStruggling = false;
            tension = 0;

            Destroy(hookedItem.gameObject);
            hookedItem = null;
        }

        if (hookedItem != null) return;

        if (col.CompareTag("Junk"))
        {
            hookedItem = col.transform;

            hookedItem.parent = transform;
            hookedItem.localPosition = new(0, -2f);
        }
        else if (col.CompareTag("Fish"))
        {
            hookedItem = col.transform;
            var fish = col.GetComponent<Fish>();
            fish.StopSwimming();

            fish.isHooked = true;                          // freeze fish logic
            var sr = hookedItem.GetComponentInChildren<SpriteRenderer>();
            if (sr) { sr.flipX = false; sr.flipY = false; } // uncheck Flip X/Y

            hookedItem.parent = transform;
            hookedItem.localPosition = new Vector3(0f, -1.5f, 0f);
            hookedItem.localRotation = Quaternion.Euler(0f, 0f, -90f); // face upward


            fishStruggleRoutine = StartCoroutine(FishStruggle());
        }
    }

    private IEnumerator FishStruggle()
    {
        while (true)
        {
            float waitTime = Random.Range(fishStrugglePause - fishStruggleVariance, fishStrugglePause + fishStruggleVariance);
            yield return new WaitForSeconds(waitTime);

            fishStruggling = true;

            float struggleTime = Random.Range(fishStruggleDuration - fishStruggleVariance, fishStruggleDuration + fishStruggleVariance);
            yield return new WaitForSeconds(struggleTime);

            fishStruggling = false;
        }
    }
}