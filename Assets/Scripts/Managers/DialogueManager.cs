using System;
using System.Collections;
using Code.Scripts.EventSystems;
using Singleton;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DialogueManager : SingletonBase<DialogueManager>
{
    public event Action OnDialogueEnded;
    public Func<bool> CanAdvanceDialogue;
    public DialogueSO currentDialogue;
    public int currentDialogueIndex;
    public float delayBetweenCharacters;
    public string currentDialogueText;
    
    private bool _currentlyWriting;
    private Coroutine _currentCoroutine;
    
    private GameObject dialogueBox;
    private bool _canWrite;
    private TMP_Text text;
    private TMP_Text speakerText;
    private TMP_Text promptText;

    protected override void Awake()
    {
        base.Awake();
        CreateDialogueUIProgrammatically();
    }

    private void Start()
    {
        EventManager.Instance.Subscribe(this,(DialogueEvent e) => ToggleUi(e.enabled));
        EventManager.Instance.Subscribe(this,(DialogueEvent e) => SetDialogueSo(e.dialogue));
        
        currentDialogueIndex = 0;
        if (currentDialogue != null)
        {
            SetDialogue();
            _currentCoroutine = StartCoroutine(DialogueCoroutine());
        }
        else
        {
            ToggleUi(false);
        }
    }

    private IEnumerator DialogueCoroutine()
    {
        _canWrite = true;
        foreach (var character in currentDialogueText)
        {
            yield return new WaitForSeconds(delayBetweenCharacters);
            text.text += character;
            _currentlyWriting = true;
        }

        _currentlyWriting = false;

    }

    private void SetDialogue()
    {
        if (currentDialogue == null || currentDialogue.dialogues == null || currentDialogue.dialogues.Length == 0) return;
        
        currentDialogueText = currentDialogue.dialogues[currentDialogueIndex].text;
        text.text = null;

        if (speakerText != null)
        {
            var currentType = currentDialogue.dialogues[currentDialogueIndex].type;
            if (currentType != DialogueType.None)
            {
                speakerText.text = currentType.ToString().ToUpper() + " // TRANSMISSION";
            }
            else
            {
                speakerText.text = "SYSTEM // INCOMING TRANSMISSION";
            }
        }
    }

    private void SetDialogueSo(DialogueSO newDialogue)
    {
        currentDialogue = newDialogue;
    }

    private void IncrementDialogueIndex()
    {
        if (_currentlyWriting)
        {
            StopCoroutine(_currentCoroutine);
            text.text = null;
            text.text = currentDialogueText;
            _currentlyWriting = false;
            return;
        }

        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
        }
        if (currentDialogueIndex + 1 >= currentDialogue.dialogues.Length)
        {
            ToggleUi(false);
            OnDialogueEnded?.Invoke();
            return;
        }
        var newIndex = currentDialogueIndex + 1;
        currentDialogueIndex = newIndex;
        SetDialogue();
        _currentCoroutine = StartCoroutine(DialogueCoroutine());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _canWrite)
        {
            if (CanAdvanceDialogue != null && !CanAdvanceDialogue()) return;
            
            IncrementDialogueIndex();
            _canWrite = true;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _canWrite = !_canWrite;
            ToggleUi(_canWrite);
        }
    }

    public void ToggleUi(bool enabled)
    {
        if (dialogueBox == null) return;
        
        switch (enabled)
        {
            case false :
                _canWrite = false;
                if (text != null) text.text = null;
                dialogueBox.SetActive(false);
                _currentlyWriting = false;
                if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
                break;
            case true :
                currentDialogueIndex = 0;
                SetDialogue();
                dialogueBox.SetActive(true);
                // CRT flicker opening effect
                dialogueBox.transform.localScale = new Vector3(1f, 0.05f, 1f);
                dialogueBox.transform.DOScaleY(1f, 0.2f).SetUpdate(true);
                if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
                _currentCoroutine = StartCoroutine(DialogueCoroutine());
                _canWrite = true;
                break;
        }
    }

    private void CreateDialogueUIProgrammatically()
    {
        GameObject canvasGo = GameObject.Find("HUD Canvas");
        if (canvasGo == null) canvasGo = GameObject.Find("Canvas");
        if (canvasGo == null) canvasGo = FindFirstObjectByType<Canvas>()?.gameObject;

        if (canvasGo == null)
        {
            Debug.LogError("DialogueManager: No Canvas found to attach Dialogue UI!");
            return;
        }

        // Dialogue Box
        dialogueBox = new GameObject("DialoguePanel_Programmatic", typeof(RectTransform), typeof(Image));
        dialogueBox.transform.SetParent(canvasGo.transform, false);

        RectTransform panelRt = dialogueBox.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.15f, 0.05f);
        panelRt.anchorMax = new Vector2(0.85f, 0.28f);
        panelRt.pivot = new Vector2(0.5f, 0f);
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;

        Image panelImg = dialogueBox.GetComponent<Image>();
        panelImg.color = new Color(0.01f, 0.05f, 0.01f, 0.95f);
        panelImg.raycastTarget = false;

        Outline outline = dialogueBox.AddComponent<Outline>();
        outline.effectColor = new Color(0.2f, 0.9f, 0.2f, 0.8f);
        outline.effectDistance = new Vector2(3f, -3f);

        // Grid lines overlay
        GameObject gridGo = new GameObject("GridLines", typeof(RectTransform), typeof(Image));
        gridGo.transform.SetParent(dialogueBox.transform, false);
        RectTransform gridRt = gridGo.GetComponent<RectTransform>();
        gridRt.anchorMin = Vector2.zero;
        gridRt.anchorMax = Vector2.one;
        gridRt.offsetMin = Vector2.zero;
        gridRt.offsetMax = Vector2.zero;
        Image gridImg = gridGo.GetComponent<Image>();
        gridImg.color = new Color(0.1f, 0.25f, 0.1f, 0.05f);
        gridImg.raycastTarget = false;

        // Scanline overlay
        GameObject scanlineGo = new GameObject("Scanline", typeof(RectTransform), typeof(Image));
        scanlineGo.transform.SetParent(dialogueBox.transform, false);
        RectTransform scanlineRt = scanlineGo.GetComponent<RectTransform>();
        scanlineRt.anchorMin = new Vector2(0f, 0.95f);
        scanlineRt.anchorMax = new Vector2(1f, 1f);
        scanlineRt.offsetMin = Vector2.zero;
        scanlineRt.offsetMax = Vector2.zero;
        Image scanlineImg = scanlineGo.GetComponent<Image>();
        scanlineImg.color = new Color(0.2f, 0.9f, 0.2f, 0.08f);
        scanlineImg.raycastTarget = false;

        // Scanline animation
        scanlineRt.anchorMin = new Vector2(0f, 1f);
        scanlineRt.anchorMax = new Vector2(1f, 1.05f);
        scanlineRt.DOAnchorMin(new Vector2(0f, -0.05f), 3f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart).SetUpdate(true);
        scanlineRt.DOAnchorMax(new Vector2(1f, -0.01f), 3f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart).SetUpdate(true);

        // Speaker Label
        GameObject speakerGo = new GameObject("SpeakerText", typeof(RectTransform), typeof(TextMeshProUGUI));
        speakerGo.transform.SetParent(dialogueBox.transform, false);
        RectTransform speakerRt = speakerGo.GetComponent<RectTransform>();
        speakerRt.anchorMin = new Vector2(0f, 0.8f);
        speakerRt.anchorMax = new Vector2(1f, 1f);
        speakerRt.offsetMin = new Vector2(25f, 0f);
        speakerRt.offsetMax = new Vector2(-25f, -5f);

        speakerText = speakerGo.GetComponent<TextMeshProUGUI>();
        speakerText.fontSize = 32;
        speakerText.fontStyle = FontStyles.Bold;
        speakerText.color = new Color(0.3f, 1f, 0.3f, 1f);
        speakerText.alignment = TextAlignmentOptions.MidlineLeft;
        speakerText.raycastTarget = false;

        // Main Text
        GameObject textGo = new GameObject("MainText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(dialogueBox.transform, false);
        RectTransform textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0f, 0.18f);
        textRt.anchorMax = new Vector2(1f, 0.8f);
        textRt.offsetMin = new Vector2(30f, 5f);
        textRt.offsetMax = new Vector2(-30f, -5f);

        text = textGo.GetComponent<TextMeshProUGUI>();
        text.fontSize = 28;
        text.color = new Color(0.2f, 0.9f, 0.2f, 1f);
        text.alignment = TextAlignmentOptions.TopLeft;
        text.enableWordWrapping = true;
        text.text = "";
        text.raycastTarget = false;

        // Continue prompt
        GameObject promptGo = new GameObject("PromptText", typeof(RectTransform), typeof(TextMeshProUGUI));
        promptGo.transform.SetParent(dialogueBox.transform, false);
        RectTransform promptRt = promptGo.GetComponent<RectTransform>();
        promptRt.anchorMin = new Vector2(0.5f, 0f);
        promptRt.anchorMax = new Vector2(1f, 0.18f);
        promptRt.offsetMin = new Vector2(0f, 5f);
        promptRt.offsetMax = new Vector2(-30f, 0f);

        promptText = promptGo.GetComponent<TextMeshProUGUI>();
        promptText.fontSize = 22;
        promptText.fontStyle = FontStyles.Italic;
        promptText.color = new Color(0.2f, 0.9f, 0.2f, 0.6f);
        promptText.alignment = TextAlignmentOptions.MidlineRight;
        promptText.raycastTarget = false;

        StartCoroutine(BlinkPromptCursor());

        dialogueBox.SetActive(false);
    }

    private bool isCurrentlyAtTop = false;
    public void SetPositionToTop(bool top)
    {
        if (dialogueBox == null || isCurrentlyAtTop == top) return;
        isCurrentlyAtTop = top;
        
        RectTransform rt = dialogueBox.GetComponent<RectTransform>();
        Vector2 targetMin = top ? new Vector2(0.15f, 0.72f) : new Vector2(0.15f, 0.05f);
        Vector2 targetMax = top ? new Vector2(0.85f, 0.95f) : new Vector2(0.85f, 0.28f);

        rt.DOComplete();
        rt.DOAnchorMin(targetMin, 0.35f).SetUpdate(true);
        rt.DOAnchorMax(targetMax, 0.35f).SetUpdate(true);
    }

    private IEnumerator BlinkPromptCursor()
    {
        bool showCursor = true;
        while (true)
        {
            if (promptText != null)
            {
                promptText.text = showCursor ? "PRESS SPACE TO CONTINUE [█]" : "PRESS SPACE TO CONTINUE [ ]";
            }
            showCursor = !showCursor;
            yield return new WaitForSecondsRealtime(0.4f);
        }
    }
}
