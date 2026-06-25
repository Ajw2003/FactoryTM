using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;
using Managers;
using Singleton;
using UnityEngine.EventSystems;

public class StoreUiScript: SingletonBase<StoreUiScript>
{
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    
    // UI elements we'll create/find programmatically
    private Image bgImage;
    private Image scanlineOverlay;
    private Image vignetteOverlay;
    
    // Right terminal panel log text
    private TMP_Text terminalLogText;
    private ScrollRect terminalScroll;
    private List<string> activeLogs = new List<string>();
    private bool isTypingLog = false;
    
    // Message Queue for Terminal
    private Queue<string> logQueue = new Queue<string>();
    private bool isProcessingQueue = false;
    
    private Coroutine bootLogsCoroutine;
    private Coroutine processLogQueueCoroutine;
    private Coroutine typeLogLineCoroutine;
    
    // Blinking cursor
    private float cursorTimer = 0f;
    private bool cursorVisible = true;
    private string cursorChar = "█"; // Retro terminal cursor block
    
    private List<List<Button>> storePages = new List<List<Button>>();
    private Dictionary<Button, Vector3> buttonOriginalScales = new Dictionary<Button, Vector3>();
    private Dictionary<Button, TMP_Text> buttonTexts = new Dictionary<Button, TMP_Text>();
    private Dictionary<Button, string> buttonOriginalTexts = new Dictionary<Button, string>();

    // Dynamically created upgrade shop buttons (from UpgradeManager)
    private List<GameObject> dynamicUpgradeButtons = new List<GameObject>();
    

    
    // Pagination fields
    [SerializeField]private int maxItemsPerPage = 3;
    private int currentPageIndex = 0;
    private int totalPages = 1;
    private Button prevPageBtn;
    private Button nextPageBtn;
    private TMP_Text pageIndicatorText;

    public int CurrentPageIndex => currentPageIndex;
    public List<List<Button>> StorePages => storePages;

    // Tabs & Map Grid UI panels
    private GameObject upgradesContainer;
    private GameObject mapGridPanel;
    private Button upgradesTabBtn;
    private Button mapTabBtn;
    private TMP_Text upgradesTabTxt;
    private TMP_Text mapTabTxt;

    public enum StoreTab { Upgrades, Map }
    private StoreTab activeTab = StoreTab.Upgrades;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        SetupTerminalAesthetics();
        CreatePaginationControls();
        RenderPage();
    }
    


    private void OnEnable()
    {
        // Subscribe to upgrade unlock events to dynamically add shop entries
        if (UpgradeManager.HasInstance)
        {
            UpgradeManager.Instance.onUpgradesChanged += OnUpgradesChanged;
            // Catch up on any upgrades that were unlocked while the store was disabled
            OnUpgradesChanged();
        }
        if (CurrencyManager.HasInstance)
        {
            CurrencyManager.Instance.onCurrencyChange += OnCurrencyChange;
        }
    }

    private void OnDisable()
    {
        if (UpgradeManager.HasInstance)
        {
            UpgradeManager.Instance.onUpgradesChanged -= OnUpgradesChanged;
        }
        if (CurrencyManager.HasInstance)
        {
            CurrencyManager.Instance.onCurrencyChange -= OnCurrencyChange;
        }
    }

    private void OnCurrencyChange()
    {
        if (activeTab == StoreTab.Map && mapGridPanel != null && mapGridPanel.activeSelf)
        {
            RefreshMapGrid();
        }
    }

    private void OnUpgradesChanged()
    {
        if (!UpgradeManager.HasInstance) return;
        var shop = UpgradeManager.Instance.activeUpgradesInShop;
        foreach (var upg in shop)
        {
            if (upg == null) continue;
            bool alreadySpawned = dynamicUpgradeButtons.Find(go => go != null && go.name == "UpgradeShopBtn_" + upg.upgradeId) != null;
            if (!alreadySpawned)
            {
                SpawnUpgradeShopButton(upg);
            }
        }
    }

    private void SpawnUpgradeShopButton(UpgradeDefinition def)
    {
        // Create button GameObject
        GameObject btnGo = new GameObject("UpgradeShopBtn_" + def.upgradeId,
            typeof(RectTransform), typeof(Image), typeof(Button), typeof(UpgradeShopItem));
        btnGo.transform.SetParent(upgradesContainer != null ? upgradesContainer.transform : transform, false);

        RectTransform rt = btnGo.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.1f, 0.5f);
        rt.anchorMax = new Vector2(0.45f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, 160f);

        Image img = btnGo.GetComponent<Image>();
        img.color = new Color(0.05f, 0.15f, 0.05f, 0.85f);
        Outline outline = btnGo.AddComponent<Outline>();
        outline.effectColor = new Color(0.1f, 0.8f, 0.1f, 0.5f);
        outline.effectDistance = new Vector2(2f, 2f);

        // Name label
        GameObject nameGo = new GameObject("NameText", typeof(RectTransform), typeof(TextMeshProUGUI));
        nameGo.transform.SetParent(btnGo.transform, false);
        RectTransform nameRt = nameGo.GetComponent<RectTransform>();
        nameRt.anchorMin = new Vector2(0f, 0.70f);
        nameRt.anchorMax = Vector2.one;
        nameRt.offsetMin = new Vector2(10f, 0f);
        nameRt.offsetMax = new Vector2(-10f, 0f);
        TextMeshProUGUI nameTxt = nameGo.GetComponent<TextMeshProUGUI>();
        nameTxt.fontSize = 36;
        nameTxt.fontStyle = FontStyles.Bold;
        nameTxt.color = new Color(0.2f, 0.9f, 0.2f, 1f);
        nameTxt.alignment = TextAlignmentOptions.MidlineLeft;

        // Type label
        GameObject typeGo = new GameObject("TypeText", typeof(RectTransform), typeof(TextMeshProUGUI));
        typeGo.transform.SetParent(btnGo.transform, false);
        RectTransform typeRt = typeGo.GetComponent<RectTransform>();
        typeRt.anchorMin = new Vector2(0f, 0.45f);
        typeRt.anchorMax = new Vector2(1f, 0.70f);
        typeRt.offsetMin = new Vector2(10f, 0f);
        typeRt.offsetMax = new Vector2(-10f, 0f);
        TextMeshProUGUI typeTxt = typeGo.GetComponent<TextMeshProUGUI>();
        typeTxt.fontSize = 32;
        typeTxt.fontStyle = FontStyles.Bold;
        typeTxt.color = new Color(0.15f, 0.75f, 0.15f, 0.9f);
        typeTxt.alignment = TextAlignmentOptions.MidlineLeft;

        // Description label
        GameObject descGo = new GameObject("DescText", typeof(RectTransform), typeof(TextMeshProUGUI));
        descGo.transform.SetParent(btnGo.transform, false);
        RectTransform descRt = descGo.GetComponent<RectTransform>();
        descRt.anchorMin = new Vector2(0f, 0.25f);
        descRt.anchorMax = new Vector2(1f, 0.45f);
        descRt.offsetMin = new Vector2(10f, 0f);
        descRt.offsetMax = new Vector2(-10f, 0f);
        TextMeshProUGUI descTxt = descGo.GetComponent<TextMeshProUGUI>();
        descTxt.fontSize = 30;
        // descTxt.fontStyle = FontStyles.Bold;
        descTxt.color = new Color(0.1f, 0.65f, 0.1f, 0.8f);
        descTxt.alignment = TextAlignmentOptions.TopLeft;
        descTxt.enableWordWrapping = true;
        descTxt.overflowMode = TextOverflowModes.Ellipsis;

        // Price label
        GameObject priceGo = new GameObject("PriceText", typeof(RectTransform), typeof(TextMeshProUGUI));
        priceGo.transform.SetParent(btnGo.transform, false);
        RectTransform priceRt = priceGo.GetComponent<RectTransform>();
        priceRt.anchorMin = Vector2.zero;
        priceRt.anchorMax = new Vector2(1f, 0.25f);
        priceRt.offsetMin = new Vector2(10f, 0f);
        priceRt.offsetMax = new Vector2(-10f, 0f);
        TextMeshProUGUI priceTxt = priceGo.GetComponent<TextMeshProUGUI>();
        priceTxt.fontSize = 34;
        priceTxt.fontStyle = FontStyles.Bold;
        priceTxt.color = new Color(0.2f, 0.8f, 0.2f, 0.85f);
        priceTxt.alignment = TextAlignmentOptions.MidlineLeft;

        // Setup the UpgradeShopItem component
        UpgradeShopItem shopItem = btnGo.GetComponent<UpgradeShopItem>();
        shopItem.Setup(def);

        // Wire purchase button
        Button btn = btnGo.GetComponent<Button>();
        btn.onClick.AddListener(() => {
            shopItem.PurchaseUpgrade();
            LogCommand("EXECUTE_BUY: " + def.upgradeName.ToUpper());
        });

        // Track it and add to the paged list
        dynamicUpgradeButtons.Add(btnGo);
        
        bool added = false;
        foreach (var page in storePages)
        {
            if (page.Count > 0 && page.Count < maxItemsPerPage)
            {
                var firstItem = page[0].GetComponent<UpgradeShopItem>();
                if (firstItem != null && firstItem.GetTypeString() == shopItem.GetTypeString())
                {
                    page.Add(btn);
                    added = true;
                    break;
                }
            }
        }
        
        if (!added)
        {
            var newPage = new List<Button>();
            newPage.Add(btn);
            storePages.Add(newPage);
        }

        buttonOriginalScales[btn] = btnGo.transform.localScale;
        buttonTexts[btn] = nameTxt;
        buttonOriginalTexts[btn] = def.upgradeName;

        AddHoverAnimations(btn);

        // Rebuild pagination
        totalPages = Mathf.Max(1, storePages.Count);
        RenderPage();
    }
    
    private void Update()
    {
        // Blinking terminal cursor
        cursorTimer += Time.unscaledDeltaTime;
        if (cursorTimer >= 0.4f)
        {
            cursorTimer = 0f;
            cursorVisible = !cursorVisible;
            UpdateLogDisplay();
        }
    }
    
    private void SetupTerminalAesthetics()
    {
        // Disable old HUD zone panel if found in the scene
        ZoneUiManager oldZoneUi = FindFirstObjectByType<ZoneUiManager>(FindObjectsInactive.Include);
        if (oldZoneUi != null)
        {
            oldZoneUi.gameObject.SetActive(false);
        }

        // 1. Force the StorePanel to cover the entire screen
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        // 2. Add full screen solid background to obfuscate game scene
        GameObject bgGo = new GameObject("Terminal_Background", typeof(RectTransform), typeof(Image));
        bgGo.transform.SetParent(transform, false);
        bgGo.transform.SetAsFirstSibling();
        
        RectTransform bgRt = bgGo.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        
        bgImage = bgGo.GetComponent<Image>();
        // Pure dark retro CRT black-green tint
        bgImage.color = new Color(0.02f, 0.04f, 0.02f, 1f);
        
        // Create an inner grid line effect
        GameObject gridGo = new GameObject("Terminal_GridLines", typeof(RectTransform), typeof(Image));
        gridGo.transform.SetParent(transform, false);
        gridGo.transform.SetSiblingIndex(1);
        RectTransform gridRt = gridGo.GetComponent<RectTransform>();
        gridRt.anchorMin = Vector2.zero;
        gridRt.anchorMax = Vector2.one;
        gridRt.offsetMin = new Vector2(20f, 20f);
        gridRt.offsetMax = new Vector2(-20f, -20f);
        Image gridImg = gridGo.GetComponent<Image>();
        gridImg.color = new Color(0.1f, 0.25f, 0.1f, 0.05f); // Subtle grid opacity
        
        // 3. Create Scanline overlay
        GameObject scanlineGo = new GameObject("Terminal_Scanlines", typeof(RectTransform), typeof(Image));
        scanlineGo.transform.SetParent(transform, false);
        scanlineGo.transform.SetSiblingIndex(2);
        
        RectTransform scanlineRt = scanlineGo.GetComponent<RectTransform>();
        scanlineRt.anchorMin = new Vector2(0f, 0f);
        scanlineRt.anchorMax = new Vector2(1f, 0.02f); // Thin horizontal band
        scanlineRt.anchoredPosition = Vector2.zero;
        
        scanlineOverlay = scanlineGo.GetComponent<Image>();
        scanlineOverlay.color = new Color(0.1f, 0.8f, 0.1f, 0.07f);
        
        // Tween the scanline down the screen continuously
        scanlineRt.anchorMin = new Vector2(0f, 1f);
        scanlineRt.anchorMax = new Vector2(1f, 1.02f);
        scanlineRt.DOAnchorMin(new Vector2(0f, -0.05f), 4.5f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart).SetUpdate(true);
        scanlineRt.DOAnchorMax(new Vector2(1f, -0.03f), 4.5f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart).SetUpdate(true);
        
        // 4. Vignette overlay for retro CRT curvature shadow
        GameObject vignetteGo = new GameObject("Terminal_Vignette", typeof(RectTransform), typeof(Image));
        vignetteGo.transform.SetParent(transform, false);
        vignetteGo.transform.SetSiblingIndex(3);
        RectTransform vigRt = vignetteGo.GetComponent<RectTransform>();
        vigRt.anchorMin = Vector2.zero;
        vigRt.anchorMax = Vector2.one;
        vigRt.offsetMin = Vector2.zero;
        vigRt.offsetMax = Vector2.zero;
        vignetteOverlay = vignetteGo.GetComponent<Image>();
        vignetteOverlay.raycastTarget = false;
        
        // Let's draw a programmatic radial black gradient for the CRT glass look
        Texture2D vignetteTex = new Texture2D(128, 128);
        for (int y = 0; y < 128; y++)
        {
            for (int x = 0; x < 128; x++)
            {
                float dx = (x - 64f) / 64f;
                float dy = (y - 64f) / 64f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.SmoothStep(0f, 0.75f, dist);
                vignetteTex.SetPixel(x, y, new Color(0f, 0.03f, 0f, alpha));
            }
        }
        vignetteTex.Apply();
        vignetteOverlay.sprite = Sprite.Create(vignetteTex, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));

        // Create container for upgrades page
        upgradesContainer = new GameObject("UpgradesContainer", typeof(RectTransform));
        upgradesContainer.transform.SetParent(transform, false);
        upgradesContainer.transform.SetSiblingIndex(4);
        RectTransform ucRt = upgradesContainer.GetComponent<RectTransform>();
        ucRt.anchorMin = Vector2.zero;
        ucRt.anchorMax = Vector2.one;
        ucRt.offsetMin = Vector2.zero;
        ucRt.offsetMax = Vector2.zero;

        // Create container for map grid
        mapGridPanel = new GameObject("MapGridPanel", typeof(RectTransform));
        mapGridPanel.transform.SetParent(transform, false);
        mapGridPanel.transform.SetSiblingIndex(5);
        RectTransform mgRt = mapGridPanel.GetComponent<RectTransform>();
        mgRt.anchorMin = new Vector2(0.1f, 0.15f);
        mgRt.anchorMax = new Vector2(0.45f, 0.81f);
        mgRt.offsetMin = Vector2.zero;
        mgRt.offsetMax = Vector2.zero;
        mapGridPanel.SetActive(false);

        // Create Tabs Panel
        CreateTabsPanel();
        
        // 5. Layout existing UI elements on the LEFT
        // Destroy existing static buttons since we unify to UpgradeDefinition
        DestroyStaticButtons();
        
        // 6. Create Right Console/Terminal log output
        CreateTerminalConsole();
    }

    private void DestroyStaticButtons()
    {
        var itemBtns = GetComponentsInChildren<UiItemButton>(true);
        foreach(var btn in itemBtns) Destroy(btn.gameObject);

        var zoneBtns = GetComponentsInChildren<UiZoneButton>(true);
        foreach(var btn in zoneBtns) Destroy(btn.gameObject);
    }
    

    
    private void CreateTerminalConsole()
    {
        // Create panel for the console log
        GameObject consolePanel = new GameObject("Terminal_Console_Panel", typeof(RectTransform), typeof(Image));
        consolePanel.transform.SetParent(transform, false);
        consolePanel.transform.SetSiblingIndex(4);
        
        RectTransform consoleRt = consolePanel.GetComponent<RectTransform>();
        consoleRt.anchorMin = new Vector2(0.55f, 0.15f);
        consoleRt.anchorMax = new Vector2(0.96f, 0.81f);
        consoleRt.offsetMin = Vector2.zero;
        consoleRt.offsetMax = Vector2.zero;
        
        Image consoleImg = consolePanel.GetComponent<Image>();
        consoleImg.color = new Color(0.01f, 0.05f, 0.01f, 0.9f);
        Outline outline = consolePanel.AddComponent<Outline>();
        outline.effectColor = new Color(0.1f, 0.8f, 0.1f, 0.7f);
        outline.effectDistance = new Vector2(2f, -2f);
        
        // Header bar
        GameObject headerGo = new GameObject("Console_Header", typeof(RectTransform), typeof(Image));
        headerGo.transform.SetParent(consolePanel.transform, false);
        RectTransform headerRt = headerGo.GetComponent<RectTransform>();
        headerRt.anchorMin = new Vector2(0f, 0.93f);
        headerRt.anchorMax = new Vector2(1f, 1f);
        headerRt.offsetMin = Vector2.zero;
        headerRt.offsetMax = Vector2.zero;
        Image headerImg = headerGo.GetComponent<Image>();
        headerImg.color = new Color(0.1f, 0.3f, 0.1f, 0.6f);
        
        GameObject headerTextGo = new GameObject("Header_Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        headerTextGo.transform.SetParent(headerGo.transform, false);
        RectTransform htRt = headerTextGo.GetComponent<RectTransform>();
        htRt.anchorMin = Vector2.zero;
        htRt.anchorMax = Vector2.one;
        htRt.offsetMin = new Vector2(10f, 0f);
        htRt.offsetMax = Vector2.zero;
        TextMeshProUGUI htTxt = headerTextGo.GetComponent<TextMeshProUGUI>();
        htTxt.text = "SYS_MONITOR // DECK_04";
        htTxt.color = new Color(0.2f, 1f, 0.2f, 1f);
        htTxt.fontSize = 50;
        htTxt.fontStyle = FontStyles.Bold;
        htTxt.alignment = TextAlignmentOptions.Left;
        
        // Scroll view for the text
        GameObject scrollGo = new GameObject("Console_Scroll", typeof(RectTransform), typeof(ScrollRect));
        scrollGo.transform.SetParent(consolePanel.transform, false);
        RectTransform scrollRt = scrollGo.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0f, 0f);
        scrollRt.anchorMax = new Vector2(1f, 0.92f);
        scrollRt.offsetMin = new Vector2(15f, 15f);
        scrollRt.offsetMax = new Vector2(-15f, -5f);
        
        terminalScroll = scrollGo.GetComponent<ScrollRect>();
        terminalScroll.horizontal = false;
        terminalScroll.vertical = true;
        terminalScroll.movementType = ScrollRect.MovementType.Clamped;
        
        // Scroll content
        GameObject contentGo = new GameObject("Console_Content", typeof(RectTransform), typeof(TextMeshProUGUI));
        contentGo.transform.SetParent(scrollGo.transform, false);
        RectTransform contentRt = contentGo.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0f, 0f);
        contentRt.anchorMax = new Vector2(1f, 1f);
        contentRt.offsetMin = Vector2.zero;
        contentRt.offsetMax = Vector2.zero;
        
        terminalLogText = contentGo.GetComponent<TextMeshProUGUI>();
        terminalLogText.color = new Color(0.2f, 1f, 0.2f, 1f);
        terminalLogText.fontSize = 44;
        terminalLogText.alignment = TextAlignmentOptions.BottomLeft;
        terminalLogText.textWrappingMode = TextWrappingModes.Normal;
        terminalLogText.overflowMode = TextOverflowModes.Truncate;
        terminalLogText.text = "";
        
        terminalScroll.content = contentRt;
    }
    
    private void AddHoverAnimations(Button btn)
    {
        EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>() ?? btn.gameObject.AddComponent<EventTrigger>();

        // Hover enter
        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => {
            if (btn.interactable)
            {
                btn.transform.DOScale(buttonOriginalScales[btn] * 1.05f, 0.15f).SetUpdate(true);
            }
            
            if (buttonTexts.ContainsKey(btn))
            {
                string currentText = buttonTexts[btn].text;
                if (!currentText.StartsWith("> "))
                {
                    buttonTexts[btn].text = "> " + currentText + " <";
                }
            }
        });
        trigger.triggers.Add(entryEnter);
        
        // Pointer Exit
        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => {
            btn.transform.DOScale(buttonOriginalScales[btn], 0.15f).SetUpdate(true);
            
            if (buttonTexts.ContainsKey(btn))
            {
                string currentText = buttonTexts[btn].text;
                if (currentText.StartsWith("> ") && currentText.EndsWith(" <"))
                {
                    buttonTexts[btn].text = currentText.Substring(2, currentText.Length - 4);
                }
            }
        });
        trigger.triggers.Add(entryExit);
    }
    
    private Sequence transitionSequence;

    public void OpenStore()
    {
        // Kill existing transitions
        rectTransform.DOComplete();
        canvasGroup.DOComplete();
        if (transitionSequence != null) transitionSequence.Kill();
        
        // 1. Initial State: scale to a thin central line
        rectTransform.localScale = new Vector3(1f, 0.002f, 1f);
        canvasGroup.alpha = 0f;
        gameObject.SetActive(true);
        
        // Reset active tab to Upgrades
        activeTab = StoreTab.Map;
        SwitchTab(StoreTab.Upgrades);
        
        // 2. Play boot animation sequence
        transitionSequence = DOTween.Sequence();
        transitionSequence.Append(canvasGroup.DOFade(1f, 0.12f))
                    .Append(rectTransform.DOScaleY(1f, 0.35f).SetEase(Ease.OutExpo))
                    .AppendCallback(() => {
                        if (!gameObject.activeInHierarchy) return;
                        // Trigger screen glitch/flicker effect
                        PlayFlickerGlitch();
                        // Trigger terminal boot-up printout logs
                        if (bootLogsCoroutine != null) StopCoroutine(bootLogsCoroutine);
                        bootLogsCoroutine = StartCoroutine(PlayBootLogs());
                    });
        
        transitionSequence.SetUpdate(true);
    }
    
    public void CloseStore()
    {
        rectTransform.DOComplete();
        canvasGroup.DOComplete();
        if (transitionSequence != null) transitionSequence.Kill();
        
        // Stop coroutines
        if (bootLogsCoroutine != null)
        {
            StopCoroutine(bootLogsCoroutine);
            bootLogsCoroutine = null;
        }
        if (processLogQueueCoroutine != null)
        {
            StopCoroutine(processLogQueueCoroutine);
            processLogQueueCoroutine = null;
        }
        if (typeLogLineCoroutine != null)
        {
            StopCoroutine(typeLogLineCoroutine);
            typeLogLineCoroutine = null;
        }
        isProcessingQueue = false;
        logQueue.Clear();

        // Play collapse/shut down sequence
        transitionSequence = DOTween.Sequence();
        transitionSequence.Append(rectTransform.DOScaleY(0.003f, 0.25f).SetEase(Ease.InExpo))
                        .Append(canvasGroup.DOFade(0f, 0.1f))
                        .AppendCallback(() => {
                            gameObject.SetActive(false);
                        });
        
        transitionSequence.SetUpdate(true);
    }
    
    private void PlayFlickerGlitch()
    {
        // Simulates CRT flicker
        Sequence flicker = DOTween.Sequence();
        flicker.Append(canvasGroup.DOFade(0.7f, 0.04f))
               .Append(canvasGroup.DOFade(1f, 0.03f))
               .Append(canvasGroup.DOFade(0.85f, 0.05f))
               .Append(canvasGroup.DOFade(1f, 0.04f))
               .SetLoops(2)
               .SetUpdate(true);
    }
    
    private System.Collections.IEnumerator PlayBootLogs()
    {
        activeLogs.Clear();
        terminalLogText.text = "";
        
        yield return new WaitForSecondsRealtime(0.08f);
        yield return StartCoroutine(TypeLogLine("FACTORY INDUSTRIAL INT. DECK 04 v4.81"));
        yield return new WaitForSecondsRealtime(0.12f);
        yield return StartCoroutine(TypeLogLine("CONNECTING LOGS SYSTEM..."));
        yield return new WaitForSecondsRealtime(0.15f);
        yield return StartCoroutine(TypeLogLine("LOCAL NETWORK PROTOCOLS LOADED."));
        yield return new WaitForSecondsRealtime(0.08f);
        yield return StartCoroutine(TypeLogLine("GRID INTERRUPT: OK."));
        yield return new WaitForSecondsRealtime(0.1f);
        yield return StartCoroutine(TypeLogLine("SYS READY. TYPE COMMAND OR CLICK MODULES."));
        yield return new WaitForSecondsRealtime(0.05f);
        yield return StartCoroutine(TypeLogLine("========================================"));
    }
    
    private IEnumerator TypeLogLine(string line)
    {
        isTypingLog = true;
        
        string baseText = string.Join("\n", activeLogs);
        if (activeLogs.Count > 0) baseText += "\n";
        
        float typeSpeed = 0.015f;
        int charactersCount = line.Length;
        
        for (int i = 0; i <= charactersCount; i++)
        {
            string currentLineText = line.Substring(0, i);
            terminalLogText.text = baseText + currentLineText + (cursorVisible ? cursorChar : "");
            
            // Auto scroll to bottom
            Canvas.ForceUpdateCanvases();
            if (terminalScroll != null) terminalScroll.verticalNormalizedPosition = 0f;
            
            yield return new WaitForSecondsRealtime(typeSpeed);
        }
        
        activeLogs.Add(line);
        // Keep logs compact
        if (activeLogs.Count > 25) activeLogs.RemoveAt(0);
        
        isTypingLog = false;
        UpdateLogDisplay();
    }
    
    public void LogCommand(string cmd)
    {
        logQueue.Enqueue(cmd);
        if (!isProcessingQueue)
        {
            if (processLogQueueCoroutine != null) StopCoroutine(processLogQueueCoroutine);
            processLogQueueCoroutine = StartCoroutine(ProcessLogQueue());
        }
    }

    private IEnumerator ProcessLogQueue()
    {
        isProcessingQueue = true;
        while (logQueue.Count > 0)
        {
            string msg = logQueue.Dequeue();
            
            // If it's a purchase command, add a prefix and a suffix response
            if (msg.StartsWith("EXECUTE_BUY:"))
            {
                if (typeLogLineCoroutine != null) StopCoroutine(typeLogLineCoroutine);
                typeLogLineCoroutine = StartCoroutine(TypeLogLine("> " + msg));
                yield return typeLogLineCoroutine;
                
                yield return new WaitForSecondsRealtime(0.05f);
                
                if (typeLogLineCoroutine != null) StopCoroutine(typeLogLineCoroutine);
                typeLogLineCoroutine = StartCoroutine(TypeLogLine("SYS: COMMAND_OK. EXECUTING TRANSACTION..."));
                yield return typeLogLineCoroutine;
            }
            else
            {
                if (typeLogLineCoroutine != null) StopCoroutine(typeLogLineCoroutine);
                typeLogLineCoroutine = StartCoroutine(TypeLogLine(msg));
                yield return typeLogLineCoroutine;
            }
            yield return new WaitForSecondsRealtime(0.02f);
        }
        isProcessingQueue = false;
    }
        private void UpdateLogDisplay()
    {
        if (isTypingLog) return;
        
        string baseText = string.Join("\n", activeLogs);
        if (activeLogs.Count > 0) baseText += "\n";
        terminalLogText.text = baseText + (cursorVisible ? cursorChar : "");
        
        // Keep scroll at bottom
        Canvas.ForceUpdateCanvases();
        if (terminalScroll != null) terminalScroll.verticalNormalizedPosition = 0f;
    }
    
    private void CreatePaginationControls()
    {
        totalPages = Mathf.Max(1, storePages.Count);

        // Container panel for pagination buttons
        GameObject pagPanel = new GameObject("Pagination_Panel", typeof(RectTransform));
        pagPanel.transform.SetParent(upgradesContainer != null ? upgradesContainer.transform : transform, false);
        RectTransform pagRt = pagPanel.GetComponent<RectTransform>();
        
        // Position it at the bottom left area
        pagRt.anchorMin = new Vector2(0.1f, 0.1f);
        pagRt.anchorMax = new Vector2(0.45f, 0.2f);
        pagRt.offsetMin = Vector2.zero;
        pagRt.offsetMax = Vector2.zero;

        // 1. Prev Button
        GameObject prevGo = new GameObject("Prev_Page_Button", typeof(RectTransform), typeof(Image), typeof(Button));
        prevGo.transform.SetParent(pagPanel.transform, false);
        prevPageBtn = prevGo.GetComponent<Button>();
        RectTransform prevRt = prevGo.GetComponent<RectTransform>();
        prevRt.anchorMin = new Vector2(0f, 0f);
        prevRt.anchorMax = new Vector2(0.3f, 1f);
        prevRt.offsetMin = Vector2.zero;
        prevRt.offsetMax = Vector2.zero;
        
        Image prevImg = prevGo.GetComponent<Image>();
        prevImg.color = new Color(0.05f, 0.15f, 0.05f, 0.85f);
        Outline prevOutline = prevGo.AddComponent<Outline>();
        prevOutline.effectColor = new Color(0.1f, 0.8f, 0.1f, 0.5f);
        
        GameObject prevTextGo = new GameObject("Prev_Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        prevTextGo.transform.SetParent(prevGo.transform, false);
        RectTransform ptRt = prevTextGo.GetComponent<RectTransform>();
        ptRt.anchorMin = Vector2.zero;
        ptRt.anchorMax = Vector2.one;
        ptRt.offsetMin = Vector2.zero;
        ptRt.offsetMax = Vector2.zero;
        TextMeshProUGUI ptTxt = prevTextGo.GetComponent<TextMeshProUGUI>();
        ptTxt.text = "<<";
        ptTxt.color = new Color(0.2f, 0.9f, 0.2f, 1f);
        ptTxt.alignment = TextAlignmentOptions.Center;
        ptTxt.fontSize = 44;
        
        prevPageBtn.onClick.AddListener(() => {
            if (currentPageIndex > 0)
            {
                currentPageIndex--;
                PlayPageTransition();
            }
        });

        // 2. Next Button
        GameObject nextGo = new GameObject("Next_Page_Button", typeof(RectTransform), typeof(Image), typeof(Button));
        nextGo.transform.SetParent(pagPanel.transform, false);
        nextPageBtn = nextGo.GetComponent<Button>();
        RectTransform nextRt = nextGo.GetComponent<RectTransform>();
        nextRt.anchorMin = new Vector2(0.7f, 0f);
        nextRt.anchorMax = new Vector2(1f, 1f);
        nextRt.offsetMin = Vector2.zero;
        nextRt.offsetMax = Vector2.zero;
        
        Image nextImg = nextGo.GetComponent<Image>();
        nextImg.color = new Color(0.05f, 0.15f, 0.05f, 0.85f);
        Outline nextOutline = nextGo.AddComponent<Outline>();
        nextOutline.effectColor = new Color(0.1f, 0.8f, 0.1f, 0.5f);
        
        GameObject nextTextGo = new GameObject("Next_Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        nextTextGo.transform.SetParent(nextGo.transform, false);
        RectTransform ntRt = nextTextGo.GetComponent<RectTransform>();
        ntRt.anchorMin = Vector2.zero;
        ntRt.anchorMax = Vector2.one;
        ntRt.offsetMin = Vector2.zero;
        ntRt.offsetMax = Vector2.zero;
        TextMeshProUGUI ntTxt = nextTextGo.GetComponent<TextMeshProUGUI>();
        ntTxt.text = ">>";
        ntTxt.color = new Color(0.2f, 0.9f, 0.2f, 1f);
        ntTxt.alignment = TextAlignmentOptions.Center;
        ntTxt.fontSize = 46;
        
        nextPageBtn.onClick.AddListener(() => {
            if (currentPageIndex < totalPages - 1)
            {
                currentPageIndex++;
                PlayPageTransition();
            }
        });

        // 3. Page Indicator Text
        GameObject indGo = new GameObject("Page_Indicator_Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        indGo.transform.SetParent(pagPanel.transform, false);
        RectTransform indRt = indGo.GetComponent<RectTransform>();
        indRt.anchorMin = new Vector2(0.3f, 0f);
        indRt.anchorMax = new Vector2(0.7f, 1f);
        indRt.offsetMin = Vector2.zero;
        indRt.offsetMax = Vector2.zero;
        
        pageIndicatorText = indGo.GetComponent<TextMeshProUGUI>();
        pageIndicatorText.color = new Color(0.2f, 0.9f, 0.2f, 1f);
        pageIndicatorText.alignment = TextAlignmentOptions.Center;
        pageIndicatorText.fontSize = 44;
        pageIndicatorText.fontStyle = FontStyles.Bold;

        // Add hover triggers to pagination buttons too
        AddHoverToNavButton(prevPageBtn, ptTxt, "<<");
        AddHoverToNavButton(nextPageBtn, ntTxt, ">>");
    }

    private void AddHoverToNavButton(Button btn, TextMeshProUGUI txt, string origText)
    {
        EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = btn.gameObject.AddComponent<EventTrigger>();
        
        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => {
            if (btn.interactable)
            {
                btn.transform.DOScale(1.05f, 0.15f).SetUpdate(true);
                Image btnImg = btn.GetComponent<Image>();
                if (btnImg != null) btnImg.DOColor(new Color(0.1f, 0.35f, 0.1f, 0.95f), 0.15f).SetUpdate(true);
                txt.text = "> " + origText + " <";
                txt.color = new Color(0.5f, 1f, 0.5f, 1f);
            }
        });
        trigger.triggers.Add(entryEnter);
        
        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => {
            btn.transform.DOScale(1f, 0.15f).SetUpdate(true);
            Image btnImg = btn.GetComponent<Image>();
            if (btnImg != null) btnImg.DOColor(new Color(0.05f, 0.15f, 0.05f, 0.85f), 0.15f).SetUpdate(true);
            txt.text = origText;
            txt.color = new Color(0.2f, 0.9f, 0.2f, 1f);
        });
        trigger.triggers.Add(entryExit);
    }

    private void PlayPageTransition()
    {
        LogCommand("NAVIGATED TO PAGE " + (currentPageIndex + 1));
        RenderPage();
    }

    private void RenderPage()
    {
        // 1. Hide all buttons first
        foreach (var page in storePages)
        {
            foreach (var btn in page)
            {
                btn.gameObject.SetActive(false);
            }
        }

        // 2. Reposition and show active page buttons
        float startY = 300f;
        float spacingY = -180f; // Increased spacing to fit 160f tall buttons

        if (currentPageIndex < storePages.Count)
        {
            var activePage = storePages[currentPageIndex];
            for (int i = 0; i < activePage.Count; i++)
            {
                Button btn = activePage[i];
                btn.gameObject.SetActive(true);
                RectTransform btnRt = btn.GetComponent<RectTransform>();
                
                btnRt.anchoredPosition = new Vector2(0f, startY + (i * spacingY));
            }
        }

        // 3. Update Nav Button states and pagination panel visibility
        if (prevPageBtn != null && prevPageBtn.gameObject.transform.parent != null)
        {
            GameObject pagPanel = prevPageBtn.gameObject.transform.parent.gameObject;
            pagPanel.SetActive(totalPages > 1);
        }

        if (prevPageBtn != null)
        {
            prevPageBtn.interactable = (currentPageIndex > 0);
            prevPageBtn.GetComponent<Image>().color = prevPageBtn.interactable ? new Color(0.05f, 0.15f, 0.05f, 0.85f) : new Color(0.02f, 0.05f, 0.02f, 0.5f);
        }
        if (nextPageBtn != null)
        {
            nextPageBtn.interactable = (currentPageIndex < totalPages - 1);
            nextPageBtn.GetComponent<Image>().color = nextPageBtn.interactable ? new Color(0.05f, 0.15f, 0.05f, 0.85f) : new Color(0.02f, 0.05f, 0.02f, 0.5f);
        }

        if (pageIndicatorText != null)
        {
            pageIndicatorText.text = $"PAGE {currentPageIndex + 1:D2} / {totalPages:D2}";
        }
    }

    private void CreateTabsPanel()
    {
        GameObject tabsPanel = new GameObject("TabsPanel", typeof(RectTransform));
        tabsPanel.transform.SetParent(transform, false);
        RectTransform tpRt = tabsPanel.GetComponent<RectTransform>();
        tpRt.anchorMin = new Vector2(0.1f, 0.83f);
        tpRt.anchorMax = new Vector2(0.45f, 0.89f);
        tpRt.offsetMin = Vector2.zero;
        tpRt.offsetMax = Vector2.zero;

        // 1. Upgrades Tab
        GameObject upgGo = new GameObject("UpgradesTabBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        upgGo.transform.SetParent(tabsPanel.transform, false);
        upgradesTabBtn = upgGo.GetComponent<Button>();
        RectTransform upgRt = upgGo.GetComponent<RectTransform>();
        upgRt.anchorMin = new Vector2(0f, 0f);
        upgRt.anchorMax = new Vector2(0.48f, 1f);
        upgRt.offsetMin = Vector2.zero;
        upgRt.offsetMax = Vector2.zero;
        
        Image upgImg = upgGo.GetComponent<Image>();
        upgImg.color = new Color(0.1f, 0.35f, 0.1f, 0.95f); // Active by default
        Outline upgOutline = upgGo.AddComponent<Outline>();
        upgOutline.effectColor = new Color(0.2f, 0.9f, 0.2f, 0.8f);
        upgOutline.effectDistance = new Vector2(2f, 2f);

        GameObject upgTxtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        upgTxtGo.transform.SetParent(upgGo.transform, false);
        RectTransform utRt = upgTxtGo.GetComponent<RectTransform>();
        utRt.anchorMin = Vector2.zero;
        utRt.anchorMax = Vector2.one;
        utRt.offsetMin = Vector2.zero;
        utRt.offsetMax = Vector2.zero;
        upgradesTabTxt = upgTxtGo.GetComponent<TextMeshProUGUI>();
        upgradesTabTxt.text = "UPGRADES SHOP";
        upgradesTabTxt.color = new Color(0.02f, 0.04f, 0.02f, 1f); // Dark text for active tab
        upgradesTabTxt.alignment = TextAlignmentOptions.Center;
        upgradesTabTxt.fontSize = 32;
        upgradesTabTxt.fontStyle = FontStyles.Bold;

        // 2. Map Tab
        GameObject mapGo = new GameObject("MapTabBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        mapGo.transform.SetParent(tabsPanel.transform, false);
        mapTabBtn = mapGo.GetComponent<Button>();
        RectTransform mapRt = mapGo.GetComponent<RectTransform>();
        mapRt.anchorMin = new Vector2(0.52f, 0f);
        mapRt.anchorMax = new Vector2(1f, 1f);
        mapRt.offsetMin = Vector2.zero;
        mapRt.offsetMax = Vector2.zero;

        Image mapImg = mapGo.GetComponent<Image>();
        mapImg.color = new Color(0.05f, 0.15f, 0.05f, 0.85f); // Inactive by default
        Outline mapOutline = mapGo.AddComponent<Outline>();
        mapOutline.effectColor = new Color(0.1f, 0.8f, 0.1f, 0.5f);
        mapOutline.effectDistance = new Vector2(2f, 2f);

        GameObject mapTxtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        mapTxtGo.transform.SetParent(mapGo.transform, false);
        RectTransform mtRt = mapTxtGo.GetComponent<RectTransform>();
        mtRt.anchorMin = Vector2.zero;
        mtRt.anchorMax = Vector2.one;
        mtRt.offsetMin = Vector2.zero;
        mtRt.offsetMax = Vector2.zero;
        mapTabTxt = mapTxtGo.GetComponent<TextMeshProUGUI>();
        mapTabTxt.text = "SATELLITE MAP";
        mapTabTxt.color = new Color(0.2f, 0.9f, 0.2f, 1f); // Bright text for inactive tab
        mapTabTxt.alignment = TextAlignmentOptions.Center;
        mapTabTxt.fontSize = 32;
        mapTabTxt.fontStyle = FontStyles.Bold;

        // Wire click events
        upgradesTabBtn.onClick.AddListener(() => SwitchTab(StoreTab.Upgrades));
        mapTabBtn.onClick.AddListener(() => SwitchTab(StoreTab.Map));

        // Add hover effects for tabs
        AddTabHoverEffect(upgradesTabBtn, upgradesTabTxt, "UPGRADES SHOP", true);
        AddTabHoverEffect(mapTabBtn, mapTabTxt, "SATELLITE MAP", false);
    }

    private void AddTabHoverEffect(Button btn, TMP_Text txt, string origText, bool startsActive)
    {
        EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>() ?? btn.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => {
            if (activeTab == StoreTab.Upgrades && btn == upgradesTabBtn) return;
            if (activeTab == StoreTab.Map && btn == mapTabBtn) return;

            btn.transform.DOScale(1.03f, 0.1f).SetUpdate(true);
            txt.color = new Color(0.5f, 1f, 0.5f, 1f);
        });
        trigger.triggers.Add(entryEnter);

        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => {
            btn.transform.DOScale(1f, 0.1f).SetUpdate(true);
            if (activeTab == StoreTab.Upgrades && btn == upgradesTabBtn)
            {
                txt.color = new Color(0.02f, 0.04f, 0.02f, 1f);
            }
            else if (activeTab == StoreTab.Map && btn == mapTabBtn)
            {
                txt.color = new Color(0.02f, 0.04f, 0.02f, 1f);
            }
            else
            {
                txt.color = new Color(0.2f, 0.9f, 0.2f, 1f);
            }
        });
        trigger.triggers.Add(entryExit);
    }

    public void SwitchTab(StoreTab tab)
    {
        if (activeTab == tab) return;
        activeTab = tab;

        // Visual feedback on tab buttons
        if (activeTab == StoreTab.Upgrades)
        {
            if (upgradesTabBtn != null)
            {
                upgradesTabBtn.GetComponent<Image>().color = new Color(0.1f, 0.35f, 0.1f, 0.95f);
                upgradesTabBtn.GetComponent<Outline>().effectColor = new Color(0.2f, 0.9f, 0.2f, 0.8f);
            }
            if (upgradesTabTxt != null) upgradesTabTxt.color = new Color(0.02f, 0.04f, 0.02f, 1f);

            if (mapTabBtn != null)
            {
                mapTabBtn.GetComponent<Image>().color = new Color(0.05f, 0.15f, 0.05f, 0.85f);
                mapTabBtn.GetComponent<Outline>().effectColor = new Color(0.1f, 0.8f, 0.1f, 0.5f);
            }
            if (mapTabTxt != null) mapTabTxt.color = new Color(0.2f, 0.9f, 0.2f, 1f);

            if (upgradesContainer != null) upgradesContainer.SetActive(true);
            if (mapGridPanel != null) mapGridPanel.SetActive(false);
            LogCommand("NAVIGATED TO UPGRADES SHOP");
        }
        else
        {
            if (mapTabBtn != null)
            {
                mapTabBtn.GetComponent<Image>().color = new Color(0.1f, 0.35f, 0.1f, 0.95f);
                mapTabBtn.GetComponent<Outline>().effectColor = new Color(0.2f, 0.9f, 0.2f, 0.8f);
            }
            if (mapTabTxt != null) mapTabTxt.color = new Color(0.02f, 0.04f, 0.02f, 1f);

            if (upgradesTabBtn != null)
            {
                upgradesTabBtn.GetComponent<Image>().color = new Color(0.05f, 0.15f, 0.05f, 0.85f);
                upgradesTabBtn.GetComponent<Outline>().effectColor = new Color(0.1f, 0.8f, 0.1f, 0.5f);
            }
            if (upgradesTabTxt != null) upgradesTabTxt.color = new Color(0.2f, 0.9f, 0.2f, 1f);

            if (upgradesContainer != null) upgradesContainer.SetActive(false);
            if (mapGridPanel != null) mapGridPanel.SetActive(true);
            LogCommand("INITIALIZING SATELLITE MAP GRID...");
            
            RefreshMapGrid();
        }
    }

    private void RefreshMapGrid()
    {
        if (mapGridPanel == null) return;
        if (ZoneManager.Instance == null) return;

        // Clear old children in mapGridPanel
        foreach (Transform child in mapGridPanel.transform)
        {
            Destroy(child.gameObject);
        }

        Vector2Int centerZone = ZoneManager.Instance.GetCurrentZone();
        float cellMargin = 0.008f;
        float cellSize = 0.2f;

        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col < 5; col++)
            {
                int zoneX = centerZone.x + (col - 2);
                int zoneY = centerZone.y + (2 - row);
                Vector2Int zoneCoords = new Vector2Int(zoneX, zoneY);

                CreateGridCell(col, row, zoneCoords, cellMargin, cellSize);
            }
        }
    }

    private void CreateGridCell(int col, int row, Vector2Int zoneCoords, float margin, float size)
    {
        bool isUnlocked = ZoneManager.Instance.IsZoneUnlocked(zoneCoords);
        bool isCurrent = (zoneCoords == ZoneManager.Instance.GetCurrentZone());
        bool isAdjacent = false;

        // Check if adjacent to ANY unlocked zone
        if (!isUnlocked)
        {
            Vector2Int[] neighbors = new Vector2Int[] {
                zoneCoords + Vector2Int.up,
                zoneCoords + Vector2Int.down,
                zoneCoords + Vector2Int.left,
                zoneCoords + Vector2Int.right
            };
            foreach (var n in neighbors)
            {
                if (ZoneManager.Instance.IsZoneUnlocked(n))
                {
                    isAdjacent = true;
                    break;
                }
            }
        }

        // Create the cell GameObject
        GameObject cellGo = new GameObject($"Cell_{zoneCoords.x}_{zoneCoords.y}", typeof(RectTransform), typeof(Image), typeof(Button));
        cellGo.transform.SetParent(mapGridPanel.transform, false);

        RectTransform rt = cellGo.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(col * size + margin, (4 - row) * size + margin);
        rt.anchorMax = new Vector2((col + 1) * size - margin, (4 - row + 1) * size - margin);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = cellGo.GetComponent<Image>();
        Button btn = cellGo.GetComponent<Button>();
        Outline outline = cellGo.AddComponent<Outline>();
        outline.effectColor = new Color(0.1f, 0.8f, 0.1f, 0.5f);
        outline.effectDistance = new Vector2(2f, 2f);

        // Text display on cell
        GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(cellGo.transform, false);
        RectTransform txtRt = txtGo.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = new Vector2(5f, 5f);
        txtRt.offsetMax = new Vector2(-5f, -5f);
        TextMeshProUGUI txt = txtGo.GetComponent<TextMeshProUGUI>();
        txt.alignment = TextAlignmentOptions.Center;
        txt.textWrappingMode = TextWrappingModes.Normal;
        txt.fontSize = 28;
        txt.fontStyle = FontStyles.Bold;

        if (isUnlocked)
        {
            // Owned / Active zone
            if (isCurrent)
            {
                img.color = new Color(0.15f, 0.45f, 0.15f, 0.95f); // brighter active green for current zone
                outline.effectColor = new Color(0.3f, 1f, 0.3f, 0.9f);
                txt.text = $"ZONE {zoneCoords.x},{zoneCoords.y}\n[ CURRENT ]";
                txt.color = new Color(0.5f, 1f, 0.5f, 1f);
            }
            else
            {
                img.color = new Color(0.05f, 0.25f, 0.05f, 0.85f);
                outline.effectColor = new Color(0.15f, 0.75f, 0.15f, 0.6f);
                txt.text = $"ZONE {zoneCoords.x},{zoneCoords.y}\n[ OWNED ]";
                txt.color = new Color(0.2f, 0.9f, 0.2f, 0.8f);
            }

            // Unlocked zones are owned, clicking does nothing
            btn.interactable = false;
        }
        else if (isAdjacent)
        {
            // Available to purchase
            img.color = new Color(0.08f, 0.18f, 0.08f, 0.85f);
            outline.effectColor = new Color(0.2f, 0.8f, 0.2f, 0.5f);
            float cost = ZoneManager.Instance.GetUnlockCost();
            txt.text = $"ZONE {zoneCoords.x},{zoneCoords.y}\n${cost:F0}";
            txt.color = new Color(0.2f, 0.8f, 0.2f, 0.9f);

            btn.onClick.AddListener(() => {
                PurchaseZone(zoneCoords);
            });

            // Hover animation for purchasable zone
            AddCellHoverEffect(btn, txt, img, outline, $"ZONE {zoneCoords.x},{zoneCoords.y}\n${cost:F0}", true);
        }
        else
        {
            // Obscured / Locked zone (out of range)
            img.color = new Color(0.01f, 0.05f, 0.01f, 0.95f);
            outline.effectColor = new Color(0.05f, 0.2f, 0.05f, 0.3f);
            txt.text = "[ ??? ]\nOFFLINE";
            txt.color = new Color(0.1f, 0.4f, 0.1f, 0.5f);

            btn.onClick.AddListener(() => {
                LogCommand("SYS: CONNECTION ERROR. ZONE OUT OF RANGE.");
            });

            // Hover animation for obscured zone
            AddCellHoverEffect(btn, txt, img, outline, "[ ??? ]\nOFFLINE", false);
        }
    }

    private void PurchaseZone(Vector2Int zoneCoords)
    {
        LogCommand($"EXECUTE_UNLOCK: ZONE {zoneCoords.x},{zoneCoords.y}");
        bool success = ZoneManager.Instance.TryUnlockZone(zoneCoords);
        if (success)
        {
            LogCommand($"SYS: ZONE {zoneCoords.x},{zoneCoords.y} ONLINE.");
            RefreshMapGrid();
        }
        else
        {
            LogCommand("SYS: ERROR. INSUFFICIENT CAPITAL.");
        }
    }

    private void AddCellHoverEffect(Button btn, TextMeshProUGUI txt, Image img, Outline outline, string origText, bool isPurchasable)
    {
        EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>() ?? btn.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => {
            if (!btn.interactable) return;

            btn.transform.DOScale(1.04f, 0.1f).SetUpdate(true);
            if (isPurchasable)
            {
                img.color = new Color(0.12f, 0.3f, 0.12f, 0.95f);
                outline.effectColor = new Color(0.3f, 1f, 0.3f, 0.9f);
                txt.color = new Color(0.5f, 1f, 0.5f, 1f);
            }
            else
            {
                // Obscured cell hover feedback
                txt.text = "[ !!! ]\nBLOCKED";
                txt.color = new Color(0.8f, 0.2f, 0.2f, 0.8f);
                outline.effectColor = new Color(0.6f, 0.1f, 0.1f, 0.5f);
            }
        });
        trigger.triggers.Add(entryEnter);

        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => {
            btn.transform.DOScale(1f, 0.1f).SetUpdate(true);
            if (isPurchasable)
            {
                img.color = new Color(0.08f, 0.18f, 0.08f, 0.85f);
                outline.effectColor = new Color(0.2f, 0.8f, 0.2f, 0.5f);
                txt.color = new Color(0.2f, 0.8f, 0.2f, 0.9f);
            }
            else
            {
                img.color = new Color(0.01f, 0.05f, 0.01f, 0.95f);
                outline.effectColor = new Color(0.05f, 0.2f, 0.05f, 0.3f);
                txt.text = origText;
                txt.color = new Color(0.1f, 0.4f, 0.1f, 0.5f);
            }
        });
        trigger.triggers.Add(entryExit);
    }
}
