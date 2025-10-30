using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

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

    //TILES
    [SerializeField] private Tilemap boundaryFishingTilemap;
    [SerializeField] private Tilemap terrainFishingTilemap;
    [SerializeField] private Tilemap backgroundFishingTilemap;
    [SerializeField] private TileBase[] normalTiles;
    [SerializeField] private TileBase[] redTiles;

    [SerializeField] private SpriteRenderer backgroundColorRenderer;
    [SerializeField] private SpriteRenderer backgroundArtRenderer;

    [SerializeField] private Sprite blueArtSprite;
    [SerializeField] private Sprite redArtSprite;


    //ANIMATION
    [SerializeField] private GameObject lanternSpr;
    [SerializeField] private GameObject lanternGlowPlayer;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private GameObject lanternMask;



    // CONSTANT:
    public float moveSpeed;

    public Vector2 enterHousePosition;
    public Vector2 leaveHousePosition;
    public Vector2 stopFishingPosition;

    // DYNAMIC:
    private Vector2 moveInput;

    // Choice event variables:
    private string choiceEventName;
    [SerializeField] public int currentDay = 1;//1;
    private int MaxDay => fishingDayLayouts != null ? fishingDayLayouts.Count : 0;

    [SerializeField] public int obedience = -10;
    [NonSerialized] public bool hasLantern;
    [NonSerialized] public bool hasFishedToday;
    [NonSerialized] public bool hasFish;

    private bool movingVertically;

    public bool isStunned;

    private bool endingDialogue;

    public bool freeCamera;


    private void Start()
    {
        freeCamera = true;

        lanternSpr?.SetActive(!hasLantern);
        playerAnimator?.SetBool("HasLantern", hasLantern);
        SetLanternGlow(hasLantern, hasLantern ? 0.25f : 0f);

        dialogue.gameObject.SetActive(true);
        Canvas.ForceUpdateCanvases();
        dialogue.gameObject.SetActive(false);

        fade.StartFade(true);

        UpdateTilesForDay();
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
        {
            StartCoroutine(FadeTransition(leaveHousePosition, false));
        }
        else if (choiceEventName == "EnterHouse")
        {
            StartCoroutine(FadeTransition(enterHousePosition, true));
        }
        else if (choiceEventName == "Lantern")
        {
            if (hasLantern)
            {
                // Player already has it — put it back
                hasLantern = false;

                // Re-enable the scene lantern
                lanternSpr?.SetActive(true);

                // Disable glow, animation, and sprite mask
                SetLanternGlow(false, 0f);
                playerAnimator?.SetBool("HasLantern", false);
                lanternMask?.SetActive(false);
            }
            else
            {
                // Player picks it up
                hasLantern = true;

                // Hide the scene lantern
                lanternSpr?.SetActive(false);

                // Enable glow, animation, and sprite mask
                SetLanternGlow(true, 0.25f);
                playerAnimator?.SetBool("HasLantern", true);
                lanternMask?.SetActive(true);
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

        foreach (var go in fishingDayLayouts) go.SetActive(false);

        if (MaxDay > 0)
        {
            int idx = Mathf.Clamp(currentDay - 1, 0, MaxDay - 1); // safe index
            fishingDayLayouts[idx].SetActive(true);
        }
        else
        {
            Debug.LogWarning("No fishingDayLayouts assigned.");
        }

        fishingToggle.SetActive(true);
        overworldToggle.SetActive(false);

        UpdateTilesForDay();
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
        // Transition visuals
        nightOverlay.SetActive(true);
        SetLanternGlow(true, 0.1f);

        // Switch back to overworld
        overworldToggle.SetActive(true);
        fishingToggle.SetActive(false);

        // Ensure player has lantern and correct animation
        hasLantern = true;
        playerAnimator?.SetBool("HasLantern", true);
        playerAnimator?.Play("IdleTree_lantern");
        lanternSpr?.SetActive(false);

        if (currentDay == 5)
        {
            Ending();
            return;
        }

        // Reset player position and controls
        playerRB.transform.position = stopFishingPosition;
        ToggleStun(false);
    }
    private void UpdateTilesForDay()
    {
        bool useRed = currentDay >= 4;

        // Always update the background color/art, even if tiles aren’t configured
        SetFishingBackdrop(useRed);

        // Then (optionally) swap tiles if arrays are valid
        if (normalTiles == null || redTiles == null) return;

        int n = Mathf.Min(normalTiles.Length, redTiles.Length);
        if (n <= 0) return;

        for (int i = 0; i < n; i++)
        {
            var from = useRed ? normalTiles[i] : redTiles[i];
            var to = useRed ? redTiles[i] : normalTiles[i];

            if (from == null || to == null) continue;

            if (boundaryFishingTilemap) boundaryFishingTilemap.SwapTile(from, to);
            if (terrainFishingTilemap) terrainFishingTilemap.SwapTile(from, to);
            if (backgroundFishingTilemap) backgroundFishingTilemap.SwapTile(from, to);
        }
    }


    private void SetFishingBackdrop(bool useRed)
    {
        if (backgroundColorRenderer != null)
        {
            // Blue (days 1–3): #4A57C3
            // Red (days 4–5):  #C34B57
            Color newColor = useRed
                ? new Color32(0xC3, 0x4B, 0x57, 0xFF)
                : new Color32(0x4A, 0x57, 0xC3, 0xFF);

            backgroundColorRenderer.color = newColor;
        }

        if (backgroundArtRenderer != null)
        {
            backgroundArtRenderer.sprite = useRed ? redArtSprite : blueArtSprite;
        }
    }

    private IEnumerator Bedtime()
    {
        // Begin transition to sleep
        ToggleStun(true);
        playerRB.transform.position = new(-50.5f, 2.5f);
        playerSR.enabled = false;
        layInBed.SetActive(true);

        // Fade room light to black
        float alpha = 1f;
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * 3f;
            if (alpha < 0f) alpha = 0f;

            roomSR.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        // Fade out and advance day
        fade.StartFade();
        yield return new WaitForSeconds(0.9f);

        roomSR.color = Color.white;

        // Progress the day safely
        currentDay++;
        if (MaxDay > 0)
            currentDay = Mathf.Clamp(currentDay, 1, MaxDay);

        // Update tiles, environment, and reset state
        UpdateTilesForDay();
        hasFishedToday = false;
        altarFish.SetActive(false);
        nightOverlay.SetActive(false);

        // Wake up WITHOUT lantern equipped
        hasLantern = false;
        lanternSpr?.SetActive(true);                 // place lantern back in scene
        playerAnimator?.SetBool("HasLantern", false);
        SetLanternGlow(false, 0f);                   // no glow on wake

        // Wake up
        layInBed.SetActive(false);
        playerSR.enabled = true;
        ToggleStun(false);
    }

    private IEnumerator FadeTransition(Vector2 targetPosition, bool enteringHouse)
    {
        // Fade out
        fade.StartFade();
        yield return new WaitForSeconds(0.9f);

        // Teleport player after screen goes dark
        playerRB.transform.position = targetPosition;
        freeCamera = enteringHouse;

        // Optional: adjust camera instantly when entering
        if (enteringHouse)
            overworldCamera.transform.position = new Vector3(-50, 3, -10);

        // Fade back in
        fade.StartFade(true);
    }

    // Normalize the player's lantern glow any time it changes
    private void SetLanternGlow(bool on, float alpha)
    {
        if (lanternGlowPlayer != null)
            lanternGlowPlayer.SetActive(on);

        if (lanternFlicker != null)
            lanternFlicker.baseAlpha = alpha;

        // Always keep glow at 2x scale
        if (lanternGlowPlayer != null)
            lanternGlowPlayer.transform.localScale = new Vector3(2f, 2f, 1f);

        // Ensure sprite alpha isn’t fighting the flicker
        var sr = lanternGlowPlayer ? lanternGlowPlayer.GetComponent<SpriteRenderer>() : null;
        if (sr != null)
        {
            var c = sr.color;
            c.a = 1f;
            sr.color = c;
        }
    }

    //Ending visuals
    [SerializeField] private GameObject entitySurface;
    [SerializeField] private GameObject tendrilOne;
    [SerializeField] private GameObject tendrilTwo;

    [SerializeField] private GameObject entityCorpse;

    private void Ending()
    {
        if (obedience > 0)
        {
            Credits.unknownEnding = true;
            StartCoroutine(UnknownEnding());
        }
        else
        {
            Credits.unknownEnding = false;
            StartCoroutine(KnownEnding());
        }
    }




    private IEnumerator MoveObjectUp(GameObject obj, float distance, float duration)
    {
        if (obj == null) yield break;
        Vector3 start = obj.transform.position;
        Vector3 end = start + new Vector3(0f, distance, 0f);
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            obj.transform.position = Vector3.Lerp(start, end, t / duration);
            yield return null;
        }
        obj.transform.position = end;
    }

    private IEnumerator MoveObject(GameObject obj, Vector3 target, float duration)
    {
        if (obj == null) yield break;
        Vector3 start = obj.transform.position;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            obj.transform.position = Vector3.Lerp(start, target, t / duration);
            yield return null;
        }
        obj.transform.position = target;
    }

    private IEnumerator StretchObjectX(GameObject obj, float startX, float endX, float duration)
    {
        if (obj == null) yield break;
        Vector3 scale = obj.transform.localScale;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            scale.x = Mathf.Lerp(startX, endX, k);
            obj.transform.localScale = scale;
            yield return null;
        }
        scale.x = endX;
        obj.transform.localScale = scale;
    }

    private IEnumerator UnknownEnding() // Pre dialogue
    {
        playerRB.transform.position = new(33, 4.4f);

        yield return new WaitForSeconds(1);

        freeCamera = true;
        screenShake.StartShake(3, .5f);
        Debug.Log("entity rises");

        StartCoroutine(MoveObjectUp(entitySurface, 10f, 3f));

        yield return new WaitForSeconds(3);

        freeCamera = true;
        Debug.Log("entity pauses");

        yield return new WaitForSeconds(1.5f);

        endingDialogue = true;

        string message = "You did well, little Fisherman.[p]" +
                        "You gave what was asked. You did not question.[p]" +
                        "The tide obeyed you, because you obeyed me.[p]" +
                        "I have watched your every cast, every trembling breath.[p]" +
                        "Now, the debt is due.[p]" +
                        "Be still.[p]" +
                        "<color=#B22222><i>Let the sea reclaim what it lent.</i></color>";

        TriggerEvent(message);
    }
    private IEnumerator UnknownEnding2() // Post dialogue
    {
        // Tendril gameobject on, moves left
        Debug.Log("tendrils appear");
        StartCoroutine(MoveObject(tendrilOne, tendrilOne.transform.position + new Vector3(-1f, 0f, 0f), 1.6f));

        yield return new WaitForSeconds(1.6f);

        // Tendril gameobject increases speed
        Debug.Log("tendrils speed up");

        tendrilOne.SetActive(false);
        tendrilTwo.SetActive(true);

        yield return new WaitForSeconds(0.1f);

        lanternMask.SetActive(false);
        instantBlack.SetActive(true);
        // Eating sound

        yield return new WaitForSeconds(3f);

        SceneManager.LoadScene(2);
    }


    private IEnumerator FadeOutObject(GameObject obj, float duration)
    {
        if (obj == null) yield break;
        var sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Color start = sr.color;
        Color end = start;
        end.a = 0f;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            sr.color = Color.Lerp(start, end, t / duration);
            yield return null;
        }
        sr.color = end;
    }


    private IEnumerator KnownEnding() // Pre dialogue
    {

        nightOverlay.GetComponent<SpriteRenderer>().color = new Color(0f, 0f, 0f, 100f / 255f);


        entityCorpse.SetActive(true);

        playerRB.transform.position = new(23, -2);

        yield return new WaitForSeconds(1.5f);

        endingDialogue = true;

        string message = "The shore... it burns.[p]" +
                        "The air tastes of iron... and you still breathe.[p]" +
                        "I warned you not to look beneath the light.[p]" +
                        "You tore the silence open... and now I sink alone.[p]" +
                        "Do you feel it, Fisherman?[p]" +
                        "The weight lifting from your chest?[p]" +
                        "<color=#B22222><i>The sea forgets you now.</i></color>[p]" +
                        "<color=#5CB3FF>Go.</color>[p]" +
                        "<color=#B22222><i>Leave me to the deep.</i></color>";

        TriggerEvent(message);
    }
    private IEnumerator KnownEnding2() // Post dialogue
    {

        StartCoroutine(FadeOutObject(entityCorpse, 3f));
        yield return new WaitForSeconds(3f);

        playerCol.enabled = false;

        playerRB.linearVelocity = Vector2.left * moveSpeed;

        while (playerRB.position.x > 9f)
            yield return null;

        // stop, snap X only to the corner, keep current Y (now -2), then head down
        playerRB.linearVelocity = Vector2.zero;
        playerRB.transform.position = new Vector2(9f, playerRB.position.y);
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