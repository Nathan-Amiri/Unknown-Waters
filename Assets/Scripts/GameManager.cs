using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // SCENE REFERENCE:
    [SerializeField] private GameObject overworldToggle;
    [SerializeField] private GameObject fishingToggle;

    [SerializeField] private Transform overworldCamera;
    [SerializeField] private SpriteRenderer playerSR;
    [SerializeField] private Rigidbody2D playerRB;

    [SerializeField] private DialogueUI dialogue;
    [SerializeField] private GameObject eventMessageScreen;
    [SerializeField] private TMP_Text eventMessageText;
    [SerializeField] private GameObject yesButton;
    [SerializeField] private GameObject noButton;

    [SerializeField] private GameObject nightOverlay;

    [SerializeField] private GameObject altarFish;
    [SerializeField] private GameObject layInBed;

    [SerializeField] private List<GameObject> fishingDayLayouts = new();

    [SerializeField] private LanternFlicker lanternFlicker;
    [SerializeField] private Fade fade;
    [SerializeField] private SpriteRenderer roomSR;

    //ANIMATION
    [SerializeField] private GameObject lanternSpr;
    [SerializeField] private GameObject lanternGlowPlayer;
    [SerializeField] private Animator playerAnimator;


    // CONSTANT:
    public float moveSpeed;

    public Vector2 enterHousePosition;
    public Vector2 leaveHousePosition;
    public Vector2 stopFishingPosition;

    // DYNAMIC:
    private Vector2 moveInput;

    // Choice event variables:
    private string choiceEventName;
    [NonSerialized] public int currentDay = 1;
    [NonSerialized] public int obedience;
    [NonSerialized] public bool hasLantern;
    [NonSerialized] public bool hasFishedToday;
    [NonSerialized] public bool hasFish;

    private bool movingVertically;

    public bool isStunned;


    void Start()
    {
        if (lanternSpr != null)
            lanternSpr.SetActive(!hasLantern);

        if (lanternGlowPlayer != null)
            lanternGlowPlayer.SetActive(hasLantern);

        if (playerAnimator != null)
            playerAnimator.SetBool("HasLantern", hasLantern);

        dialogue.gameObject.SetActive(true);
        Canvas.ForceUpdateCanvases();
        dialogue.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Move input, prevents diagonal movement:
        {
            if (Input.GetButtonDown("Horizontal")) movingVertically = false;
            if (Input.GetButtonDown("Vertical")) movingVertically = true;

            if (Input.GetButtonUp("Horizontal") && Input.GetButton("Vertical")) movingVertically = true;
            if (Input.GetButtonUp("Vertical") && Input.GetButton("Horizontal")) movingVertically = false;

            if (movingVertically)
                moveInput = new(0, Input.GetAxisRaw("Vertical"));
            else
                moveInput = new(Input.GetAxisRaw("Horizontal"), 0);
        }
    }

    private void FixedUpdate()
    {
        // Move
        if (!isStunned)
            playerRB.linearVelocity = moveSpeed * moveInput;

        // Camera follow player
        Vector3 newPosition = playerRB.position;
        newPosition.z = -10;
        overworldCamera.position = newPosition;
    }

    private void ToggleStun(bool stun)
    {
        isStunned = stun;
        playerRB.linearVelocity = Vector2.zero;
    }


    public void TriggerEvent(string eventMessage, string newChoiceEventName = null)
    {
        //eventOpen = true;
        choiceEventName = newChoiceEventName;
        ToggleStun(true);

        string[] pages = eventMessage.Split(new string[] { "[p]" }, StringSplitOptions.None);

        if (string.IsNullOrEmpty(choiceEventName))
        {
            dialogue.ShowMessage(pages, () =>
            {
                EndEvent();   // always un-stun after a plain message
            });
        }
        else
        {
            dialogue.ShowChoice(pages, new[] { "Yes", "No" }, idx =>
            {
                // YesButton logic
                if (idx == 0) YesButton();
                else NoButton();

                //Always ends the event after a selection
                EndEvent();
            });
        }
    }

    public void EndEvent()
    {
        ToggleStun(false);
        choiceEventName = null;
    }
    public void NoButton()
    {
        EndEvent();
    }

    public void YesButton()
    {
        if (choiceEventName == "Fishing")
            StartFishing();
        else if (choiceEventName == "LeaveHouse")
            playerRB.transform.position = leaveHousePosition;
        else if (choiceEventName == "EnterHouse")
            playerRB.transform.position = enterHousePosition;
        else if (choiceEventName == "Lantern")
        {
            if (hasLantern)
            {
                // If player currently has it, put it back
                hasLantern = false;

                // Enable the scene lantern again
                if (lanternSpr != null)
                    lanternSpr.SetActive(true);

                // Turn off the player’s glow
                if (lanternGlowPlayer != null)
                    lanternGlowPlayer.SetActive(false);

                // Switch to non-lantern animation set
                if (playerAnimator != null)
                    playerAnimator.SetBool("HasLantern", false);
            }
            else
            {
                // Player picks it up
                hasLantern = true;

                // Hide the scene lantern
                if (lanternSpr != null)
                    lanternSpr.SetActive(false);

                // Turn on the player’s glow
                if (lanternGlowPlayer != null)
                    lanternGlowPlayer.SetActive(true);

                // Switch to lantern animation set
                if (playerAnimator != null)
                    playerAnimator.SetBool("HasLantern", true);
            }
        }
        else if (choiceEventName == "Bed")
        {
            StartCoroutine(Bedtime());
        }
        else if (choiceEventName == "Altar")
        {
            hasFish = false;
            altarFish.SetActive(true);
        }

        EndEvent();
    }

    private void StartFishing()
    {
        ToggleStun(true);
        fade.StartFade();

        Invoke(nameof(FishTime), .9f);
    }
    private void FishTime()
    {
        hasFishedToday = true;

        ToggleStun(true);

        foreach (GameObject fishingDayLayout in fishingDayLayouts)
            fishingDayLayout.SetActive(false);
        fishingDayLayouts[currentDay - 1].SetActive(true);

        fishingToggle.SetActive(true);
        overworldToggle.SetActive(false);
    }
    public void StopFishing() // Called by FishingMinigame
    {
        fade.StartFade();

        Invoke(nameof(NoFishTime), .9f);
    }
    private void NoFishTime()
    {
        playerRB.transform.position = stopFishingPosition;

        ToggleStun(false);

        nightOverlay.SetActive(true);
        lanternFlicker.baseAlpha += .35f;
        lanternFlicker.transform.localScale = new Vector2(1.5f, 1.5f);

        overworldToggle.SetActive(true);
        fishingToggle.SetActive(false);
    }

    private IEnumerator Bedtime()
    {
        ToggleStun(true);
        playerRB.transform.position = new(-50.5f, 2.5f);
        playerSR.enabled = false;
        layInBed.SetActive(true);

        float alpha = 1;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime * 3;
            if (alpha < 0) alpha = 0;

            Color newColor = Color.white;
            newColor.a = alpha;
            roomSR.color = newColor;

            yield return null;
        }

        // Dream event
        yield return new WaitForSeconds(3);

        fade.StartFade();
        yield return new WaitForSeconds(.9f);

        roomSR.color = Color.white;

        currentDay += 1;
        hasFishedToday = false;
        altarFish.SetActive(false);
        nightOverlay.SetActive(false);
        lanternFlicker.baseAlpha -= .35f;
        lanternFlicker.transform.localScale = new Vector2(1, 1);

        layInBed.SetActive(false);
        playerSR.enabled = true;
        ToggleStun(false);
    }
}