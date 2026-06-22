using System.Collections;
using Code.Scripts.EventSystems;
using Singleton;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using Buildings;

public class TutorialManager : SingletonBase<TutorialManager>
{
    [Header("Dialogue Assets")]
    public DialogueSO introDialogue;
    public DialogueSO combatDialogue;

    [Header("State (ReadOnly)")]
    [SerializeField] private int currentDialogueIndex = -1;
    [SerializeField] private bool hasPlacedMiner = false;
    [SerializeField] private bool hasPlacedSeller = false;
    [SerializeField] private bool hasPlacedConveyor = false;
    
    private float currencyAtWaitStart = -1f;
    private bool introDialogueActive = false;

    protected override void Awake()
    {
        persistBetweenScenes = false;
        base.Awake();
    }

    private void Start()
    {
        StartCoroutine(StartTutorialRoutine());
        
        if (PlacementManager.Instance != null)
        {
            PlacementManager.Instance.OnBuildingPlaced += HandleBuildingPlaced;
        }

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.onCurrencyChange += HandleCurrencyChange;
        }
        
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnded += HandleDialogueEnded;
            DialogueManager.Instance.CanAdvanceDialogue = CheckCanAdvanceDialogue;
        }
    }

    private void OnDestroy()
    {
        if (PlacementManager.Instance != null)
        {
            PlacementManager.Instance.OnBuildingPlaced -= HandleBuildingPlaced;
        }
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.onCurrencyChange -= HandleCurrencyChange;
        }
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnded -= HandleDialogueEnded;
            DialogueManager.Instance.CanAdvanceDialogue = null;
        }
    }

    private IEnumerator StartTutorialRoutine()
    {
        yield return new WaitForSeconds(1.5f); // Wait a moment for the scene and UI to settle
        
        // Ensure DayNightManager is paused
        if (DayNightManager.Instance != null)
        {
            DayNightManager.Instance.isTutorialActive = true;
        }

        introDialogueActive = true;
        PlayDialogue(introDialogue);
    }

    private void Update()
    {
        if (!introDialogueActive || DialogueManager.Instance == null || DialogueManager.Instance.currentDialogue != introDialogue)
            return;

        currentDialogueIndex = DialogueManager.Instance.currentDialogueIndex;

        // Perform specific update checks for blocking steps
        switch (currentDialogueIndex)
        {
            case 2: // "Press E To open your company device"
                if (UiManager.Instance != null && UiManager.Instance.StorePanel != null && UiManager.Instance.StorePanel.activeSelf)
                {
                    // Switched off the dialogue UI so they can navigate
                    DialogueManager.Instance.ToggleUi(false);
                    AdvanceToDialogueIndex(3);
                }
                break;

            case 3: // "navigate between pages and get your bearings"
                // Turned off currently. Turn back on when they reach the BUILDING page.
                int buildingPage = GetBuildingPage();
                if (buildingPage != -1 && StoreUiScript.Instance != null && StoreUiScript.Instance.CurrentPageIndex == buildingPage)
                {
                    // Turn UI back on to guide them
                    DialogueManager.Instance.ToggleUi(true);
                    AdvanceToDialogueIndex(4);
                }
                break;

            case 4: // "Then when you're ready buy a seller, a miner, and 5 conveyors."
                if (GetInventoryCount(BuildingType.Miner) >= 1 &&
                    GetInventoryCount(BuildingType.Seller) >= 1 &&
                    GetInventoryCount(BuildingType.Conveyor) >= 5)
                {
                    AdvanceToDialogueIndex(5);
                }
                break;

            case 5: // "Now, when you're ready close the company device by pressing escape or E"
                if (UiManager.Instance != null && UiManager.Instance.StorePanel != null && !UiManager.Instance.StorePanel.activeSelf)
                {
                    AdvanceToDialogueIndex(6);
                }
                break;

            case 7: // "Now select your miner, once selected press the R key to rotate..."
                bool isMinerSelected = HotbarManager.Instance != null && 
                                       HotbarManager.Instance.GetSelectedBuilding() != null && 
                                       HotbarManager.Instance.GetSelectedBuilding().type == BuildingType.Miner;
                if (isMinerSelected && Input.GetKeyDown(KeyCode.R))
                {
                    AdvanceToDialogueIndex(8);
                }
                break;

            case 8: // "Left click to place down a miner and then select your conveyor item"
                bool isConveyorSelected = HotbarManager.Instance != null && 
                                         HotbarManager.Instance.GetSelectedBuilding() != null && 
                                         HotbarManager.Instance.GetSelectedBuilding().type == BuildingType.Conveyor;
                if (hasPlacedMiner && isConveyorSelected)
                {
                    AdvanceToDialogueIndex(9);
                }
                break;

            case 9: // "Once selected place down 1-5 conveyors..."
                if (hasPlacedConveyor)
                {
                    AdvanceToDialogueIndex(10);
                }
                break;

            case 10: // "Finally select the seller and place it..."
                // We handle advancing from index 10 when they earn their first dollar.
                // It is checked in HandleCurrencyChange below.
                break;
        }
    }

    private void PlayDialogue(DialogueSO dialogue)
    {
        if (dialogue == null)
        {
            Debug.LogWarning("TutorialManager: DialogueSO is null. Skipping dialogue step.");
            HandleDialogueEnded();
            return;
        }
        
        var eSet = new DialogueEvent { enabled = true, dialogue = dialogue, index = 0 };
        EventManager.Instance.Publish(eSet);
    }

    private bool CheckCanAdvanceDialogue()
    {
        if (!introDialogueActive || DialogueManager.Instance == null || DialogueManager.Instance.currentDialogue != introDialogue)
            return true;

        int index = DialogueManager.Instance.currentDialogueIndex;

        // Block spacebar advancement on task steps so they must perform the action
        if (index == 2 || index == 3 || index == 4 || index == 5 || index == 7 || index == 8 || index == 9 || index == 10)
        {
            return false;
        }

        return true;
    }

    private void AdvanceToDialogueIndex(int targetIndex)
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.currentDialogueIndex = targetIndex;
            // Calls SetDialogue() which pulls text and speaker name dynamically
            // It will also trigger the coroutine to display characters
            var method = typeof(DialogueManager).GetMethod("SetDialogue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null) method.Invoke(DialogueManager.Instance, null);
            
            // Re-trigger dialogue typing coroutine
            var coroutineMethod = typeof(DialogueManager).GetMethod("DialogueCoroutine", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (coroutineMethod != null)
            {
                var coroutine = (IEnumerator)coroutineMethod.Invoke(DialogueManager.Instance, null);
                // Stop any running typewriter coroutine on the DialogueManager first
                var coroutineField = typeof(DialogueManager).GetField("_currentCoroutine", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (coroutineField != null)
                {
                    Coroutine current = (Coroutine)coroutineField.GetValue(DialogueManager.Instance);
                    if (current != null) DialogueManager.Instance.StopCoroutine(current);
                    
                    Coroutine newCoroutine = DialogueManager.Instance.StartCoroutine(coroutine);
                    coroutineField.SetValue(DialogueManager.Instance, newCoroutine);
                }
            }
        }
    }

    private void HandleDialogueEnded()
    {
        if (introDialogueActive)
        {
            introDialogueActive = false;
            // Intro dialogue is fully completed, start combat tutorial
            PlayDialogue(combatDialogue);
        }
        else
        {
            // Combat dialogue completed, start game
            CompleteTutorial();
        }
    }

    private void HandleBuildingPlaced(BuildingData data)
    {
        if (data.type == BuildingType.Miner)
        {
            hasPlacedMiner = true;
        }
        else if (data.type == BuildingType.Seller)
        {
            hasPlacedSeller = true;
            // Record starting currency right when seller is placed (so we can check if it goes up)
            if (CurrencyManager.Instance != null)
            {
                currencyAtWaitStart = CurrencyManager.Instance.currentCurrencyValue;
            }
        }
        else if (data.type == BuildingType.Conveyor)
        {
            hasPlacedConveyor = true;
        }
    }

    private void HandleCurrencyChange()
    {
        if (introDialogueActive && currentDialogueIndex == 10)
        {
            if (CurrencyManager.Instance != null && currencyAtWaitStart >= 0f)
            {
                if (CurrencyManager.Instance.currentCurrencyValue > currencyAtWaitStart)
                {
                    // Ernt first dollar!
                    AdvanceToDialogueIndex(11);
                }
            }
        }
    }

    private int GetBuildingPage()
    {
        if (StoreUiScript.Instance == null || StoreUiScript.Instance.StorePages == null) return -1;
        var pages = StoreUiScript.Instance.StorePages;
        for (int i = 0; i < pages.Count; i++)
        {
            if (pages[i].Count > 0)
            {
                var btn = pages[i][0];
                if (btn != null)
                {
                    var shopItem = btn.GetComponent<UpgradeShopItem>();
                    if (shopItem != null && shopItem.GetTypeString() == "BUILDING")
                    {
                        return i;
                    }
                }
            }
        }
        return -1;
    }

    private int GetInventoryCount(BuildingType type)
    {
        if (InventoryManager.Instance == null || InventoryManager.Instance.items == null) return 0;
        var item = InventoryManager.Instance.items.Find(i => i.data != null && i.data.type == type);
        return item != null ? item.count : 0;
    }

    private void CompleteTutorial()
    {
        ShowCompletionVisual();

        if (DayNightManager.Instance != null)
        {
            DayNightManager.Instance.CompleteTutorial();
            DayNightManager.Instance.timeRemaining = 5f; 
        }
    }

    private void ShowCompletionVisual()
    {
        GameObject canvasGo = GameObject.Find("HUD Canvas");
        if (canvasGo == null) canvasGo = GameObject.Find("Canvas");
        if (canvasGo == null) canvasGo = FindFirstObjectByType<Canvas>()?.gameObject;

        if (canvasGo == null) return;

        GameObject visualPanel = new GameObject("TutorialCompletionVisual", typeof(RectTransform), typeof(CanvasGroup));
        visualPanel.transform.SetParent(canvasGo.transform, false);

        RectTransform panelRt = visualPanel.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.anchoredPosition = new Vector2(0f, 100f);
        panelRt.sizeDelta = new Vector2(600f, 150f);

        CanvasGroup cg = visualPanel.GetComponent<CanvasGroup>();
        cg.alpha = 0f;

        GameObject bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgGo.transform.SetParent(visualPanel.transform, false);
        RectTransform bgRt = bgGo.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        Image bgImg = bgGo.GetComponent<Image>();
        bgImg.color = new Color(0f, 0.05f, 0f, 0.9f);
        Outline outline = bgGo.AddComponent<Outline>();
        outline.effectColor = new Color(0.2f, 0.9f, 0.2f, 0.8f);
        outline.effectDistance = new Vector2(2f, -2f);

        GameObject textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(visualPanel.transform, false);
        RectTransform textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(10f, 10f);
        textRt.offsetMax = new Vector2(-10f, -10f);

        TextMeshProUGUI txt = textGo.GetComponent<TextMeshProUGUI>();
        txt.fontSize = 36;
        txt.fontStyle = FontStyles.Bold;
        txt.color = new Color(0.2f, 0.9f, 0.2f, 1f);
        txt.alignment = TextAlignmentOptions.Center;
        txt.text = "<color=#32FF32>TUTORIAL COMPLETED</color>\nSYSTEMS UNLOCKED\n<color=#FF3232>WARNING: RAID DETECTED</color>";

        Sequence seq = DOTween.Sequence();
        seq.Append(cg.DOFade(1f, 0.5f))
           .AppendInterval(0.2f)
           .AppendCallback(() => {
               outline.effectColor = new Color(1f, 0.2f, 0.2f, 0.9f);
           })
           .AppendInterval(0.3f)
           .AppendCallback(() => {
               outline.effectColor = new Color(0.2f, 0.9f, 0.2f, 0.8f);
           })
           .AppendInterval(0.3f)
           .AppendCallback(() => {
               outline.effectColor = new Color(1f, 0.2f, 0.2f, 0.9f);
           })
           .AppendInterval(0.3f)
           .AppendCallback(() => {
               outline.effectColor = new Color(0.2f, 0.9f, 0.2f, 0.8f);
           })
           .AppendInterval(2.5f)
           .Append(panelRt.DOAnchorPosY(150f, 0.8f).SetEase(Ease.InBack))
           .Join(cg.DOFade(0f, 0.8f))
           .AppendCallback(() => {
               Destroy(visualPanel);
           });

        seq.SetUpdate(true);
    }
}
