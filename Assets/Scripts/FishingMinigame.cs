using System.Collections;
using System.Collections.Generic;
using System.Xml.Xsl;
using TMPro;
using UnityEngine;

public class FishingMinigame : MonoBehaviour
{
    // SCENE REFERENCE:
    [SerializeField] private TMP_Text clockText;
    [SerializeField] private RectTransform depthMeterBack;
    [SerializeField] private RectTransform depthMeterBar;

    [SerializeField] private Rigidbody2D hookRB;

    // CONSTANT:
    public float fallSpeed;
    public float reelSpeed;
    public float moveSpeed;

    private readonly float xLimit = 8;
    private readonly float yLimit = 30;

    // DYNAMIC:
    private int timer;

    private float moveInput;
    private bool reelInput;

    private bool hooked;



    private void Start()
    {
        StartFishing();
    }

    private void StartFishing()
    {
        StartCoroutine(ClockRoutine());
    }

    private IEnumerator ClockRoutine()
    {
        timer = 59;
        
        while (timer > 0)
        {
            clockText.text = timer.ToString();
            timer -= 1;

            yield return new WaitForSeconds(1);
        }

        Debug.Log("Time's up!");
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
    }

    private void FixedUpdate()
    {
        // Hook control
        hookRB.linearVelocity = new Vector2(moveSpeed * moveInput, reelInput ? reelSpeed : -fallSpeed);
    }

    private void LateUpdate()
    {
        // Camera follow hook
        Vector3 newPosition = transform.position;
        newPosition.z = -10;
        Camera.main.transform.position = newPosition;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (hooked) return;

        if (col.CompareTag("Junk"))
        {
            hooked = true;

            col.transform.parent = transform;
            col.transform.localPosition = new(0, -2f);
        }
        else if (col.CompareTag("Fish"))
        {
            hooked = true;

            col.GetComponent<Fish>().StopSwimming();

            col.transform.parent = transform;
            col.transform.rotation = Quaternion.Euler(0, 0, -90);
            col.transform.localPosition = new(0, -1.5f);
        }

    }
}