using System.Collections;
using System.Collections.Generic;
using Code.Scripts.EventSystems;
using Singleton;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using Buildings;
using Managers;

public enum TutorialState
{
    NotStarted,
    BriefIntro,             // Land, walk next to and press E to open IDT/DCT
    MineCoalManually,       // Hover coal deposit & press E to mine
    FuelDCT,                // Walk near DCT, press E to open UI and input coal
    SellOtherOres,          // Mine other resources, sell to IDT until $50
    BuyFirstWeapon,         // Open store, buy first weapon (others lock)
    BuyAmmoHealth,          // Purchase ammo as well as health packs
    DestroyEnemyOutpost,    // North-East outpost, destroy all structures
    BuyMinerConveyors,      // Open store, buy miner and 10 conveyors
    SetupAutomation,        // Place miner, route conveyors, sell 1 item automatically
    EarnAndExpand,          // Earn and expand until night
    DefendFirstRaid,        // Defend against first raid
    Completed
}

public class TutorialManager : SingletonBase<TutorialManager>
{
    [Header("Dialogue Assets (Bypassed)")]
    public DialogueSO introDialogue;
    public DialogueSO combatDialogue;

    [Header("State (ReadOnly)")]
    public TutorialState currentState = TutorialState.NotStarted;
    [SerializeField] private int coalFedCount = 0;
    [SerializeField] private bool hasFiredWeapon = false;
    [SerializeField] private bool hasDodgeRolled = false;
    private bool isSubscribed = false;

    // Progression variables
    [SerializeField] private bool hasOpenedIDT = false;
    [SerializeField] private bool hasPurchasedWeapon = false;
    [SerializeField] private bool hasPurchasedAmmo = false;
    [SerializeField] private bool hasPurchasedHealthPack = false;
    [SerializeField] private bool hasClearedOutpost = false;
    [SerializeField] private bool hasSoldAutomatically = false;
    
    private bool weaponsUnlockedInShop = false;
    private bool isAutomaticSaleSubscribed = false;
    private bool outpostSetupDone = false;
    private EnemyOutpost tutorialOutpost;

    [Header("Objective UI")]
    private GameObject objectivePanel;
    private TMP_Text objectiveText;

    protected override void Awake()
    {
        persistBetweenScenes = false;
        base.Awake();
    }

    private void Start()
    {
        bool playTutorial = PlayerPrefs.GetInt("PlayTutorial", 1) == 1;
        if (!playTutorial)
        {
            currentState = TutorialState.Completed;
            if (DayNightManager.Instance != null)
            {
                DayNightManager.Instance.isTutorialActive = false;
            }
            return;
        }

        SubscribeEvents();
        StartCoroutine(StartTutorialRoutine());
    }

    private void SubscribeEvents()
    {
        if (isSubscribed) return;
        isSubscribed = true;

        PlayerWeapon.OnPlayerShoot += HandlePlayerShoot;
        
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.OnPlayerDodge += HandlePlayerDodge;
        }

        Code.Scripts.EventSystems.EventManager.Instance?.Subscribe<EnemyOutpostClearedEvent>(this, HandleOutpostCleared);
    }

    private void UnsubscribeEvents()
    {
        if (!isSubscribed) return;
        isSubscribed = false;

        PlayerWeapon.OnPlayerShoot -= HandlePlayerShoot;

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.OnPlayerDodge -= HandlePlayerDodge;
        }

        if (InterDimensionalTransporter.Instance != null && isAutomaticSaleSubscribed)
        {
            InterDimensionalTransporter.Instance.OnItemSold -= HandleAutomaticSale;
        }

        Code.Scripts.EventSystems.EventManager.Instance?.Unsubscribe<EnemyOutpostClearedEvent>(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        UnsubscribeEvents();
    }

    private IEnumerator StartTutorialRoutine()
    {
        yield return new WaitForSeconds(1.5f); // Let scene settle
        
        if (DayNightManager.Instance != null)
        {
            DayNightManager.Instance.isTutorialActive = true;
        }

        // Wipe starting items and resources
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.items.Clear();
        }

        if (BuildingUiManager.Instance != null)
        {
            foreach (ResourceType r in System.Enum.GetValues(typeof(ResourceType)))
            {
                int count = BuildingUiManager.Instance.GetResourceCount(r);
                if (count > 0)
                {
                    BuildingUiManager.Instance.RemoveResource(r, count);
                }
            }
        }

        // Set player money to $0
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.currentCurrencyValue = 0f;
            UiManager.Instance?.UpdateCurrency(0f);
        }

        CreateObjectiveUI(); // Create the programmatic objective HUD
        currentState = TutorialState.BriefIntro;
        UpdateObjectiveText();
    }

    public bool IsStoreLocked()
    {
        return currentState == TutorialState.BriefIntro || 
               currentState == TutorialState.MineCoalManually || 
               currentState == TutorialState.FuelDCT || 
               currentState == TutorialState.SellOtherOres;
    }

    public bool IsTutorialCompleted()
    {
        return currentState == TutorialState.Completed;
    }

    private void Update()
    {
        if (currentState == TutorialState.Completed) return;

        bool isStoreOpen = UiManager.Instance != null && UiManager.Instance.StorePanel != null && UiManager.Instance.StorePanel.activeSelf;

        switch (currentState)
        {
            case TutorialState.BriefIntro:
                if (BuildingUiManager.Instance != null && BuildingUiManager.Instance.CurrentOpenBuilding is InterDimensionalTransporter)
                {
                    currentState = TutorialState.MineCoalManually;
                    UpdateObjectiveText();
                }
                break;

            case TutorialState.MineCoalManually:
                int coalCount = BuildingUiManager.Instance != null ? BuildingUiManager.Instance.GetResourceCount(ResourceType.Coal) : 0;
                if (coalCount >= 5)
                {
                    currentState = TutorialState.FuelDCT;
                    UpdateObjectiveText();
                }
                break;

            case TutorialState.FuelDCT:
                if (coalFedCount >= 5)
                {
                    currentState = TutorialState.SellOtherOres;
                    UpdateObjectiveText();
                    
                    if (UiManager.HasInstance)
                    {
                        UiManager.Instance.ShowGeneralAlert("IDT ONLINE - COMMERCIAL PORT ONLINE", new Color(0.2f, 1f, 0.2f));
                    }
                }
                break;

            case TutorialState.SellOtherOres:
                float currentMoney = CurrencyManager.Instance != null ? CurrencyManager.Instance.currentCurrencyValue : 0f;
                if (currentMoney >= 50f)
                {
                    currentState = TutorialState.BuyFirstWeapon;
                    UpdateObjectiveText();
                }
                break;

            case TutorialState.BuyFirstWeapon:
                UnlockWeaponsInShop();
                if (hasPurchasedWeapon)
                {
                    currentState = TutorialState.BuyAmmoHealth;
                    UpdateObjectiveText();
                }
                break;

            case TutorialState.BuyAmmoHealth:
                if (hasPurchasedAmmo && hasPurchasedHealthPack && !isStoreOpen)
                {
                    currentState = TutorialState.DestroyEnemyOutpost;
                    UpdateObjectiveText();
                }
                break;

            case TutorialState.DestroyEnemyOutpost:
                SetupOutpostPhase();
                
                // Track outpost destruction
                if (tutorialOutpost != null)
                {
                    tutorialOutpost.buildings.RemoveAll(b => b == null);
                    if (tutorialOutpost.buildings.Count == 0)
                    {
                        hasClearedOutpost = true;
                    }
                }

                if (hasClearedOutpost)
                {
                    currentState = TutorialState.BuyMinerConveyors;
                    UpdateObjectiveText();
                }
                break;

            case TutorialState.BuyMinerConveyors:
                int minerCount = GetInventoryCount(BuildingType.Miner);
                int conveyorCount = GetInventoryCount(BuildingType.Conveyor);
                if (minerCount >= 1 && conveyorCount >= 10 && !isStoreOpen)
                {
                    currentState = TutorialState.SetupAutomation;
                    UpdateObjectiveText();
                }
                break;

            case TutorialState.SetupAutomation:
                // Ensure automated sale listener is active
                if (InterDimensionalTransporter.Instance != null && !isAutomaticSaleSubscribed)
                {
                    InterDimensionalTransporter.Instance.OnItemSold += HandleAutomaticSale;
                    isAutomaticSaleSubscribed = true;
                }

                if (hasSoldAutomatically)
                {
                    currentState = TutorialState.EarnAndExpand;
                    UpdateObjectiveText();

                    // Start evening countdown
                    if (DayNightManager.Instance != null)
                    {
                        DayNightManager.Instance.isTutorialActive = false;
                        DayNightManager.Instance.timeRemaining = 25f; // Night in 25s
                    }
                }
                break;

            case TutorialState.EarnAndExpand:
                if (DayNightManager.Instance != null && DayNightManager.Instance.currentPhase == CyclePhase.Evening)
                {
                    currentState = TutorialState.DefendFirstRaid;
                    UpdateObjectiveText();
                }
                break;

            case TutorialState.DefendFirstRaid:
                UpdateObjectiveText();
                if (DayNightManager.Instance != null && DayNightManager.Instance.currentPhase == CyclePhase.UpgradePhase)
                {
                    currentState = TutorialState.Completed;
                    CompleteTutorial(true);
                }
                break;
        }
    }

    private void SetupOutpostPhase()
    {
        if (outpostSetupDone) return;
        outpostSetupDone = true;

        // 1. Force unlock the North, East, and North-East zones so camera can follow
        if (ZoneManager.Instance != null)
        {
            ZoneManager.Instance.ForceUnlockZone(new Vector2Int(1, 0));
            ZoneManager.Instance.ForceUnlockZone(new Vector2Int(0, 1));
            ZoneManager.Instance.ForceUnlockZone(new Vector2Int(1, 1));
        }

        // 2. Spawn tutorial outpost
        if (EnemyOutpostManager.Instance != null)
        {
            Vector2Int centerCell = Vector2Int.zero;
            if (GridManager.Instance != null) centerCell = GridManager.Instance.center;
            
            Vector2Int spawnPos = centerCell + new Vector2Int(16, 16);
            tutorialOutpost = EnemyOutpostManager.Instance.SpawnTutorialOutpost(spawnPos, 4);
        }

        // 3. Unlock dodge roll for tutorial
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.canDodgeRoll = true;
        }
    }

    private void UnlockWeaponsInShop()
    {
        if (weaponsUnlockedInShop || UpgradeManager.Instance == null) return;
        weaponsUnlockedInShop = true;

        foreach (var upg in UpgradeManager.Instance.allUpgrades)
        {
            if (upg != null && upg.type == UpgradeType.Weapon)
            {
                if (!UpgradeManager.Instance.activeUpgradesInShop.Contains(upg))
                {
                    UpgradeManager.Instance.activeUpgradesInShop.Add(upg);
                }
            }
        }
        UpgradeManager.Instance.TriggerUpgradesChanged();
    }

    public void HandleWeaponPurchased(UpgradeDefinition weapon)
    {
        if (currentState == TutorialState.BuyFirstWeapon)
        {
            hasPurchasedWeapon = true;
            
            // Remove other weapons from shop
            if (UpgradeManager.Instance != null)
            {
                List<UpgradeDefinition> toRemove = new List<UpgradeDefinition>();
                foreach (var upg in UpgradeManager.Instance.activeUpgradesInShop)
                {
                    if (upg != null && upg.type == UpgradeType.Weapon && upg != weapon)
                    {
                        toRemove.Add(upg);
                    }
                }
                foreach (var upg in toRemove)
                {
                    UpgradeManager.Instance.activeUpgradesInShop.Remove(upg);
                }
                UpgradeManager.Instance.TriggerUpgradesChanged();
            }
            UpdateObjectiveText();
        }
    }

    public void HandleAmmoPurchased()
    {
        if (currentState == TutorialState.BuyAmmoHealth)
        {
            hasPurchasedAmmo = true;
            UpdateObjectiveText();
        }
    }

    public void HandleHealthPackPurchased()
    {
        if (currentState == TutorialState.BuyAmmoHealth)
        {
            hasPurchasedHealthPack = true;
            UpdateObjectiveText();
        }
    }

    private void HandleAutomaticSale(ConveyorItem item)
    {
        if (currentState == TutorialState.SetupAutomation)
        {
            hasSoldAutomatically = true;
            UpdateObjectiveText();
        }
    }

    private void HandleFuelAdded(ResourceType type)
    {
        if (currentState == TutorialState.FuelDCT && type == ResourceType.Coal)
        {
            coalFedCount++;
            UpdateObjectiveText();
        }
    }

    private void HandlePlayerShoot()
    {
        if (currentState == TutorialState.DefendFirstRaid)
        {
            hasFiredWeapon = true;
        }
    }

    private void HandlePlayerDodge()
    {
        if (currentState == TutorialState.DefendFirstRaid)
        {
            hasDodgeRolled = true;
        }
    }

    private void HandleOutpostCleared(EnemyOutpostClearedEvent e)
    {
        if (currentState == TutorialState.DestroyEnemyOutpost)
        {
            hasClearedOutpost = true;
            UpdateObjectiveText();
        }
    }

    private int GetInventoryCount(BuildingType type)
    {
        if (InventoryManager.Instance == null || InventoryManager.Instance.items == null) return 0;
        var item = InventoryManager.Instance.items.Find(i => i.data != null && i.data.type == type);
        return item != null ? item.count : 0;
    }

    public void CompleteTutorial(bool showVisual)
    {
        currentState = TutorialState.Completed;

        if (objectivePanel != null)
        {
            Destroy(objectivePanel);
        }

        if (showVisual)
        {
            ShowCompletionVisual();
        }

        PlayerPrefs.SetInt("PlayTutorial", 0);
        PlayerPrefs.Save();

        UnsubscribeEvents();

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ToggleUi(false);
        }

        if (DayNightManager.Instance != null)
        {
            DayNightManager.Instance.CompleteTutorial();
            if (showVisual)
            {
                DayNightManager.Instance.timeRemaining = 10f; // Give them a few seconds before next day starts
            }
            else
            {
                DayNightManager.Instance.isTutorialActive = false;
            }
        }
    }

    public void RestartTutorial()
    {
        StopAllCoroutines();

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ToggleUi(false);
        }

        currentState = TutorialState.BriefIntro;
        coalFedCount = 0;
        hasFiredWeapon = false;
        hasDodgeRolled = false;
        hasOpenedIDT = false;
        hasPurchasedWeapon = false;
        hasPurchasedAmmo = false;
        hasPurchasedHealthPack = false;
        hasClearedOutpost = false;
        hasSoldAutomatically = false;
        weaponsUnlockedInShop = false;
        isAutomaticSaleSubscribed = false;
        outpostSetupDone = false;
        tutorialOutpost = null;

        PlayerPrefs.SetInt("PlayTutorial", 1);
        PlayerPrefs.Save();

        if (DayNightManager.Instance != null)
        {
            DayNightManager.Instance.isTutorialActive = true;
        }

        SubscribeEvents();
        StartCoroutine(StartTutorialRoutine());
    }

    public static void RequestTutorialAgain()
    {
        PlayerPrefs.SetInt("PlayTutorial", 1);
        PlayerPrefs.Save();
        Debug.Log("Tutorial requested. It will play again on next scene start.");
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

    private void CreateObjectiveUI()
    {
        GameObject canvasGo = GameObject.Find("HUD Canvas");
        if (canvasGo == null) canvasGo = GameObject.Find("Canvas");
        if (canvasGo == null) canvasGo = FindFirstObjectByType<Canvas>()?.gameObject;

        if (canvasGo == null) return;

        objectivePanel = new GameObject("TutorialObjectivePanel", typeof(RectTransform), typeof(Image));
        objectivePanel.transform.SetParent(canvasGo.transform, false);

        RectTransform panelRt = objectivePanel.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(1f, 0f);
        panelRt.anchorMax = new Vector2(1f, 0f);
        panelRt.pivot = new Vector2(1f, 0f);
        panelRt.anchoredPosition = new Vector2(-20f, 20f);
        panelRt.sizeDelta = new Vector2(440f, 180f);

        Image img = objectivePanel.GetComponent<Image>();
        img.color = new Color(0.01f, 0.05f, 0.01f, 0.9f);

        Outline outline = objectivePanel.AddComponent<Outline>();
        outline.effectColor = new Color(0.2f, 0.9f, 0.2f, 0.7f);
        outline.effectDistance = new Vector2(2f, -2f);

        GameObject textGo = new GameObject("ObjectiveText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(objectivePanel.transform, false);

        RectTransform textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(15f, 15f);
        textRt.offsetMax = new Vector2(-15f, -15f);

        objectiveText = textGo.GetComponent<TextMeshProUGUI>();
        objectiveText.fontSize = 26;
        objectiveText.color = new Color(0.2f, 1f, 0.2f);
        objectiveText.alignment = TextAlignmentOptions.TopLeft;
        objectiveText.enableWordWrapping = true;
        
        UpdateObjectiveText();
    }

    public void UpdateObjectiveText()
    {
        if (objectiveText == null) return;

        string title = "MISSION OBJECTIVES\n-----------------\n";
        string body = "";

        switch (currentState)
        {
            case TutorialState.BriefIntro:
                body = "- Walk to the Interdimensional Transceiver (IDT)\n- Press [E] next to the IDT to open its panel";
                break;
            case TutorialState.MineCoalManually:
                int coalCount = BuildingUiManager.Instance != null ? BuildingUiManager.Instance.GetResourceCount(ResourceType.Coal) : 0;
                body = $"- Hover Coal deposits and hold [E] to mine\n- Mine Coal ({coalCount}/5)";
                break;
            case TutorialState.FuelDCT:
                body = $"- Open the IDT panel (Press [E] next to it)\n- Insert 5 Coal to boot reactor (Coal fed: {coalFedCount}/5)";
                break;
            case TutorialState.SellOtherOres:
                float currentMoney = CurrencyManager.Instance != null ? CurrencyManager.Instance.currentCurrencyValue : 0f;
                body = $"- Manually mine other resources (Iron, Copper)\n- Open the IDT UI & Sell Carried Ores\n- Earn $50 (Current: ${currentMoney:F0}/$50)";
                break;
            case TutorialState.BuyFirstWeapon:
                body = "- Move away from buildings & Press [E] to open Store\n- Purchase your first Weapon Upgrade";
                break;
            case TutorialState.BuyAmmoHealth:
                string ammoStatus = hasPurchasedAmmo ? "[x]" : "[ ]";
                string healthStatus = hasPurchasedHealthPack ? "[x]" : "[ ]";
                body = $"- Navigate the Store and purchase:\n  {ammoStatus} Defensive Ammo Pack\n  {healthStatus} Tactical Health Pack\n- Close the Store menu to progress";
                break;
            case TutorialState.DestroyEnemyOutpost:
                int remainingBuildings = 0;
                if (tutorialOutpost != null)
                {
                    tutorialOutpost.buildings.RemoveAll(b => b == null);
                    remainingBuildings = tutorialOutpost.buildings.Count;
                }
                body = $"- Travel North-East to locate the human outpost\n- Destroy all outpost structures ({remainingBuildings} remaining)";
                break;
            case TutorialState.BuyMinerConveyors:
                int miners = GetInventoryCount(BuildingType.Miner);
                int conveyors = GetInventoryCount(BuildingType.Conveyor);
                body = $"- Open the Ship Store catalog (Press [E])\n- Purchase 1 Miner ({miners}/1)\n- Purchase 10 Conveyors ({conveyors}/10)";
                break;
            case TutorialState.SetupAutomation:
                body = "- Place Miner on an ore deposit (Press [R] to rotate)\n- Connect Miner to the IDT with Conveyors\n- Wait for 1 automatic resource sale";
                break;
            case TutorialState.EarnAndExpand:
                float time = DayNightManager.Instance != null ? DayNightManager.Instance.timeRemaining : 0f;
                body = $"- Automation established!\n- Earn credits and expand your factory\n- Prepare for nightfall ({Mathf.CeilToInt(time)}s remaining)";
                break;
            case TutorialState.DefendFirstRaid:
                int activeEnemies = 0;
                if (GameManager.Instance != null && GameManager.Instance.ActiveEnemies != null)
                {
                    activeEnemies = GameManager.Instance.ActiveEnemies.Count;
                }
                body = $"- Press [TAB] to enter COMBAT MODE\n- Defend the IDT from invaders!\n- Active Hostiles: {activeEnemies}";
                break;
            default:
                body = "Tutorial complete.";
                break;
        }

        objectiveText.text = title + body;
    }
}
