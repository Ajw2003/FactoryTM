using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
namespace Managers
{
    using System;
    using System.Collections;
    using Code.Scripts.EventSystems;
    using Singleton;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    using DG.Tweening;
    
    public enum DialoguePositionMode
    {
        DefaultBottom,
        AboveHotbar,
        PlacementTop,
        ShopLeftTop
    }
    
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
        
        private bool isDialogueActive = false;
        public bool IsDialogueActive => isDialogueActive;
    
        protected override void Awake()
        {
            base.Awake();
            CreateDialogueUIProgrammatically();
        }
    
        private void Start()
        {
            EventManager.Instance.Subscribe(this, (DialogueEvent e) => HandleDialogueEvent(e));
            
            bool playTutorial = PlayerPrefs.GetInt("PlayTutorial", 1) == 1;
            if (!playTutorial)
            {
                ToggleUi(false);
            }
            else
            {
                ToggleUi(false);
            }
        }
    
        private void HandleDialogueEvent(DialogueEvent e)
        {
            bool playTutorial = PlayerPrefs.GetInt("PlayTutorial", 1) == 1;
            if (!playTutorial)
            {
                ToggleUi(false);
                return;
            }
            currentDialogue = e.dialogue;
            ToggleUi(e.enabled);
        }
    
        private IEnumerator DialogueCoroutine()
        {
            _canWrite = true;
            _currentlyWriting = true;
    
            if (dialogueBox != null && !dialogueBox.activeSelf)
            {
                dialogueBox.SetActive(true);
            }
            if (text != null) text.text = "";
            foreach (var character in currentDialogueText)
            {
                yield return new WaitForSeconds(delayBetweenCharacters);
                if (text != null) text.text += character;
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
                DialogueSO oldDialogue = currentDialogue;
                OnDialogueEnded?.Invoke();
                if (currentDialogue == oldDialogue)
                {
                    ToggleUi(false);
                }
                return;
            }
            var newIndex = currentDialogueIndex + 1;
            currentDialogueIndex = newIndex;
            SetDialogue();
            _currentCoroutine = StartCoroutine(DialogueCoroutine());
        }
    
        private void Update()
        {
            if (PauseManager.IsPaused) return;
    
            bool playTutorial = PlayerPrefs.GetInt("PlayTutorial", 1) == 1;
            if (!playTutorial)
            {
                if (isDialogueActive)
                {
                    ToggleUi(false);
                }
                return;
            }
    
            if (isDialogueActive)
            {
                bool isStoreOpen = StoreUiScript.HasInstance && StoreUiScript.Instance.gameObject.activeInHierarchy;
                DialoguePositionMode targetMode = isStoreOpen ? DialoguePositionMode.ShopLeftTop : DialoguePositionMode.DefaultBottom;
                if (currentPositionMode != targetMode)
                {
                    SetPositionMode(targetMode);
                }
    
                if (dialogueBox != null)
                {
                    if (!dialogueBox.activeSelf)
                    {
                        dialogueBox.SetActive(true);
                        SetPositionMode(currentPositionMode, true);
                        ReplayCurrentDialogue();
                    }
                    else
                    {
                        dialogueBox.transform.SetAsLastSibling();
                    }
                }
            }
    
            if (Input.GetKeyDown(KeyCode.Space) && _canWrite)
            {
                if (CanAdvanceDialogue != null && !CanAdvanceDialogue()) return;
                
                IncrementDialogueIndex();
                _canWrite = true;
            }
        }
    
        public void ToggleUi(bool enabled)
        {
            if (dialogueBox == null) return;
            
            switch (enabled)
            {
                case false :
                    _canWrite = false;
                    isDialogueActive = false;
                    if (text != null) text.text = null;
                    dialogueBox.SetActive(false);
                    _currentlyWriting = false;
                    if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
                    break;
                case true :
                    isDialogueActive = true;
                    currentDialogueIndex = 0;
                    SetDialogue();
                    
                    bool isStoreOpen = StoreUiScript.HasInstance && StoreUiScript.Instance.gameObject.activeInHierarchy;
                    bool wasActive = dialogueBox.activeSelf;
                    
                    if (isStoreOpen)
                    {
                        dialogueBox.SetActive(true);
                        SetPositionMode(DialoguePositionMode.ShopLeftTop, true);
                    }
                    else
                    {
                        dialogueBox.SetActive(true);
                        if (!wasActive)
                        {
                            // Reset position to bottom on initial opening
                            SetPositionMode(DialoguePositionMode.DefaultBottom, true);
                            // CRT flicker opening effect
                            dialogueBox.transform.localScale = new Vector3(1f, 0.05f, 1f);
                            dialogueBox.transform.DOScaleY(1f, 0.2f).SetUpdate(true);
                        }
                    }
                    if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
                    _currentCoroutine = StartCoroutine(DialogueCoroutine());
                    _canWrite = true;
                    break;
            }
        }
    
        public void ReplayCurrentDialogue()
        {
            if (!isDialogueActive || currentDialogue == null) return;
            
            if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
            _currentCoroutine = StartCoroutine(DialogueCoroutine());
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
            panelRt.anchorMin = new Vector2(0.5f, 0f);
            panelRt.anchorMax = new Vector2(0.5f, 0f);
            panelRt.pivot = new Vector2(0.5f, 0f);
            panelRt.anchoredPosition = new Vector2(0f, 20f);
            panelRt.sizeDelta = new Vector2(880f, 180f);
    
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
            text.fontSize = 36;
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
            promptText.fontSize = 32;
            promptText.fontStyle = FontStyles.Italic;
            promptText.color = new Color(0.2f, 0.9f, 0.2f, 0.6f);
            promptText.alignment = TextAlignmentOptions.MidlineRight;
            promptText.raycastTarget = false;
    
            StartCoroutine(BlinkPromptCursor());
    
            dialogueBox.SetActive(false);
        }
    
        private DialoguePositionMode currentPositionMode = DialoguePositionMode.DefaultBottom;
        private bool hasPositionInitialized = false;
    
        public void SetPositionMode(DialoguePositionMode mode, bool force = false)
        {
            if (dialogueBox == null) return;
            if (hasPositionInitialized && currentPositionMode == mode && !force) return;
    
            currentPositionMode = mode;
            hasPositionInitialized = true;
    
            Vector2 targetAnchorMin = new Vector2(0.5f, 0f);
            Vector2 targetAnchorMax = new Vector2(0.5f, 0f);
            Vector2 targetPivot = new Vector2(0.5f, 0f);
            Vector2 targetAnchoredPosition = new Vector2(0f, 20f);
            Vector2 targetSizeDelta = new Vector2(880f, 180f);
    
            switch (mode)
            {
                case DialoguePositionMode.DefaultBottom:
                    targetAnchorMin = new Vector2(0.5f, 0f);
                    targetAnchorMax = new Vector2(0.5f, 0f);
                    targetPivot = new Vector2(0.5f, 0f);
                    targetAnchoredPosition = new Vector2(0f, 20f);
                    targetSizeDelta = new Vector2(880f, 180f);
                    break;
                case DialoguePositionMode.AboveHotbar:
                    targetAnchorMin = new Vector2(0.5f, 0f);
                    targetAnchorMax = new Vector2(0.5f, 0f);
                    targetPivot = new Vector2(0.5f, 0f);
                    targetAnchoredPosition = new Vector2(0f, 160f);
                    targetSizeDelta = new Vector2(880f, 180f);
                    break;
                case DialoguePositionMode.PlacementTop:
                    targetAnchorMin = new Vector2(0.5f, 1f);
                    targetAnchorMax = new Vector2(0.5f, 1f);
                    targetPivot = new Vector2(0.5f, 1f);
                    targetAnchoredPosition = new Vector2(0f, -20f);
                    targetSizeDelta = new Vector2(880f, 180f);
                    break;
                case DialoguePositionMode.ShopLeftTop:
                    targetAnchorMin = new Vector2(0.5f, 0f);
                    targetAnchorMax = new Vector2(0.5f, 0f);
                    targetPivot = new Vector2(0.5f, 0f);
                    targetAnchoredPosition = new Vector2(0f, 20f);
                    targetSizeDelta = new Vector2(880f, 180f);
                    break;
            }
    
            RectTransform rt = dialogueBox.GetComponent<RectTransform>();
            rt.DOComplete();
            rt.DOAnchorMin(targetAnchorMin, 0.35f).SetUpdate(true);
            rt.DOAnchorMax(targetAnchorMax, 0.35f).SetUpdate(true);
            rt.DOPivot(targetPivot, 0.35f).SetUpdate(true);
            rt.DOAnchorPos(targetAnchoredPosition, 0.35f).SetUpdate(true);
            rt.DOSizeDelta(targetSizeDelta, 0.35f).SetUpdate(true);
        }
    
        private IEnumerator BlinkPromptCursor()
        {
            bool showCursor = true;
            while (true)
            {
                if (promptText != null)
                {
                    if (_canWrite)
                    {
                        promptText.text = showCursor ? "PRESS SPACE TO CONTINUE [█]" : "PRESS SPACE TO CONTINUE [ ]";
                    }
                    else
                    {
                        promptText.text = showCursor ? "SYSTEM ACTIVE [█]" : "SYSTEM ACTIVE [ ]";
                    }
                }
                showCursor = !showCursor;
                yield return new WaitForSecondsRealtime(0.4f);
            }
        }
    
        public void DisplayTutorialObjective(DialogueSO dialogueSO, string overrideText)
        {
            if (dialogueSO == null) return;
    
            bool isNewDialogue = (currentDialogue != dialogueSO);
            currentDialogue = dialogueSO;
            currentDialogueIndex = 0;
            currentDialogueText = overrideText;
    
            if (speakerText != null)
            {
                speakerText.text = "MISSION OBJECTIVES //";
            }
    
            if (!isDialogueActive)
            {
                isDialogueActive = true;
                if (dialogueBox != null)
                {
                    dialogueBox.SetActive(true);
                    SetPositionMode(DialoguePositionMode.DefaultBottom, true);
                    
                    // CRT flicker opening effect
                    dialogueBox.transform.localScale = new Vector3(1f, 0.05f, 1f);
                    dialogueBox.transform.DOScaleY(1f, 0.2f).SetUpdate(true);
                }
                _canWrite = false;
                if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
                _currentCoroutine = StartCoroutine(DialogueCoroutine());
            }
            else
            {
                if (isNewDialogue)
                {
                    _canWrite = false;
                    if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
                    _currentCoroutine = StartCoroutine(DialogueCoroutine());
                }
                else if (!_currentlyWriting)
                {
                    if (text != null) text.text = overrideText;
                }
            }
        }
    }
    
}


