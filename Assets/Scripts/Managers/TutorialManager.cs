using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
namespace Managers
{
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
    
    public class TutorialManager : StateMachine.BaseStateMachine
    {
        public static TutorialManager Instance { get; private set; }
        public static bool HasInstance => Instance != null;
    
        [Header("Dialogue Assets (Bypassed)")]
        [SerializeField] private DialogueSO introDialogue;
        [SerializeField] private DialogueSO combatDialogue;

        [Header("State (ReadOnly)")]
        public TutorialState currentState => (CurrentState as StateMachine.BaseTutorialState)?.StateId ?? TutorialState.NotStarted;

        [Header("Progression Variables")]
        [SerializeField] private int coalFedCount = 0;
        [SerializeField] private bool hasFiredWeapon = false;
        [SerializeField] private bool hasDodgeRolled = false;
        [SerializeField] private bool isSubscribed = false;

        [SerializeField] private bool hasOpenedIDT = false;
        [SerializeField] private bool hasPurchasedWeapon = false;
        [SerializeField] private bool hasPurchasedAmmo = false;
        [SerializeField] private bool hasPurchasedHealthPack = false;
        [SerializeField] private bool hasClearedOutpost = false;
        [SerializeField] private bool hasSoldAutomatically = false;

        [SerializeField] private bool weaponsUnlockedInShop = false;
        [SerializeField] private bool isAutomaticSaleSubscribed = false;
        [SerializeField] private bool isFuelSubscribed = false;
        [SerializeField] private bool outpostSetupDone = false;
        [SerializeField] private EnemyOutpost tutorialOutpost;

        // Progression flags are driven by TutorialManager itself and read by the tutorial state
        // classes; the two the states also need to set go through the Mark* methods below.
        public int CoalFedCount => coalFedCount;
        public bool HasFiredWeapon => hasFiredWeapon;
        public bool HasDodgeRolled => hasDodgeRolled;
        public bool IsSubscribed => isSubscribed;
        public bool HasOpenedIDT => hasOpenedIDT;
        public bool HasPurchasedWeapon => hasPurchasedWeapon;
        public bool HasPurchasedAmmo => hasPurchasedAmmo;
        public bool HasPurchasedHealthPack => hasPurchasedHealthPack;
        public bool HasClearedOutpost => hasClearedOutpost;
        public bool HasSoldAutomatically => hasSoldAutomatically;
        public bool WeaponsUnlockedInShop => weaponsUnlockedInShop;
        public bool IsAutomaticSaleSubscribed => isAutomaticSaleSubscribed;
        public bool IsFuelSubscribed => isFuelSubscribed;
        public bool OutpostSetupDone => outpostSetupDone;
        public EnemyOutpost TutorialOutpost => tutorialOutpost;

        /// <summary>Records that the tutorial's target outpost has been fully destroyed.</summary>
        public void MarkOutpostCleared()
        {
            hasClearedOutpost = true;
        }

        /// <summary>Records that the automatic-sale hook on the IDT has been wired up.</summary>
        public void MarkAutomaticSaleSubscribed()
        {
            isAutomaticSaleSubscribed = true;
        }

        /// <summary>Records that the fuel-added hook on the IDT has been wired up.</summary>
        public void MarkFuelSubscribed()
        {
            isFuelSubscribed = true;
        }
    
        private List<GameObject> activeObjectiveObjects = new List<GameObject>();
        private List<TextMeshProUGUI> objectiveTexts = new List<TextMeshProUGUI>();
        private TutorialState lastStateForObjectives = TutorialState.NotStarted;
        private bool _needsObjectiveUpdate = false;

        private void MarkObjectiveUpdateDirty()
        {
            _needsObjectiveUpdate = true;
        }

        // State instances
        public StateMachine.NotStartedTutorialState notStartedState { get; private set; }
        public StateMachine.BriefIntroTutorialState briefIntroState { get; private set; }
        public StateMachine.MineCoalManuallyTutorialState mineCoalState { get; private set; }
        public StateMachine.FuelDCTTutorialState fuelDCTState { get; private set; }
        public StateMachine.SellOtherOresTutorialState sellOtherOresState { get; private set; }
        public StateMachine.BuyFirstWeaponTutorialState buyFirstWeaponState { get; private set; }
        public StateMachine.BuyAmmoHealthTutorialState buyAmmoHealthState { get; private set; }
        public StateMachine.DestroyEnemyOutpostTutorialState destroyOutpostState { get; private set; }
        public StateMachine.BuyMinerConveyorsTutorialState buyMinerConveyorsState { get; private set; }
        public StateMachine.SetupAutomationTutorialState setupAutomationState { get; private set; }
        public StateMachine.EarnAndExpandTutorialState earnAndExpandState { get; private set; }
        public StateMachine.DefendFirstRaidTutorialState defendFirstRaidState { get; private set; }
        public StateMachine.CompletedTutorialState completedState { get; private set; }
    
        protected void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
    
            // Initialize state objects
            notStartedState = new StateMachine.NotStartedTutorialState(this);
            briefIntroState = new StateMachine.BriefIntroTutorialState(this);
            mineCoalState = new StateMachine.MineCoalManuallyTutorialState(this);
            fuelDCTState = new StateMachine.FuelDCTTutorialState(this);
            sellOtherOresState = new StateMachine.SellOtherOresTutorialState(this);
            buyFirstWeaponState = new StateMachine.BuyFirstWeaponTutorialState(this);
            buyAmmoHealthState = new StateMachine.BuyAmmoHealthTutorialState(this);
            destroyOutpostState = new StateMachine.DestroyEnemyOutpostTutorialState(this);
            buyMinerConveyorsState = new StateMachine.BuyMinerConveyorsTutorialState(this);
            setupAutomationState = new StateMachine.SetupAutomationTutorialState(this);
            earnAndExpandState = new StateMachine.EarnAndExpandTutorialState(this);
            defendFirstRaidState = new StateMachine.DefendFirstRaidTutorialState(this);
            completedState = new StateMachine.CompletedTutorialState(this);
    
            ChangeState(notStartedState);
        }
    
        private void Start()
        {
            bool playTutorial = PlayerPrefs.GetInt("PlayTutorial", 1) == 1;
            if (!playTutorial)
            {
                ChangeState(completedState);
                if (DayNightManager.Instance != null)
                {
                    DayNightManager.Instance.SetTutorialActive(false);
                }
                return;
            }
    
            SubscribeEvents();
            StartCoroutine(StartTutorialRoutine());
        }

        public override void Update()
        {
            base.Update();
            if (_needsObjectiveUpdate)
            {
                UpdateObjectiveText();
                _needsObjectiveUpdate = false;
            }
            else if (currentState == TutorialState.EarnAndExpand || currentState == TutorialState.DefendFirstRaid)
            {
                UpdateObjectivesPanelText();
            }
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

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.onInventoryChange += MarkObjectiveUpdateDirty;
            }
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.onCurrencyChange += MarkObjectiveUpdateDirty;
            }
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

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.onInventoryChange -= MarkObjectiveUpdateDirty;
            }
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.onCurrencyChange -= MarkObjectiveUpdateDirty;
            }
        }
    
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            UnsubscribeEvents();
        }
    
        private IEnumerator StartTutorialRoutine()
        {
            yield return new WaitForSeconds(1.5f); // Let scene settle
            
            if (DayNightManager.Instance != null)
            {
                DayNightManager.Instance.SetTutorialActive(true);
            }
    
            // Set player money to $0
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.currentCurrencyValue = 0f;
                UiManager.Instance?.UpdateCurrency(0f);
            }
    
            ChangeState(briefIntroState);
        }
    
        public bool IsStoreLocked()
        {
            return currentState == TutorialState.BriefIntro || 
                   currentState == TutorialState.MineCoalManually || 
                   currentState == TutorialState.FuelDCT;
        }
    
        public bool IsTutorialCompleted()
        {
            return currentState == TutorialState.Completed;
        }
    
        public void CheckTransitions()
        {
            if (CurrentState is StateMachine.BaseTutorialState tutorialState)
            {
                tutorialState.CheckTransitions();
            }
        }
    
        public void SetupOutpostPhase()
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
                if (GridManager.Instance != null) centerCell = GridManager.Instance.Center;
                
                Vector2Int spawnPos = centerCell + new Vector2Int(16, 16);
                tutorialOutpost = EnemyOutpostManager.Instance.SpawnTutorialOutpost(spawnPos, 4);
            }
    
            // 3. (Dodge roll unlock moved to UpgradeManager)
            // Removed: PlayerController.Instance.canDodgeRoll = true;
        }
    
        public void UnlockWeaponsInShop()
        {
            if (weaponsUnlockedInShop || UpgradeManager.Instance == null) return;
            weaponsUnlockedInShop = true;
    
            foreach (var upg in UpgradeManager.Instance.AllUpgrades)
            {
                if (upg != null && upg.type == UpgradeType.Weapon)
                {
                    if (!UpgradeManager.Instance.ActiveUpgradesInShop.Contains(upg))
                    {
                        UpgradeManager.Instance.ActiveUpgradesInShop.Add(upg);
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
                    foreach (var upg in UpgradeManager.Instance.ActiveUpgradesInShop)
                    {
                        if (upg != null && upg.type == UpgradeType.Weapon && upg != weapon)
                        {
                            toRemove.Add(upg);
                        }
                    }
                    foreach (var upg in toRemove)
                    {
                        UpgradeManager.Instance.ActiveUpgradesInShop.Remove(upg);
                    }
                    UpgradeManager.Instance.TriggerUpgradesChanged();
                }
                UpdateObjectiveText();
                CheckTransitions();
            }
        }
    
        public void HandleAmmoPurchased()
        {
            if (currentState == TutorialState.BuyAmmoHealth)
            {
                hasPurchasedAmmo = true;
                UpdateObjectiveText();
                CheckTransitions();
            }
        }
    
        public void HandleHealthPackPurchased()
        {
            if (currentState == TutorialState.BuyAmmoHealth)
            {
                hasPurchasedHealthPack = true;
                UpdateObjectiveText();
                CheckTransitions();
            }
        }
    
        public void HandleAutomaticSale(ConveyorItem item)
        {
            if (currentState == TutorialState.SetupAutomation)
            {
                hasSoldAutomatically = true;
                UpdateObjectiveText();
                CheckTransitions();
            }
        }
    
        public void HandleFuelAdded(specificItemType type)
        {
            if (currentState == TutorialState.FuelDCT && type == specificItemType.Coal)
            {
                coalFedCount++;
                UpdateObjectiveText();
                CheckTransitions();
            }
        }
    
        public void HandlePlayerShoot()
        {
            if (currentState == TutorialState.DefendFirstRaid)
            {
                hasFiredWeapon = true;
                CheckTransitions();
            }
        }
    
        public void HandlePlayerDodge()
        {
            if (currentState == TutorialState.DefendFirstRaid)
            {
                hasDodgeRolled = true;
                CheckTransitions();
            }
        }
    
        private void HandleOutpostCleared(EnemyOutpostClearedEvent e)
        {
            if (currentState == TutorialState.DestroyEnemyOutpost)
            {
                hasClearedOutpost = true;
                UpdateObjectiveText();
                CheckTransitions();
            }
        }
    
        public int GetInventoryCount(BuildingType type)
        {
            if (InventoryManager.Instance == null || InventoryManager.Instance.items == null) return 0;
            var item = InventoryManager.Instance.items.Find(i => i.data != null && i.data.type == type);
            return item != null ? item.count : 0;
        }
    
        public void CompleteTutorial(bool showVisual)
        {
            if (currentState != TutorialState.Completed)
            {
                ChangeState(completedState);
            }
    
            ClearObjectives();
    
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
                    DayNightManager.Instance.SetTimeRemaining(10f); // Give them a few seconds before next day starts
                }
                else
                {
                    DayNightManager.Instance.SetTutorialActive(false);
                }
            }
        }
    
        public void RestartTutorial()
        {
            StopAllCoroutines();
    
            lastStateForObjectives = TutorialState.NotStarted;
            ClearObjectives();
    
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.ToggleUi(false);
            }
    
            ChangeState(notStartedState);
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
                DayNightManager.Instance.SetTutorialActive(true);
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
    
        private DialogueSO LoadDialogueSOForState(TutorialState state)
        {
            string path = $"Dialogue/Tutorial/{state}";
            DialogueSO dialogue = Resources.Load<DialogueSO>(path);
            if (dialogue == null)
            {
                Debug.LogError($"TutorialManager: DialogueSO not found at Resources/{path}");
            }
            return dialogue;
        }
    
        private string GetFormattedObjectiveText(TutorialState state, bool isInitial)
        {
            DialogueSO dialogue = LoadDialogueSOForState(state);
            if (dialogue == null || dialogue.dialogues == null || dialogue.dialogues.Length == 0) return "";
    
            string template = dialogue.dialogues[0].text;
    
            switch (state)
            {
                case TutorialState.MineCoalManually:
                    int coalCount = isInitial ? 0 : (BuildingUiManager.Instance != null ? BuildingUiManager.Instance.GetResourceCount(specificItemType.Coal) : 0);
                    return string.Format(template, coalCount);
    
                case TutorialState.FuelDCT:
                    int fuelCount = isInitial ? 0 : coalFedCount;
                    return string.Format(template, fuelCount);
    
                case TutorialState.SellOtherOres:
                    float currentMoney = isInitial ? 0f : (CurrencyManager.Instance != null ? CurrencyManager.Instance.currentCurrencyValue : 0f);
                    return string.Format(template, currentMoney);
    
                case TutorialState.BuyAmmoHealth:
                    string ammoStatus = isInitial ? "[ ]" : (hasPurchasedAmmo ? "[x]" : "[ ]");
                    string healthStatus = isInitial ? "[ ]" : (hasPurchasedHealthPack ? "[x]" : "[ ]");
                    return string.Format(template, ammoStatus, healthStatus);
    
                case TutorialState.DestroyEnemyOutpost:
                    int currentHP = 0;
                    int maxHP = 0;
                    if (!isInitial && tutorialOutpost != null)
                    {
                        tutorialOutpost.buildings.RemoveAll(b => b == null);
                        foreach(var b in tutorialOutpost.buildings) {
                            if (b != null) {
                                currentHP += b.Health;
                                maxHP += (b.data != null ? b.data.maxHealth : b.Health);
                            }
                        }
                    }
                    else if (isInitial)
                    {
                        currentHP = 100;
                        maxHP = 100;
                    }
                    int percent = maxHP > 0 ? Mathf.CeilToInt(((float)currentHP / maxHP) * 100f) : 0;
                    return string.Format(template, percent);
    
                case TutorialState.BuyMinerConveyors:
                    int miners = isInitial ? 0 : GetInventoryCount(BuildingType.Miner);
                    int conveyors = isInitial ? 0 : GetInventoryCount(BuildingType.Conveyor);
                    return string.Format(template, miners, conveyors);
    
                case TutorialState.EarnAndExpand:
                    float time = isInitial ? 25f : (DayNightManager.Instance != null ? DayNightManager.Instance.TimeRemaining : 0f);
                    return string.Format(template, Mathf.CeilToInt(time));
    
                case TutorialState.DefendFirstRaid:
                    int activeEnemies = isInitial ? 0 : (GameManager.Instance != null && GameManager.Instance.ActiveEnemies != null ? GameManager.Instance.ActiveEnemies.Count : 0);
                    return string.Format(template, activeEnemies);
    
                default:
                    return template;
            }
        }
    
        public void UpdateObjectiveText()
        {
            if (DialogueManager.Instance == null) return;
    
            // 1. Trigger bottom static dialogue only once when transitioning to a new state
            if (currentState != lastStateForObjectives)
            {
                lastStateForObjectives = currentState;
    
                DialogueSO dialogue = LoadDialogueSOForState(currentState);
                if (dialogue != null && dialogue.dialogues != null && dialogue.dialogues.Length > 0)
                {
                    string initialDialogueText = GetFormattedObjectiveText(currentState, true);
                    DialogueManager.Instance.DisplayTutorialObjective(dialogue, initialDialogueText);
                }
    
                // Rebuild the left-side objectives panel list
                RebuildObjectivesList();
            }
            else
            {
                DialogueSO dialogue = LoadDialogueSOForState(currentState);
                if (dialogue != null && dialogue.dialogues != null && dialogue.dialogues.Length > 0)
                {
                    string updatedDialogueText = GetFormattedObjectiveText(currentState, false);
                    DialogueManager.Instance.DisplayTutorialObjective(dialogue, updatedDialogueText);
                }
            }
    
            // 2. Dynamically update the left-side objectives list checkboxes/values
            UpdateObjectivesPanelText();
        }
    
        private void CreateObjectivesUI()
        {
            if (UiManager.Instance == null || UiManager.Instance.PlayerStatsPanel == null) return;
            CreateObjectiveItem("SYS_OBJECTIVES //", true);
        }
    
        private TextMeshProUGUI CreateObjectiveItem(string initialText, bool isHeader = false)
        {
            if (UiManager.Instance == null || UiManager.Instance.PlayerStatsPanel == null) return null;
            Transform parentTransform = UiManager.Instance.PlayerStatsPanel.transform;
    
            GameObject itemGo = new GameObject(isHeader ? "ObjectiveHeader" : "ObjectiveItem", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(UnityEngine.UI.LayoutElement));
            itemGo.transform.SetParent(parentTransform, false);
    
            RectTransform rt = itemGo.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.sizeDelta = new Vector2(600f, isHeader ? 40f : 30f);
            }
    
            UnityEngine.UI.LayoutElement layout = itemGo.GetComponent<UnityEngine.UI.LayoutElement>();
            if (layout != null)
            {
                layout.preferredWidth = 600f;
                layout.preferredHeight = isHeader ? 40f : 30f;
            }
    
            TextMeshProUGUI txt = itemGo.GetComponent<TextMeshProUGUI>();
            txt.fontSize = isHeader ? 28 : 24;
            txt.fontStyle = isHeader ? FontStyles.Bold : FontStyles.Normal;
            txt.color = isHeader ? new Color(0.2f, 1f, 0.2f) : new Color(0.8f, 1f, 0.8f);
            txt.alignment = TextAlignmentOptions.MidlineLeft;
            txt.enableWordWrapping = true;
            txt.text = initialText;
    
            activeObjectiveObjects.Add(itemGo);
    
            if (!isHeader)
            {
                objectiveTexts.Add(txt);
            }
            return txt;
        }
    
        private void ClearObjectives()
        {
            foreach (var go in activeObjectiveObjects)
            {
                if (go != null)
                {
                    Destroy(go);
                }
            }
            activeObjectiveObjects.Clear();
            objectiveTexts.Clear();
        }
    
        private void RebuildObjectivesList()
        {
            ClearObjectives();
            CreateObjectivesUI();
    
            switch (currentState)
            {
                case TutorialState.BriefIntro:
                    CreateObjectiveItem("[ ] Walk to the Interdimensional Transceiver (IDT)");
                    CreateObjectiveItem("[ ] Press [E] next to the IDT to open its panel");
                    break;
    
                case TutorialState.MineCoalManually:
                    CreateObjectiveItem("[ ] Mine Coal (0/5)");
                    break;
    
                case TutorialState.FuelDCT:
                    CreateObjectiveItem("[ ] Open the IDT panel");
                    CreateObjectiveItem("[ ] Insert 5 Coal to boot reactor (Coal fed: 0/5)");
                    break;
    
                case TutorialState.SellOtherOres:
                    CreateObjectiveItem("[ ] Mine Iron and Copper ores");
                    CreateObjectiveItem("[ ] Sell ores at the IDT UI");
                    CreateObjectiveItem("[ ] Earn $50 (Current: $0/$50)");
                    break;
    
                case TutorialState.BuyFirstWeapon:
                    CreateObjectiveItem("[ ] Move away from structures");
                    CreateObjectiveItem("[ ] Press [E] to open the Ship Store");
                    CreateObjectiveItem("[ ] Purchase your first weapon upgrade");
                    break;
    
                case TutorialState.BuyAmmoHealth:
                    CreateObjectiveItem("[ ] Purchase Defensive Ammo Pack");
                    CreateObjectiveItem("[ ] Purchase Tactical Health Pack");
                    CreateObjectiveItem("[ ] Close the Store menu");
                    break;
    
                case TutorialState.DestroyEnemyOutpost:
                    CreateObjectiveItem("[ ] Travel North-East to locate human outpost");
                    CreateObjectiveItem("[ ] Destroy all human outpost structures");
                    break;
    
                case TutorialState.BuyMinerConveyors:
                    CreateObjectiveItem("[ ] Open the Ship Store");
                    CreateObjectiveItem("[ ] Purchase 1 Miner");
                    CreateObjectiveItem("[ ] Purchase 10 Conveyors");
                    break;
    
                case TutorialState.SetupAutomation:
                    CreateObjectiveItem("[ ] Place Miner on an ore deposit");
                    CreateObjectiveItem("[ ] Connect Miner to the IDT with Conveyors");
                    CreateObjectiveItem("[ ] Wait for 1 automatic resource sale");
                    break;
    
                case TutorialState.EarnAndExpand:
                    CreateObjectiveItem("[ ] Automation active!");
                    CreateObjectiveItem("[ ] Earn credits and expand your factory");
                    CreateObjectiveItem("[ ] Prepare for nightfall");
                    break;
    
                case TutorialState.DefendFirstRaid:
                    CreateObjectiveItem("[ ] Press [TAB] to enter COMBAT MODE");
                    CreateObjectiveItem("[ ] Defend the IDT from invaders!");
                    break;
            }
        }
    
        private void UpdateObjectivesPanelText()
        {
            if (objectiveTexts == null || objectiveTexts.Count == 0) return;
    
            switch (currentState)
            {
                case TutorialState.BriefIntro:
                    bool hasIDT = BuildingUiManager.Instance != null && BuildingUiManager.Instance.CurrentOpenBuilding is InterDimensionalTransporter;
                    objectiveTexts[0].text = hasIDT ? "[x] Walk to the Interdimensional Transceiver (IDT)" : "[ ] Walk to the Interdimensional Transceiver (IDT)";
                    objectiveTexts[1].text = hasIDT ? "[x] Press [E] next to the IDT to open its panel" : "[ ] Press [E] next to the IDT to open its panel";
                    break;
    
                case TutorialState.MineCoalManually:
                    int coalCount = BuildingUiManager.Instance != null ? BuildingUiManager.Instance.GetResourceCount(specificItemType.Coal) : 0;
                    objectiveTexts[0].text = coalCount >= 5 ? $"[x] Mine Coal ({coalCount}/5)" : $"[ ] Mine Coal ({coalCount}/5)";
                    break;
    
                case TutorialState.FuelDCT:
                    bool hasDCTOpen = BuildingUiManager.Instance != null && BuildingUiManager.Instance.CurrentOpenBuilding is InterDimensionalTransporter;
                    objectiveTexts[0].text = (hasDCTOpen || coalFedCount > 0) ? "[x] Open the IDT panel" : "[ ] Open the IDT panel";
                    objectiveTexts[1].text = coalFedCount >= 5 ? $"[x] Insert 5 Coal to boot reactor (Coal fed: {coalFedCount}/5)" : $"[ ] Insert 5 Coal to boot reactor (Coal fed: {coalFedCount}/5)";
                    break;
    
                case TutorialState.SellOtherOres:
                    float currentMoney = CurrencyManager.Instance != null ? CurrencyManager.Instance.currentCurrencyValue : 0f;
                    objectiveTexts[0].text = currentMoney > 0f ? "[x] Mine Iron and Copper ores" : "[ ] Mine Iron and Copper ores";
                    objectiveTexts[1].text = currentMoney > 0f ? "[x] Sell ores at the IDT UI" : "[ ] Sell ores at the IDT UI";
                    objectiveTexts[2].text = currentMoney >= 50f ? $"[x] Earn $50 (Current: ${currentMoney:F0}/$50)" : $"[ ] Earn $50 (Current: ${currentMoney:F0}/$50)";
                    break;
    
                case TutorialState.BuyFirstWeapon:
                    bool isStoreOpen = UiManager.Instance != null && UiManager.Instance.StorePanel != null && UiManager.Instance.StorePanel.activeSelf;
                    objectiveTexts[0].text = isStoreOpen ? "[x] Move away from structures" : "[ ] Move away from structures";
                    objectiveTexts[1].text = isStoreOpen ? "[x] Press [E] to open the Ship Store" : "[ ] Press [E] to open the Ship Store";
                    objectiveTexts[2].text = hasPurchasedWeapon ? "[x] Purchase your first weapon upgrade" : "[ ] Purchase your first weapon upgrade";
                    break;
    
                case TutorialState.BuyAmmoHealth:
                    objectiveTexts[0].text = hasPurchasedAmmo ? "[x] Purchase Defensive Ammo Pack" : "[ ] Purchase Defensive Ammo Pack";
                    objectiveTexts[1].text = hasPurchasedHealthPack ? "[x] Purchase Tactical Health Pack" : "[ ] Purchase Tactical Health Pack";
                    bool isStoreStillOpen = UiManager.Instance != null && UiManager.Instance.StorePanel != null && UiManager.Instance.StorePanel.activeSelf;
                    objectiveTexts[2].text = (!isStoreStillOpen && hasPurchasedAmmo && hasPurchasedHealthPack) ? "[x] Close the Store menu" : "[ ] Close the Store menu";
                    break;
    
                case TutorialState.DestroyEnemyOutpost:
                    int currentOutpostHP = 0;
                    int maxOutpostHP = 0;
                    if (tutorialOutpost != null)
                    {
                        tutorialOutpost.buildings.RemoveAll(b => b == null);
                        foreach(var b in tutorialOutpost.buildings) {
                            if (b != null) {
                                currentOutpostHP += b.Health;
                                maxOutpostHP += (b.data != null ? b.data.maxHealth : b.Health);
                            }
                        }
                    }
                    int hpPercent = maxOutpostHP > 0 ? Mathf.CeilToInt(((float)currentOutpostHP / maxOutpostHP) * 100f) : 0;
                    objectiveTexts[0].text = "[x] Travel North-East to locate human outpost";
                    objectiveTexts[1].text = hpPercent <= 0 ? $"[x] Destroy all human outpost structures (0% remaining)" : $"[ ] Destroy all human outpost structures ({hpPercent}% remaining)";
                    break;
    
                case TutorialState.BuyMinerConveyors:
                    int miners = GetInventoryCount(BuildingType.Miner);
                    int conveyors = GetInventoryCount(BuildingType.Conveyor);
                    bool storeOpenForBuy = UiManager.Instance != null && UiManager.Instance.StorePanel != null && UiManager.Instance.StorePanel.activeSelf;
                    objectiveTexts[0].text = storeOpenForBuy ? "[x] Open the Ship Store" : "[ ] Open the Ship Store";
                    objectiveTexts[1].text = miners >= 1 ? $"[x] Purchase 1 Miner ({miners}/1)" : $"[ ] Purchase 1 Miner ({miners}/1)";
                    objectiveTexts[2].text = conveyors >= 10 ? $"[x] Purchase 10 Conveyors ({conveyors}/10)" : $"[ ] Purchase 10 Conveyors ({conveyors}/10)";
                    break;
    
                case TutorialState.SetupAutomation:
                    objectiveTexts[0].text = hasSoldAutomatically ? "[x] Place Miner on an ore deposit" : "[ ] Place Miner on an ore deposit";
                    objectiveTexts[1].text = hasSoldAutomatically ? "[x] Connect Miner to the IDT with Conveyors" : "[ ] Connect Miner to the IDT with Conveyors";
                    objectiveTexts[2].text = hasSoldAutomatically ? "[x] Wait for 1 automatic resource sale" : "[ ] Wait for 1 automatic resource sale";
                    break;
    
                case TutorialState.EarnAndExpand:
                    float timeRemaining = DayNightManager.Instance != null ? DayNightManager.Instance.TimeRemaining : 0f;
                    objectiveTexts[0].text = "[x] Automation active!";
                    objectiveTexts[1].text = "[ ] Earn credits and expand your factory";
                    objectiveTexts[2].text = timeRemaining <= 0f ? "[x] Prepare for nightfall" : $"[ ] Prepare for nightfall ({Mathf.CeilToInt(timeRemaining)}s remaining)";
                    break;
    
                case TutorialState.DefendFirstRaid:
                    int activeEnemies = GameManager.Instance != null && GameManager.Instance.ActiveEnemies != null ? GameManager.Instance.ActiveEnemies.Count : 0;
                    objectiveTexts[0].text = activeEnemies == 0 ? "[x] Press [TAB] to enter COMBAT MODE" : "[ ] Press [TAB] to enter COMBAT MODE";
                    objectiveTexts[1].text = activeEnemies == 0 ? "[x] Defend the IDT from invaders! (0 active hostiles)" : $"[ ] Defend the IDT from invaders! ({activeEnemies} active hostiles)";
                    break;
            }
        }
    }
    
}


