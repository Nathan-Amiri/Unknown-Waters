using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // SCENE REFERENCE:
    [SerializeField] private GameObject overworldToggle;
    [SerializeField] private GameObject fishingToggle;

    [SerializeField] private Transform overworldCamera;
    [SerializeField] private SpriteRenderer playerSR;
    [SerializeField] private Rigidbody2D playerRB;
    [SerializeField] private Collider2D playerCol;

    [SerializeField] private DialogueUI dialogue;
    [SerializeField] private GameObject eventMessageScreen;
    [SerializeField] private TMP_Text eventMessageText;
    [SerializeField] private GameObject yesButton;
    [SerializeField] private GameObject noButton;

    [SerializeField] private GameObject nightOverlay;

    [SerializeField] private GameObject instantBlack;

    [SerializeField] private GameObject altarFish;
    [SerializeField] private GameObject layInBed;

    [SerializeField] private List<GameObject> fishingDayLayouts = new();

    [SerializeField] private LanternFlicker lanternFlicker;
    [SerializeField] private Fade fade;
    [SerializeField] private SpriteRenderer roomSR;

    [SerializeField] private GameObject fishingTrigger;
    [SerializeField] private GameObject pathTrigger;

    [SerializeField] private ScreenShake screenShake;

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
    [NonSerialized] public int currentDay = 5;//1;
    [NonSerialized] public int obedience = -10;
    [NonSerialized] public bool hasLantern;
    [NonSerialized] public bool hasFishedToday;
    [NonSerialized] public bool hasFish;

    private bool movingVertically;

    public bool isStunned;

    private bool endingDialogue;

    private bool freeCamera;


    private void Start()
    {
        freeCamera = true;
        screenShake.StartShake(3, .5f);

        if (lanternSpr != null)
            lanternSpr.SetActive(!hasLantern);

        if (lanternGlowPlayer != null)
            lanternGlowPlayer.SetActive(hasLantern);

        if (playerAnimator != null)
            playerAnimator.SetBool("HasLantern", hasLantern);

        dialogue.gameObject.SetActive(true);
        Canvas.ForceUpdateCanvases();
        dialogue.gameObject.SetActive(false);

        fade.StartFade(true);
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

        if (freeCamera)
            return;

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
        if (endingDialogue)
        {
            if (obedience > 0)
                StartCoroutine(UnknownEnding2());
            else
                StartCoroutine(KnownEnding2());

            return;
        }

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

        if (currentDay == 5) // Turn off triggers that would be set off during the endings
        {
            fishingTrigger.SetActive(false);
            pathTrigger.SetActive(false);
        }

        Invoke(nameof(NoFishTime), .9f);
    }
    private void NoFishTime()
    {
        nightOverlay.SetActive(true);
        lanternFlicker.baseAlpha += .35f;
        lanternFlicker.transform.localScale = new Vector2(1.5f, 1.5f);

        overworldToggle.SetActive(true);
        fishingToggle.SetActive(false);

        if (currentDay == 5)
        {
            Ending();
            return;
        }    

        playerRB.transform.position = stopFishingPosition;

        ToggleStun(false);
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
        //yield return new WaitForSeconds(3);

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




    private void Ending()
    {
        if (obedience > 0)
            StartCoroutine(UnknownEnding());
        else
            StartCoroutine(KnownEnding());
    }
    private IEnumerator UnknownEnding() // Pre dialogue
    {
        playerRB.transform.position = new(33, 4.4f);

        yield return new WaitForSeconds(1);

        freeCamera = true;
        screenShake.StartShake(3, .5f);
        Debug.Log("entity rises");

        yield return new WaitForSeconds(3);

        freeCamera = true;
        Debug.Log("entity pauses");

        yield return new WaitForSeconds(1.5f);

        endingDialogue = true;

        string message = ""; // All end dialogue here in this string using [p]
        TriggerEvent(message);
    }
    private IEnumerator UnknownEnding2() // Post dialogue
    {
        // Tendril gameobject on, moves left
        Debug.Log("tendrils appear");

        yield return new WaitForSeconds(1.6f);

        // Tendril gameobject increases speed
        Debug.Log("tendrils speed up");

        yield return new WaitForSeconds(.4f);

        instantBlack.SetActive(true);
        // Eating sound

        yield return new WaitForSeconds(3f);

        SceneManager.LoadScene(2);
    }
    private IEnumerator KnownEnding() // Pre dialogue
    {
        playerRB.transform.position = new(25, .5f);

        yield return new WaitForSeconds(1.5f);

        endingDialogue = true;

        string message = ""; // All end dialogue here in this string using [p]
        TriggerEvent(message);
    }
    private IEnumerator KnownEnding2() // Post dialogue
    {
        // Entity fades out, or maybe corpse just lies there.
        // I don't see an easy way to fade to day using existing fade code (I didn't make the fade code scalable to things other than the black overlay),
        // plus you'd have to fade the lanter too...if it's super important we can just do it manually here. Or not worry about it.

        playerCol.enabled = false;

        playerRB.linearVelocity = Vector2.left * moveSpeed;

        while (playerRB.position.x > 9)
            yield return null;

        playerRB.transform.position = new(9, .5f);
        playerRB.linearVelocity = Vector2.down * moveSpeed;

        while (playerRB.position.y > -6)
            yield return null;

        freeCamera = true;

        while (playerRB.position.y > -11)
            yield return null;

        // Player currently walks over the tiles below the path opening. The easiest way to fix this would be to put some more tiles down there on a layer above the player

        fade.StartFade();

        yield return new WaitForSeconds(.9f); // Don't change this time!

        SceneManager.LoadScene(2);
    }
}