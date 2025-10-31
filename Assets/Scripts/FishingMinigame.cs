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

    [SerializeField] private ScreenShake screenShake;

    [SerializeField] private GameObject itGotAway;

    [SerializeField] private GameObject finalEntity;
    private bool finalEntityHooked = false;

    // CONSTANT:
    public int timerLength;

    public float fallSpeed;
    public float reelSpeed;
    private float reelSpeedBase;
    public float moveSpeed;

    private const float surfaceBuffer = 0.05f; // keep hook slightly below surface


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

    private void Awake() { reelSpeedBase = reelSpeed; }

    private void OnEnable()
    {
        StartFishing();
    }

    private void StartFishing()
    {
        reelSpeed = reelSpeedBase;

        finalEntityHooked = false;
        StopAllCoroutines();

        itGotAway.SetActive(false);
        hookRB.constraints = RigidbodyConstraints2D.FreezeRotation;

        tension = 0f;
        fishStruggling = false;
        hookedItem = null;
        fishStruggleRoutine = null;


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

        if (finalEntityHooked)
        {
            screenShake.StopContinuous();
            finalEntityHooked = false;
        }

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
        // keep a tiny gap under the surface instead of exact 0
        if (transform.position.y > -surfaceBuffer)
            transform.position = new(transform.position.x, -surfaceBuffer);
        if (transform.position.y < -yLimit)
            transform.position = new(transform.position.x, -yLimit);

        // Prevent hook from getting stuck at the surface
        if (transform.position.y >= -surfaceBuffer)
        {
            if (!reelInput)
                hookRB.position = new Vector2(hookRB.position.x, -surfaceBuffer - 0.02f); // nudge down
            else
                hookRB.linearVelocity = new Vector2(hookRB.linearVelocity.x, 0f); // stop pushing up
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

        if (tension < 0)
            tension = 0;

        if (tension > 100)
        {
            hookRB.constraints = RigidbodyConstraints2D.FreezeAll;

            tension = 0;

            if (fishStruggleRoutine != null)
            {
                StopCoroutine(fishStruggleRoutine);
                fishStruggleRoutine = null;
            }
            fishStruggling = false;

            if (hookedItem != null)
            {
                Destroy(hookedItem.gameObject);
                hookedItem = null;
            }

            reelSpeed -= 1;

            itGotAway.SetActive(true);
        }

        if (itGotAway.activeSelf && Input.GetButtonDown("Reel"))
        {
            itGotAway.SetActive(false);
            hookRB.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // This code block isn't a typo fyi
        {
            float safeT = Mathf.Max(0.0001f, tension);
            float ratio = 100f / safeT;
            ratio = Mathf.Min(ratio, 100f);
            float offset = tensionMeterBack.rect.height / 2f;
            float barHeight = (tensionMeterBack.rect.height / ratio) - offset;
            tensionMeterBar.transform.localPosition = new Vector2(0f, barHeight);
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
        // When the hook returns to the fisherman with something attached
        if (col.CompareTag("Fisherman") && hookedItem != null)
        {
            bool destroyHooked = true;      // default: clear the hook
            bool applyReelPenalty = true;   // default: reelSpeed -= 1

            // If it's not a fish (junk or final entity)
            if (!hookedItem.TryGetComponent(out Fish fish))
            {
                if (gameManager.currentDay == 5)
                {
                    // Only end if it's the final entity; ignore junk turn-in on Day 5
                    if (hookedItem.gameObject == finalEntity)
                    {
                        StopFishing();
                        return;
                    }

                    // Day 5 junk: no penalties, but DO clear the hook to avoid ghost junk
                    destroyHooked = true;
                    applyReelPenalty = false;
                }
                else
                {
                    // Days 1–4: subtract time for junk
                    timer = Mathf.Max(1, timer - 10);
                    clockText.text = timer.ToString();
                }
            }
            else
            {
                // Fish successfully caught
                gameManager.hasFish = true;
                fishScore += 1;
                fishScoreText.text = "Fish Caught: " + fishScore;

                if (fishStruggleRoutine != null)
                {
                    StopCoroutine(fishStruggleRoutine);
                    fishStruggleRoutine = null;
                }

                gameManager.obedience += 1;
            }

            // Reset fishing state
            fishStruggling = false;
            tension = 0;

            if (destroyHooked && hookedItem != null)
                Destroy(hookedItem.gameObject);

            hookedItem = null;

            if (applyReelPenalty)
                reelSpeed -= 1;
        }

        // Ignore if still holding something
        if (hookedItem != null) return;

        // Hooked junk
        if (col.CompareTag("Junk"))
        {
            hookedItem = col.transform;
            hookedItem.parent = transform;
            hookedItem.localPosition = new Vector3(0f, -2f, 0f);

            reelSpeed += 1;

            // Special case: final entity counts as "Junk" but triggers ending
            if (col.gameObject == finalEntity)
            {
                finalEntityHooked = true;
                screenShake.StartContinuous(0.7f); // adjustable strength
                MusicManager.I?.PlayEntityReelUp(0.8f);
            }
        }
        // Hooked fish
        else if (col.CompareTag("Fish"))
        {
            hookedItem = col.transform;
            Fish fish = col.GetComponent<Fish>();
            fish.StopSwimming();

            fish.isHooked = true; // freeze fish logic

            // Unflip sprite so it faces upward
            var sr = hookedItem.GetComponentInChildren<SpriteRenderer>();
            if (sr)
            {
                sr.flipX = false;
                sr.flipY = false;
            }

            hookedItem.parent = transform;
            hookedItem.localPosition = new Vector3(0f, -1.5f, 0f);
            hookedItem.localRotation = Quaternion.Euler(0f, 0f, -90f);

            fishStruggleRoutine = StartCoroutine(FishStruggle());

            reelSpeed += 1;
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
            screenShake.StartShake(struggleTime, .4f);

            yield return new WaitForSeconds(struggleTime);

            fishStruggling = false;
        }
    }
}