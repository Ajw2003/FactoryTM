using System.Collections;
using Code.Scripts.EventSystems;
using Singleton;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public enum TutorialState
{
    NotStarted,
    IntroDialogue,
    WaitingForBuildings,
    WaitingForFirstDollar,
    CombatDialogue,
    Completed
}

public class TutorialManager : SingletonBase<TutorialManager>
{
    [Header("Dialogue Assets")]
    public DialogueSO introDialogue;
    public DialogueSO combatDialogue;

    [Header("State")]
    public TutorialState currentState = TutorialState.NotStarted;

    private bool hasPlacedMiner = false;
    private bool hasPlacedSeller = false;
    private float initialCurrency = -1;

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
            initialCurrency = CurrencyManager.Instance.currentCurrencyValue;
            CurrencyManager.Instance.onCurrencyChange += HandleCurrencyChange;
        }
        
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnded += HandleDialogueEnded;
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
        }
    }

    private IEnumerator StartTutorialRoutine()
    {
        yield return new WaitForSeconds(1f); // Wait a moment for the scene to settle
        
        // Ensure DayNightManager is paused
        if (DayNightManager.Instance != null)
        {
            DayNightManager.Instance.isTutorialActive = true;
        }

        currentState = TutorialState.IntroDialogue;
        PlayDialogue(introDialogue);
    }

    private void PlayDialogue(DialogueSO dialogue)
    {
        if (dialogue == null)
        {
            Debug.LogWarning("TutorialManager: DialogueSO is null. Skipping dialogue step.");
            HandleDialogueEnded(); // Skip if not assigned
            return;
        }
        
        var eSet = new DialogueEvent { enabled = true, dialogue = dialogue, index = 0 };
        EventManager.Instance.Publish(eSet);
    }

    private void HandleDialogueEnded()
    {
        if (currentState == TutorialState.IntroDialogue)
        {
            currentState = TutorialState.WaitingForBuildings;
            CheckBuildingsCondition();
        }
        else if (currentState == TutorialState.CombatDialogue)
        {
            CompleteTutorial();
        }
    }

    private void HandleBuildingPlaced(Buildings.BuildingData data)
    {
        if (data.type == Buildings.BuildingType.Miner)
        {
            hasPlacedMiner = true;
        }
        else if (data.type == Buildings.BuildingType.Seller)
        {
            hasPlacedSeller = true;
        }
        
        if (currentState == TutorialState.WaitingForBuildings)
        {
            CheckBuildingsCondition();
        }
    }

    private void CheckBuildingsCondition()
    {
        if (hasPlacedMiner && hasPlacedSeller)
        {
            currentState = TutorialState.WaitingForFirstDollar;
        }
    }

    private void HandleCurrencyChange()
    {
        if (currentState == TutorialState.WaitingForFirstDollar)
        {
            if (CurrencyManager.Instance != null && CurrencyManager.Instance.currentCurrencyValue > initialCurrency)
            {
                currentState = TutorialState.CombatDialogue;
                PlayDialogue(combatDialogue);
            }
        }
    }

    private void CompleteTutorial()
    {
        currentState = TutorialState.Completed;
        
        ShowCompletionVisual();

        if (DayNightManager.Instance != null)
        {
            DayNightManager.Instance.CompleteTutorial();
            
            // Force the day to end quickly so the raid starts soon after
            DayNightManager.Instance.timeRemaining = 5f; 
        }
    }

    private void ShowCompletionVisual()
    {
        GameObject canvasGo = GameObject.Find("HUD Canvas");
        if (canvasGo == null) canvasGo = GameObject.Find("Canvas");
        if (canvasGo == null) canvasGo = FindFirstObjectByType<Canvas>()?.gameObject;

        if (canvasGo == null) return;

        // Create UI Panel
        GameObject visualPanel = new GameObject("TutorialCompletionVisual", typeof(RectTransform), typeof(CanvasGroup));
        visualPanel.transform.SetParent(canvasGo.transform, false);

        RectTransform panelRt = visualPanel.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.anchoredPosition = new Vector2(0f, 100f); // Slightly above center
        panelRt.sizeDelta = new Vector2(600f, 150f);

        CanvasGroup cg = visualPanel.GetComponent<CanvasGroup>();
        cg.alpha = 0f;

        // Background
        GameObject bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgGo.transform.SetParent(visualPanel.transform, false);
        RectTransform bgRt = bgGo.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        Image bgImg = bgGo.GetComponent<Image>();
        bgImg.color = new Color(0f, 0.05f, 0f, 0.9f); // CRT dark green
        Outline outline = bgGo.AddComponent<Outline>();
        outline.effectColor = new Color(0.2f, 0.9f, 0.2f, 0.8f);
        outline.effectDistance = new Vector2(2f, -2f);

        // Text
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
        txt.color = new Color(0.2f, 0.9f, 0.2f, 1f); // Glowing green
        txt.alignment = TextAlignmentOptions.Center;
        txt.text = "<color=#32FF32>TUTORIAL COMPLETED</color>\nSYSTEMS UNLOCKED\n<color=#FF3232>WARNING: RAID DETECTED</color>";

        // Animation sequence using DOTween (fades in, flashes color, fades out)
        Sequence seq = DOTween.Sequence();
        seq.Append(cg.DOFade(1f, 0.5f))
           .AppendInterval(0.2f)
           // Flash outline red/green
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
           .AppendInterval(2f) // Let it stay
           .Append(panelRt.DOAnchorPosY(150f, 0.8f).SetEase(Ease.InBack)) // slide up
           .Join(cg.DOFade(0f, 0.8f)) // fade out
           .AppendCallback(() => {
               Destroy(visualPanel);
           });

        seq.SetUpdate(true); // run timescale independent
    }
}
