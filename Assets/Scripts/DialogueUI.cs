using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Header("Hook up your existing objects")]
    [SerializeField] private GameObject root;            // EventMessage
    [SerializeField] private TMP_Text messageText;       // MessageText
    [SerializeField] private RectTransform yesRow;       // Yes object (its parent container)
    [SerializeField] private TMP_Text yesLabel;          // TMP under Yes
    [SerializeField] private RectTransform noRow;        // No object (its parent container)
    [SerializeField] private TMP_Text noLabel;           // TMP under No
    [SerializeField] private Image cursorYes;
    [SerializeField] private Image cursorNo;      // Small Image that acts as the selector

    [Header("Typing")]
    [SerializeField] private float charsPerSecond = 45f;
    [SerializeField] private float fastForwardMultiplier = 4f;

    [Header("Cursor offset from the left of a row")]
    [SerializeField] private Vector2 cursorOffset = new Vector2(-24, 0);

    public bool IsOpen { get; private set; }

    // runtime state
    private readonly List<string> pages = new();
    private int pageIndex;
    private bool pageFullyShown;
    private Coroutine typeRoutine;

    private bool hasChoices;
    private string[] options;
    private int selectedIndex; // 0 yes, 1 no

    private Action onMessageFinished;
    private Action<int> onChoiceSelected;

    void Awake()
    {
        root.SetActive(false);
        SetChoicesVisible(false);
    }

    void Update()
    {
        if (!IsOpen) return;

        // navigate choices when visible (HORIZONTAL)
        if (hasChoices && pageFullyShown)
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) { selectedIndex = Mathf.Max(0, selectedIndex - 1); UpdateChoiceCursors(); }
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) { selectedIndex = Mathf.Min(1, selectedIndex + 1); UpdateChoiceCursors(); }
        }

        // confirm and paging with Space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!pageFullyShown)
            {
                // finish page instantly
                if (typeRoutine != null) StopCoroutine(typeRoutine);
                messageText.maxVisibleCharacters = int.MaxValue;
                pageFullyShown = true;

                if (hasChoices && pageIndex == pages.Count - 1)
                    SetChoicesVisible(true);
            }
            else
            {
                if (hasChoices && pageIndex == pages.Count - 1)
                {
                    int idx = selectedIndex;
                    var cb = onChoiceSelected; // capture before Close() clears it
                    Close();
                    cb?.Invoke(idx);
                }
                else
                {
                    NextPageOrClose();
                }
            }
        }
    }

    // Public API
    public void ShowMessage(IEnumerable<string> messagePages, Action onFinished)
    {
        Prepare(messagePages);
        hasChoices = false;
        onMessageFinished = onFinished;
        StartTyping();
    }

    public void ShowChoice(IEnumerable<string> messagePages, string[] optionTexts, Action<int> onSelected)
    {
        Prepare(messagePages);
        hasChoices = true;
        options = optionTexts;
        onChoiceSelected = onSelected;

        // push labels
        yesLabel.text = options != null && options.Length > 0 ? options[0] : "Yes";
        noLabel.text = options != null && options.Length > 1 ? options[1] : "No";

        StartTyping();
    }

    // internal helpers
    private void Prepare(IEnumerable<string> messagePages)
    {
        pages.Clear();
        pages.AddRange(messagePages);
        if (pages.Count == 0) pages.Add("");

        pageIndex = 0;
        pageFullyShown = false;

        IsOpen = true;
        root.SetActive(true);

        SetChoicesVisible(false);
    }

    private void StartTyping()
    {
        if (typeRoutine != null) StopCoroutine(typeRoutine);
        typeRoutine = StartCoroutine(TypeRoutine(pages[pageIndex]));
    }

    private IEnumerator TypeRoutine(string fullText)
    {
        messageText.text = fullText;
        messageText.maxVisibleCharacters = 0;
        pageFullyShown = false;

        int total = fullText.Length;

        for (int i = 0; i < total; i++)
        {
            bool fast = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float rate = charsPerSecond * (fast ? fastForwardMultiplier : 1f);
            messageText.maxVisibleCharacters = i + 1;
            yield return new WaitForSeconds(1f / Mathf.Max(1f, rate));
        }

        messageText.maxVisibleCharacters = int.MaxValue;
        pageFullyShown = true;

        if (hasChoices && pageIndex == pages.Count - 1)
            SetChoicesVisible(true);
    }

    private void NextPageOrClose()
    {
        if (pageIndex < pages.Count - 1)
        {
            pageIndex++;
            SetChoicesVisible(false);
            StartTyping();
        }
        else
        {
            var cb = onMessageFinished; // capture before Close() clears it
            Close();
            cb?.Invoke();
        }
    }

    private void Close()
    {
        if (typeRoutine != null) StopCoroutine(typeRoutine);
        typeRoutine = null;

        IsOpen = false;
        root.SetActive(false);
        SetChoicesVisible(false);

        onMessageFinished = null;
        onChoiceSelected = null;
        options = null;
        pages.Clear();
    }

    private void SetChoicesVisible(bool visible)
    {
        yesRow.gameObject.SetActive(visible);
        noRow.gameObject.SetActive(visible);

        if (visible)
        {

            // Always start on Yes
            selectedIndex = 0;
            UpdateChoiceCursors();
        }
        else
        {
            if (cursorYes) cursorYes.gameObject.SetActive(false);
            if (cursorNo) cursorNo.gameObject.SetActive(false);
        }
    }

    private void UpdateChoiceCursors()
    {
        if (cursorYes != null)
            cursorYes.gameObject.SetActive(selectedIndex == 0);

        if (cursorNo != null)
            cursorNo.gameObject.SetActive(selectedIndex == 1);
    }
}
