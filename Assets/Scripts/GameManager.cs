using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // SCENE REFERENCE:
    [SerializeField] private GameObject overworldToggle;
    [SerializeField] private GameObject fishingToggle;

    [SerializeField] private Transform overworldCamera;
    [SerializeField] private Rigidbody2D playerRB;

    [SerializeField] private DialogueUI dialogue;
    [SerializeField] private GameObject eventMessageScreen;
    [SerializeField] private TMP_Text eventMessageText;
    [SerializeField] private GameObject yesButton;
    [SerializeField] private GameObject noButton;

    [SerializeField] private GameObject nightOverlay;

    [SerializeField] private GameObject altarFish;

    [SerializeField] private List<GameObject> fishingDayLayouts = new();

    [SerializeField] private LanternFlicker lanternFlicker;

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

    //private bool eventOpen = false;


    void Start()
    {
        if (lanternSpr != null)
            lanternSpr.SetActive(!hasLantern);

        if (lanternGlowPlayer != null)
            lanternGlowPlayer.SetActive(hasLantern);

        if (playerAnimator != null)
            playerAnimator.SetBool("HasLantern", hasLantern);
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
                // Run your existing logic
                if (idx == 0) YesButton();
                else NoButton();

                // Safety net: ALWAYS end the event after a selection
                EndEvent();
            });
        }
    }

    public void EndEvent()
    {
        //eventOpen = false;
        ToggleStun(false);                // <- re-enable player input here
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
            // CHANGE TO DAY LIGHTING

            currentDay += 1;
            hasFishedToday = false;
            altarFish.SetActive(false);
            nightOverlay.SetActive(false);
            lanternFlicker.baseAlpha -= .35f;
            lanternFlicker.transform.localScale = new Vector2(1, 1);
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
        playerRB.transform.position = stopFishingPosition;

        ToggleStun(false);

        nightOverlay.SetActive(true);
        lanternFlicker.baseAlpha += .35f;
        lanternFlicker.transform.localScale = new Vector2(1.5f, 1.5f);

        overworldToggle.SetActive(true);
        fishingToggle.SetActive(false);
    }
}