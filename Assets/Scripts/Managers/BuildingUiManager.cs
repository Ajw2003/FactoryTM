using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Singleton;
using Buildings;
using UnityEngine.EventSystems;

public class BuildingUiManager : SingletonBase<BuildingUiManager>
{
    [Header("Discovery Registry")]
    private HashSet<ResourceType> discoveredResources = new HashSet<ResourceType>();

    [Header("Player Resource Inventory")]
    private Dictionary<ResourceType, int> playerResources = new Dictionary<ResourceType, int>();

    [Header("UI Panels")]
    private GameObject jadePanel;
    private TMP_Text jadeTitleText;
    private TMP_Text jadeDetailText;
    private TMP_Text jadePromptText;

    private GameObject buildingPanel;
    private TMP_Text buildingTitleText;
    private TMP_Text buildingDetailText;
    private GameObject fuelBarContainer;
    private Image fuelBarFillImage;
    private TMP_Text fuelBarText;
    
    private GameObject actionButton1Go;
    private Button actionButton1;
    private TMP_Text actionButton1Text;
    
    private GameObject actionButton2Go;
    private Button actionButton2;
    private TMP_Text actionButton2Text;

    [Header("State")]
    private BuildingLogic currentOpenBuilding;
    public BuildingLogic CurrentOpenBuilding => currentOpenBuilding;
    private Canvas HUDCanvas;

    public bool IsPanelOpen => buildingPanel != null && buildingPanel.activeSelf;
    
    public BuildingLogic HoveredBuilding { get; private set; }
    public ResourceNode HoveredNode { get; private set; }
    public ConveyorItem HoveredItem { get; private set; }
    public BuildingLogic HoveredInteractable { get; private set; }

    // Hover UI Info
    private string hoveredUiName = "";
    private string hoveredUiDesc = "";
    private string hoveredUiExtra = "";
    private bool isHoveringUi = false;

    protected override void Awake()
    {
        persistBetweenScenes = false;
        base.Awake();
    }

    private void Start()
    {
        FindCanvas();
        CreateJadePanel();
        CreateBuildingUiPanel();
        
        // Start with Stone and Coal as undiscovered, but let player discover them systems-style!
        // We do not pre-register them, so they start as Unknown until mined!
    }

    private void FindCanvas()
    {
        GameObject canvasGo = GameObject.Find("HUD Canvas");
        if (canvasGo == null) canvasGo = GameObject.Find("Canvas");
        if (canvasGo == null) canvasGo = FindFirstObjectByType<Canvas>()?.gameObject;

        if (canvasGo != null)
        {
            HUDCanvas = canvasGo.GetComponent<Canvas>();
        }
    }

    #region Discovery & Inventory API

    public void DiscoverResource(ResourceType type)
    {
        if (discoveredResources.Add(type))
        {
            if (UiManager.HasInstance)
            {
                UiManager.Instance.ShowGeneralAlert($"NEW RESOURCE IDENTIFIED: {type.ToString().ToUpper()}", new Color(0.2f, 1f, 1f));
            }
        }
    }

    public bool IsResourceDiscovered(ResourceType type)
    {
        return discoveredResources.Contains(type);
    }

    public int GetResourceCount(ResourceType type)
    {
        if (playerResources.TryGetValue(type, out int count))
        {
            return count;
        }
        return 0;
    }

    public void AddResource(ResourceType type, int count = 1)
    {
        if (playerResources.ContainsKey(type))
        {
            playerResources[type] += count;
        }
        else
        {
            playerResources[type] = count;
        }

        DiscoverResource(type);
        
        if (TutorialManager.HasInstance)
        {
            TutorialManager.Instance.UpdateObjectiveText();
        }
    }

    public bool RemoveResource(ResourceType type, int count = 1)
    {
        if (GetResourceCount(type) >= count)
        {
            playerResources[type] -= count;
            
            if (TutorialManager.HasInstance)
            {
                TutorialManager.Instance.UpdateObjectiveText();
            }
            return true;
        }
        return false;
    }

    #endregion

    #region Jade Hover HUD Creation

    private void CreateJadePanel()
    {
        if (HUDCanvas == null) return;

        // Jade Panel Container
        jadePanel = new GameObject("JadeHoverHUD", typeof(RectTransform), typeof(Image));
        jadePanel.transform.SetParent(HUDCanvas.transform, false);

        RectTransform rt = jadePanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.96f);
        rt.anchorMax = new Vector2(0.5f, 0.96f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, 0f);
        rt.sizeDelta = new Vector2(620f, 140f);

        Image img = jadePanel.GetComponent<Image>();
        img.color = new Color(0.01f, 0.05f, 0.01f, 0.92f); // CRT translucent green background

        Outline outline = jadePanel.AddComponent<Outline>();
        outline.effectColor = new Color(0.2f, 0.9f, 0.2f, 0.8f);
        outline.effectDistance = new Vector2(2f, -2f);

        // Title text
        GameObject titleGo = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGo.transform.SetParent(jadePanel.transform, false);
        RectTransform titleRt = titleGo.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0f, 0.65f);
        titleRt.anchorMax = new Vector2(1f, 0.95f);
        titleRt.offsetMin = new Vector2(20f, 0f);
        titleRt.offsetMax = new Vector2(-20f, 0f);

        jadeTitleText = titleGo.GetComponent<TextMeshProUGUI>();
        jadeTitleText.fontSize = 30;
        jadeTitleText.fontStyle = FontStyles.Bold;
        jadeTitleText.color = new Color(0.2f, 1f, 0.2f);
        jadeTitleText.alignment = TextAlignmentOptions.Left;

        // Detail text
        GameObject detailGo = new GameObject("DetailText", typeof(RectTransform), typeof(TextMeshProUGUI));
        detailGo.transform.SetParent(jadePanel.transform, false);
        RectTransform detailRt = detailGo.GetComponent<RectTransform>();
        detailRt.anchorMin = new Vector2(0f, 0.35f);
        detailRt.anchorMax = new Vector2(1f, 0.65f);
        detailRt.offsetMin = new Vector2(20f, 0f);
        detailRt.offsetMax = new Vector2(-20f, 0f);

        jadeDetailText = detailGo.GetComponent<TextMeshProUGUI>();
        jadeDetailText.fontSize = 24;
        jadeDetailText.color = new Color(0.2f, 0.8f, 0.2f, 0.85f);
        jadeDetailText.alignment = TextAlignmentOptions.Left;

        // Action prompt text
        GameObject promptGo = new GameObject("PromptText", typeof(RectTransform), typeof(TextMeshProUGUI));
        promptGo.transform.SetParent(jadePanel.transform, false);
        RectTransform promptRt = promptGo.GetComponent<RectTransform>();
        promptRt.anchorMin = new Vector2(0f, 0.05f);
        promptRt.anchorMax = new Vector2(1f, 0.35f);
        promptRt.offsetMin = new Vector2(20f, 0f);
        promptRt.offsetMax = new Vector2(-20f, 0f);

        jadePromptText = promptGo.GetComponent<TextMeshProUGUI>();
        jadePromptText.fontSize = 22;
        jadePromptText.fontStyle = FontStyles.Italic;
        jadePromptText.color = new Color(1f, 0.8f, 0.2f); // gold prompt color
        jadePromptText.alignment = TextAlignmentOptions.Left;

        jadePanel.SetActive(false);
    }

    #endregion

    #region Machine Config Panel Creation

    private void CreateBuildingUiPanel()
    {
        if (HUDCanvas == null) return;

        // Building config main background
        buildingPanel = new GameObject("BuildingConfigPanel", typeof(RectTransform), typeof(Image));
        buildingPanel.transform.SetParent(HUDCanvas.transform, false);

        RectTransform rt = buildingPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(680f, 440f);

        Image img = buildingPanel.GetComponent<Image>();
        img.color = new Color(0.01f, 0.05f, 0.01f, 0.97f);

        Outline outline = buildingPanel.AddComponent<Outline>();
        outline.effectColor = new Color(0.2f, 0.9f, 0.2f, 0.9f);
        outline.effectDistance = new Vector2(3f, -3f);

        // Title label
        GameObject titleGo = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGo.transform.SetParent(buildingPanel.transform, false);
        RectTransform titleRt = titleGo.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0f, 0.88f);
        titleRt.anchorMax = new Vector2(1f, 0.98f);
        titleRt.offsetMin = new Vector2(25f, 0f);
        titleRt.offsetMax = new Vector2(-70f, 0f);

        buildingTitleText = titleGo.GetComponent<TextMeshProUGUI>();
        buildingTitleText.fontSize = 36;
        buildingTitleText.fontStyle = FontStyles.Bold;
        buildingTitleText.color = new Color(0.2f, 1f, 0.2f);
        buildingTitleText.alignment = TextAlignmentOptions.MidlineLeft;

        // Close button [X]
        GameObject closeGo = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        closeGo.transform.SetParent(buildingPanel.transform, false);
        RectTransform closeRt = closeGo.GetComponent<RectTransform>();
        closeRt.anchorMin = new Vector2(1f, 1f);
        closeRt.anchorMax = new Vector2(1f, 1f);
        closeRt.pivot = new Vector2(1f, 1f);
        closeRt.anchoredPosition = new Vector2(-20f, -20f);
        closeRt.sizeDelta = new Vector2(45f, 45f);

        Image closeImg = closeGo.GetComponent<Image>();
        closeImg.color = new Color(0.1f, 0.25f, 0.1f, 0.8f);
        Outline closeOutline = closeGo.AddComponent<Outline>();
        closeOutline.effectColor = new Color(0.2f, 0.9f, 0.2f, 0.6f);
        closeOutline.effectDistance = new Vector2(1f, -1f);

        GameObject closeTextGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        closeTextGo.transform.SetParent(closeGo.transform, false);
        RectTransform closeTextRt = closeTextGo.GetComponent<RectTransform>();
        closeTextRt.anchorMin = Vector2.zero;
        closeTextRt.anchorMax = Vector2.one;
        closeTextRt.offsetMin = Vector2.zero;
        closeTextRt.offsetMax = Vector2.zero;
        TextMeshProUGUI closeTxt = closeTextGo.GetComponent<TextMeshProUGUI>();
        closeTxt.text = "X";
        closeTxt.fontSize = 30;
        closeTxt.fontStyle = FontStyles.Bold;
        closeTxt.color = new Color(0.2f, 1f, 0.2f);
        closeTxt.alignment = TextAlignmentOptions.Center;

        Button closeBtn = closeGo.GetComponent<Button>();
        closeBtn.onClick.AddListener(() => ClosePanel());

        // Detail text
        GameObject detailGo = new GameObject("DetailText", typeof(RectTransform), typeof(TextMeshProUGUI));
        detailGo.transform.SetParent(buildingPanel.transform, false);
        RectTransform detailRt = detailGo.GetComponent<RectTransform>();
        detailRt.anchorMin = new Vector2(0f, 0.48f);
        detailRt.anchorMax = new Vector2(1f, 0.85f);
        detailRt.offsetMin = new Vector2(30f, 0f);
        detailRt.offsetMax = new Vector2(-30f, 0f);

        buildingDetailText = detailGo.GetComponent<TextMeshProUGUI>();
        buildingDetailText.fontSize = 28;
        buildingDetailText.color = new Color(0.2f, 0.9f, 0.2f);
        buildingDetailText.alignment = TextAlignmentOptions.TopLeft;
        buildingDetailText.enableWordWrapping = true;

        // Progress / Fuel Bar Container
        fuelBarContainer = new GameObject("FuelBarContainer", typeof(RectTransform), typeof(Image));
        fuelBarContainer.transform.SetParent(buildingPanel.transform, false);
        RectTransform barRt = fuelBarContainer.GetComponent<RectTransform>();
        barRt.anchorMin = new Vector2(0.5f, 0.38f);
        barRt.anchorMax = new Vector2(0.5f, 0.38f);
        barRt.pivot = new Vector2(0.5f, 0.5f);
        barRt.anchoredPosition = Vector2.zero;
        barRt.sizeDelta = new Vector2(600f, 40f);

        Image barBg = fuelBarContainer.GetComponent<Image>();
        barBg.color = new Color(0.02f, 0.1f, 0.02f, 0.8f);
        Outline barOutline = fuelBarContainer.AddComponent<Outline>();
        barOutline.effectColor = new Color(0.2f, 0.9f, 0.2f, 0.5f);
        barOutline.effectDistance = new Vector2(1f, -1f);

        // Progress fill Image
        GameObject fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillGo.transform.SetParent(fuelBarContainer.transform, false);
        RectTransform fillRt = fillGo.GetComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0f, 0f);
        fillRt.anchorMax = new Vector2(1f, 1f); // fill dynamically scaled later
        fillRt.pivot = new Vector2(0f, 0.5f);
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;

        fuelBarFillImage = fillGo.GetComponent<Image>();
        fuelBarFillImage.color = new Color(0.2f, 0.9f, 0.2f, 0.85f);

        // Progress Text
        GameObject barTextGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        barTextGo.transform.SetParent(fuelBarContainer.transform, false);
        RectTransform barTextRt = barTextGo.GetComponent<RectTransform>();
        barTextRt.anchorMin = Vector2.zero;
        barTextRt.anchorMax = Vector2.one;
        barTextRt.offsetMin = Vector2.zero;
        barTextRt.offsetMax = Vector2.zero;
        fuelBarText = barTextGo.GetComponent<TextMeshProUGUI>();
        fuelBarText.fontSize = 24;
        fuelBarText.color = Color.black; // high contrast black on green bar
        fuelBarText.fontStyle = FontStyles.Bold;
        fuelBarText.alignment = TextAlignmentOptions.Center;

        // Button 1: Insert Fuel / Reload
        actionButton1Go = new GameObject("ActionButton1", typeof(RectTransform), typeof(Image), typeof(Button));
        actionButton1Go.transform.SetParent(buildingPanel.transform, false);
        RectTransform btn1Rt = actionButton1Go.GetComponent<RectTransform>();
        btn1Rt.anchorMin = new Vector2(0.05f, 0.08f);
        btn1Rt.anchorMax = new Vector2(0.48f, 0.28f);
        btn1Rt.offsetMin = Vector2.zero;
        btn1Rt.offsetMax = Vector2.zero;

        Image btn1Img = actionButton1Go.GetComponent<Image>();
        btn1Img.color = new Color(0.05f, 0.15f, 0.05f, 0.9f);
        Outline btn1Outline = actionButton1Go.AddComponent<Outline>();
        btn1Outline.effectColor = new Color(0.2f, 0.9f, 0.2f, 0.7f);

        GameObject btn1TxtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        btn1TxtGo.transform.SetParent(actionButton1Go.transform, false);
        RectTransform btn1TxtRt = btn1TxtGo.GetComponent<RectTransform>();
        btn1TxtRt.anchorMin = Vector2.zero;
        btn1TxtRt.anchorMax = Vector2.one;
        btn1TxtRt.offsetMin = Vector2.zero;
        btn1TxtRt.offsetMax = Vector2.zero;
        actionButton1Text = btn1TxtGo.GetComponent<TextMeshProUGUI>();
        actionButton1Text.fontSize = 24;
        actionButton1Text.color = new Color(0.2f, 1f, 0.2f);
        actionButton1Text.alignment = TextAlignmentOptions.Center;

        actionButton1 = actionButton1Go.GetComponent<Button>();

        // Button 2: Deposit All / Open Catalog
        actionButton2Go = new GameObject("ActionButton2", typeof(RectTransform), typeof(Image), typeof(Button));
        actionButton2Go.transform.SetParent(buildingPanel.transform, false);
        RectTransform btn2Rt = actionButton2Go.GetComponent<RectTransform>();
        btn2Rt.anchorMin = new Vector2(0.52f, 0.08f);
        btn2Rt.anchorMax = new Vector2(0.95f, 0.28f);
        btn2Rt.offsetMin = Vector2.zero;
        btn2Rt.offsetMax = Vector2.zero;

        Image btn2Img = actionButton2Go.GetComponent<Image>();
        btn2Img.color = new Color(0.05f, 0.15f, 0.05f, 0.9f);
        Outline btn2Outline = actionButton2Go.AddComponent<Outline>();
        btn2Outline.effectColor = new Color(0.2f, 0.9f, 0.2f, 0.7f);

        GameObject btn2TxtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        btn2TxtGo.transform.SetParent(actionButton2Go.transform, false);
        RectTransform btn2TxtRt = btn2TxtGo.GetComponent<RectTransform>();
        btn2TxtRt.anchorMin = Vector2.zero;
        btn2TxtRt.anchorMax = Vector2.one;
        btn2TxtRt.offsetMin = Vector2.zero;
        btn2TxtRt.offsetMax = Vector2.zero;
        actionButton2Text = btn2TxtGo.GetComponent<TextMeshProUGUI>();
        actionButton2Text.fontSize = 24;
        actionButton2Text.color = new Color(0.2f, 1f, 0.2f);
        actionButton2Text.alignment = TextAlignmentOptions.Center;

        actionButton2 = actionButton2Go.GetComponent<Button>();

        buildingPanel.SetActive(false);
    }

    #endregion

    #region Panel Control (Open/Close/Interact)

    public void OpenPanel(BuildingLogic building)
    {
        if (building == null || buildingPanel == null) return;

        currentOpenBuilding = building;
        buildingPanel.SetActive(true);
        RefreshBuildingPanel();

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.StateMachine.TransitionTo(PlayerController.Instance.StateMachine.buildingUiState);
        }
    }

    public void ClosePanel()
    {
        if (buildingPanel == null || !buildingPanel.activeSelf) return;

        buildingPanel.SetActive(false);
        currentOpenBuilding = null;

        if (PlayerController.Instance != null && PlayerController.Instance.StateMachine.CurrentState == PlayerController.Instance.StateMachine.buildingUiState)
        {
            PlayerController.Instance.StateMachine.TransitionTo(PlayerController.Instance.StateMachine.idleState);
        }
    }

    public void InteractWithHovered()
    {
        if (HoveredInteractable != null)
        {
            OpenPanel(HoveredInteractable);
        }
    }

    public void RefreshBuildingPanel()
    {
        if (currentOpenBuilding == null) return;

        // Default buttons state
        actionButton1Go.SetActive(true);
        actionButton2Go.SetActive(true);
        actionButton1.onClick.RemoveAllListeners();
        actionButton2.onClick.RemoveAllListeners();

        // 1. Seller (Reactor) UI
        if (currentOpenBuilding is Seller seller)
        {
            buildingTitleText.text = "IDT REACTOR MODULE";
            
            string statusStr = seller.fuelRemaining > 0f ? "OPERATIONAL" : "OFFLINE (COAL REQUIRED)";
            if (seller.isUraniumBoosted) statusStr = "BOOSTED // URANIUM ACTIVE (2X VALUE)";
            
            buildingDetailText.text = $"Reactor Status: {statusStr}\n" +
                                      $"Intake Coal: {GetResourceCount(ResourceType.Coal)} available\n" +
                                      $"Intake Uranium: {GetResourceCount(ResourceType.Uranium)} available";

            fuelBarContainer.SetActive(true);
            float fuelPct = seller.fuelRemaining / seller.maxFuel;
            fuelBarFillImage.rectTransform.anchorMax = new Vector2(fuelPct, 1f);
            fuelBarText.text = $"Reactor Fuel: {Mathf.CeilToInt(seller.fuelRemaining)}s / {Mathf.CeilToInt(seller.maxFuel)}s";

            // Fuel with Coal
            actionButton1Text.text = "INSERT COAL (+20s)";
            actionButton1.interactable = GetResourceCount(ResourceType.Coal) > 0;
            actionButton1.onClick.AddListener(() => {
                if (RemoveResource(ResourceType.Coal, 1))
                {
                    seller.fuelRemaining = Mathf.Min(seller.maxFuel, seller.fuelRemaining + 20f);
                    if (UiManager.HasInstance) UiManager.Instance.ShowGeneralAlert("REACTOR FUELED: COAL (+20s)", new Color(0.3f, 0.9f, 0.3f));
                    
                    // Trigger tutorial fuel count directly
                    if (TutorialManager.HasInstance)
                    {
                        var method = typeof(TutorialManager).GetMethod("HandleFuelAdded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (method != null) method.Invoke(TutorialManager.Instance, new object[] { ResourceType.Coal });
                    }
                    RefreshBuildingPanel();
                }
            });

            // Sell carried ores OR insert Uranium
            int copperCount = GetResourceCount(ResourceType.Copper);
            int ironCount = GetResourceCount(ResourceType.Iron);
            int stonCount = GetResourceCount(ResourceType.Ston);
            int diamondCount = GetResourceCount(ResourceType.Diamond);
            int titaniumCount = GetResourceCount(ResourceType.Titanium);
            int quartzCount = GetResourceCount(ResourceType.Quartz);
            int totalOres = copperCount + ironCount + stonCount + diamondCount + titaniumCount + quartzCount;

            if (GetResourceCount(ResourceType.Uranium) > 0)
            {
                actionButton2Text.text = "INSERT URANIUM (+60s)";
                actionButton2.interactable = true;
                actionButton2.onClick.AddListener(() => {
                    if (RemoveResource(ResourceType.Uranium, 1))
                    {
                        seller.fuelRemaining = Mathf.Min(seller.maxFuel, seller.fuelRemaining + 60f);
                        seller.isUraniumBoosted = true;
                        seller.uraniumBoostDuration = 30f;
                        if (UiManager.HasInstance) UiManager.Instance.ShowGeneralAlert("REACTOR BOOSTED: URANIUM (+60s)", new Color(0.3f, 1f, 1f));
                        
                        if (TutorialManager.HasInstance)
                        {
                            var method = typeof(TutorialManager).GetMethod("HandleFuelAdded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            if (method != null) method.Invoke(TutorialManager.Instance, new object[] { ResourceType.Uranium });
                        }
                        RefreshBuildingPanel();
                    }
                });
            }
            else
            {
                // Calculate total earnings based on ore values
                float estimatedEarnings = stonCount * 5f + copperCount * 10f + ironCount * 15f + quartzCount * 20f + titaniumCount * 30f + diamondCount * 50f;
                float multiplier = seller.isUraniumBoosted ? 2f : 1f;
                float finalEarnings = estimatedEarnings * multiplier;

                actionButton2Text.text = totalOres > 0 ? $"SELL CARRIED ORES (+${finalEarnings:F0})" : "SELL CARRIED ORES";
                actionButton2.interactable = totalOres > 0 && seller.fuelRemaining > 0f;
                actionButton2.onClick.AddListener(() => {
                    if (totalOres > 0)
                    {
                        RemoveResource(ResourceType.Copper, copperCount);
                        RemoveResource(ResourceType.Iron, ironCount);
                        RemoveResource(ResourceType.Ston, stonCount);
                        RemoveResource(ResourceType.Diamond, diamondCount);
                        RemoveResource(ResourceType.Titanium, titaniumCount);
                        RemoveResource(ResourceType.Quartz, quartzCount);

                        CurrencyManager.Instance.AddCurrency(finalEarnings);
                        if (UiManager.HasInstance)
                        {
                            UiManager.Instance.ShowGeneralAlert($"ORES SOLD: +${finalEarnings:F0}", new Color(0.2f, 1f, 0.2f));
                        }
                        RefreshBuildingPanel();
                    }
                });
            }
        }
        // 2. Miner UI
        else if (currentOpenBuilding is MinerLogic miner)
        {
            buildingTitleText.text = miner.data.buildingName.ToUpper();
            
            string statusStr = miner.fuelRemaining > 0f ? "EXTRACTING" : "OUT OF FUEL (COAL REQUIRED)";
            buildingDetailText.text = $"Machine Status: {statusStr}\n" +
                                      $"Extraction Speed: {miner.data.proccessingSpeed / miner.GetTierMultiplier():F1}s / cycle\n" +
                                      $"Carried Coal: {GetResourceCount(ResourceType.Coal)} available";

            fuelBarContainer.SetActive(true);
            float fuelPct = miner.fuelRemaining / miner.maxFuel;
            fuelBarFillImage.rectTransform.anchorMax = new Vector2(fuelPct, 1f);
            fuelBarText.text = $"Boiler Fuel: {Mathf.CeilToInt(miner.fuelRemaining)}% / 100%";

            // Add Coal
            actionButton1Text.text = "LOAD 1 COAL";
            actionButton1.interactable = GetResourceCount(ResourceType.Coal) > 0;
            actionButton1.onClick.AddListener(() => {
                if (RemoveResource(ResourceType.Coal, 1))
                {
                    miner.fuelRemaining = Mathf.Min(miner.maxFuel, miner.fuelRemaining + 25f);
                    RefreshBuildingPanel();
                }
            });

            // Deposit all Coal
            actionButton2Text.text = "LOAD ALL COAL";
            int avail = GetResourceCount(ResourceType.Coal);
            actionButton2.interactable = avail > 0;
            actionButton2.onClick.AddListener(() => {
                int needed = Mathf.CeilToInt((miner.maxFuel - miner.fuelRemaining) / 25f);
                int transfer = Mathf.Min(avail, needed);
                if (transfer > 0)
                {
                    if (RemoveResource(ResourceType.Coal, transfer))
                    {
                        miner.fuelRemaining = Mathf.Min(miner.maxFuel, miner.fuelRemaining + (transfer * 25f));
                        RefreshBuildingPanel();
                    }
                }
            });
        }
        // 3. Furnace UI
        else if (currentOpenBuilding is Furnace furnace)
        {
            buildingTitleText.text = furnace.data.buildingName.ToUpper();
            
            string statusStr = furnace.fuelRemaining > 0f ? "SMELTING" : "OUT OF FUEL (COAL REQUIRED)";
            buildingDetailText.text = $"Smelter Status: {statusStr}\n" +
                                      $"Cooking Speed: {furnace.data.proccessingSpeed / furnace.GetTierMultiplier():F1}s / cycle\n" +
                                      $"Carried Coal: {GetResourceCount(ResourceType.Coal)} available";

            fuelBarContainer.SetActive(true);
            float fuelPct = furnace.fuelRemaining / furnace.maxFuel;
            fuelBarFillImage.rectTransform.anchorMax = new Vector2(fuelPct, 1f);
            fuelBarText.text = $"Furnace Fuel: {Mathf.CeilToInt(furnace.fuelRemaining)}% / 100%";

            // Add Coal
            actionButton1Text.text = "LOAD 1 COAL";
            actionButton1.interactable = GetResourceCount(ResourceType.Coal) > 0;
            actionButton1.onClick.AddListener(() => {
                if (RemoveResource(ResourceType.Coal, 1))
                {
                    furnace.fuelRemaining = Mathf.Min(furnace.maxFuel, furnace.fuelRemaining + 25f);
                    RefreshBuildingPanel();
                }
            });

            // Deposit all Coal
            actionButton2Text.text = "LOAD ALL COAL";
            int avail = GetResourceCount(ResourceType.Coal);
            actionButton2.interactable = avail > 0;
            actionButton2.onClick.AddListener(() => {
                int needed = Mathf.CeilToInt((furnace.maxFuel - furnace.fuelRemaining) / 25f);
                int transfer = Mathf.Min(avail, needed);
                if (transfer > 0)
                {
                    if (RemoveResource(ResourceType.Coal, transfer))
                    {
                        furnace.fuelRemaining = Mathf.Min(furnace.maxFuel, furnace.fuelRemaining + (transfer * 25f));
                        RefreshBuildingPanel();
                    }
                }
            });
        }
        // 4. Turret UI
        else if (currentOpenBuilding is TurretLogic turret)
        {
            buildingTitleText.text = turret.data.buildingName.ToUpper();
            
            string statusStr = turret.ammoRemaining > 0 ? "ARMED // SCANNING" : "OUT OF AMMO // DEFENSE SYSTEM HALTED";
            int reserve = PlayerController.Instance != null ? PlayerController.Instance.ammoReserve : 0;

            buildingDetailText.text = $"Turret Status: {statusStr}\n" +
                                      $"Targeting Range: {turret.targetRange} tiles\n" +
                                      $"Ammo Reserves: {reserve} rounds carried";

            fuelBarContainer.SetActive(true);
            float ammoPct = (float)turret.ammoRemaining / turret.maxAmmo;
            fuelBarFillImage.rectTransform.anchorMax = new Vector2(ammoPct, 1f);
            fuelBarText.text = $"Mag Capacity: {turret.ammoRemaining} / {turret.maxAmmo}";

            // Insert 30 Ammo
            actionButton1Text.text = "INSERT 30 AMMO";
            actionButton1.interactable = reserve > 0 && turret.ammoRemaining < turret.maxAmmo;
            actionButton1.onClick.AddListener(() => {
                if (PlayerController.Instance != null)
                {
                    int load = Mathf.Min(30, PlayerController.Instance.ammoReserve);
                    load = Mathf.Min(load, turret.maxAmmo - turret.ammoRemaining);
                    if (load > 0)
                    {
                        PlayerController.Instance.ammoReserve -= load;
                        turret.ammoRemaining += load;
                        
                        // Force weapon UI updates
                        PlayerWeapon playerWeapon = PlayerController.Instance.GetComponentInChildren<PlayerWeapon>();
                        if (playerWeapon != null) playerWeapon.UpdateAmmoUI();

                        RefreshBuildingPanel();
                    }
                }
            });

            // Fully Reload
            actionButton2Text.text = "FULLY RELOAD";
            actionButton2.interactable = reserve > 0 && turret.ammoRemaining < turret.maxAmmo;
            actionButton2.onClick.AddListener(() => {
                if (PlayerController.Instance != null)
                {
                    int needed = turret.maxAmmo - turret.ammoRemaining;
                    int load = Mathf.Min(needed, PlayerController.Instance.ammoReserve);
                    if (load > 0)
                    {
                        PlayerController.Instance.ammoReserve -= load;
                        turret.ammoRemaining += load;

                        PlayerWeapon playerWeapon = PlayerController.Instance.GetComponentInChildren<PlayerWeapon>();
                        if (playerWeapon != null) playerWeapon.UpdateAmmoUI();

                        RefreshBuildingPanel();
                    }
                }
            });
        }
    }

    #endregion

    #region Hover Raycasting & Update

    private void Update()
    {
        if (PauseManager.IsPaused) return;

        // Perform raycasts to determine hovered target
        PerformHoverChecks();

        // Refresh panel if open
        if (IsPanelOpen)
        {
            RefreshBuildingPanel();
        }
    }

    private void PerformHoverChecks()
    {
        // 1. Check UI Hovering first
        isHoveringUi = false;
        hoveredUiName = "";
        hoveredUiDesc = "";
        hoveredUiExtra = "";

        if (EventSystem.current != null)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                foreach (var result in results)
                {
                    // Check if it's a hotbar slot
                    HotbarSlotUI slot = result.gameObject.GetComponentInParent<HotbarSlotUI>();
                    if (slot != null)
                    {
                        // Get data via reflection or public method
                        var field = slot.GetType().GetField("currentData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        BuildingData data = field != null ? (BuildingData)field.GetValue(slot) : null;
                        
                        if (data != null)
                        {
                            isHoveringUi = true;
                            hoveredUiName = data.buildingName;
                            hoveredUiDesc = data.description;
                            var item = InventoryManager.Instance.items.Find(i => i.data == data);
                            hoveredUiExtra = $"In Inventory: {(item != null ? item.count : 0)}";
                            break;
                        }
                    }

                    // Check if it's a shop upgrade item
                    UpgradeShopItem shopItem = result.gameObject.GetComponentInParent<UpgradeShopItem>();
                    if (shopItem != null && shopItem.Definition != null)
                    {
                        isHoveringUi = true;
                        hoveredUiName = shopItem.Definition.upgradeName.ToUpper();
                        hoveredUiDesc = shopItem.Definition.description;
                        
                        float cost = shopItem.Definition.costInShop;
                        if (shopItem.Definition.type == UpgradeType.ZoneExpansion && ZoneManager.HasInstance)
                        {
                            cost = ZoneManager.Instance.GetUnlockCost();
                        }
                        hoveredUiExtra = $"Type: {shopItem.GetTypeString()} // Cost: ${cost:F0}";
                        break;
                    }
                }
            }
        }

        if (isHoveringUi)
        {
            HoveredBuilding = null;
            HoveredNode = null;
            HoveredItem = null;
            HoveredInteractable = null;

            jadePanel.SetActive(true);
            jadeTitleText.text = hoveredUiName;
            jadeDetailText.text = hoveredUiDesc;
            jadePromptText.text = hoveredUiExtra;
            return;
        }

        // 2. Check World Hovering
        if (GridManager.Instance == null || Camera.main == null) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Vector2Int cell = GridManager.Instance.WorldToCellConversion(mouseWorldPos);

        // Hovered building
        HoveredBuilding = null;
        HoveredInteractable = null;
        if (PlacementManager.HasInstance)
        {
            if (PlacementManager.Instance.GetActiveBuildings().TryGetValue(cell, out GameObject buildingObj))
            {
                if (buildingObj != null)
                {
                    HoveredBuilding = buildingObj.GetComponent<BuildingLogic>();
                    if (HoveredBuilding != null && HoveredBuilding.data.type != BuildingType.Conveyor)
                    {
                        HoveredInteractable = HoveredBuilding;
                    }
                }
            }
        }

        // Hovered conveyor item
        HoveredItem = null;
        if (ItemTracker.HasInstance)
        {
            List<ConveyorItem> itemsInCell = ItemTracker.Instance.GetItemsInCell(cell);
            if (itemsInCell != null && itemsInCell.Count > 0)
            {
                // Take first item lying in the cell
                HoveredItem = itemsInCell[0];
            }
        }

        // Hovered ore node
        HoveredNode = null;
        if (ResourceManager.Instance != null)
        {
            HoveredNode = ResourceManager.Instance.GetNodeAtPosition(cell);
        }

        // 3. Update Jade HUD contents
        if (HoveredBuilding != null)
        {
            jadePanel.SetActive(true);
            jadeTitleText.text = HoveredBuilding.data.buildingName.ToUpper();
            
            string hpStr = $"Structure Durability: {HoveredBuilding.Health} / {Mathf.RoundToInt(HoveredBuilding.data.maxHealth * HoveredBuilding.GetTierMultiplier())} HP";
            string statusStr = "";
            string promptStr = "";

            if (HoveredBuilding.data.type == BuildingType.Conveyor)
            {
                statusStr = "Conveyor Belt System";
                promptStr = ""; // Conveyors need no UI
            }
            else
            {
                promptStr = "[E] Open Configuration Interface";
                
                if (HoveredBuilding is Seller seller)
                {
                    statusStr = seller.fuelRemaining > 0f ? $"IDT Active // Fuel: {Mathf.CeilToInt(seller.fuelRemaining)}s" : "IDT Reactor Offline // Coal Required";
                    if (seller.isUraniumBoosted) statusStr = $"REACTOR BOOSTED // Uranium remaining: {Mathf.CeilToInt(seller.uraniumBoostDuration)}s";
                }
                else if (HoveredBuilding is MinerLogic miner)
                {
                    statusStr = miner.fuelRemaining > 0f ? $"Operational // Fuel: {Mathf.CeilToInt(miner.fuelRemaining)}%" : "System Halted // Fuel depleted";
                }
                else if (HoveredBuilding is Furnace furnace)
                {
                    statusStr = furnace.fuelRemaining > 0f ? $"Smelting // Fuel: {Mathf.CeilToInt(furnace.fuelRemaining)}%" : "System Halted // Fuel depleted";
                }
                else if (HoveredBuilding is TurretLogic turret)
                {
                    statusStr = turret.ammoRemaining > 0 ? $"Defense Active // Ammo: {turret.ammoRemaining} / {turret.maxAmmo}" : "WEAPON OFFLINE // Ammo depleted";
                }
            }

            jadeDetailText.text = $"{hpStr}\n{statusStr}";
            jadePromptText.text = promptStr;
        }
        else if (HoveredItem != null)
        {
            jadePanel.SetActive(true);
            jadeTitleText.text = $"{HoveredItem.name.Replace("(Clone)", "").Replace("_", " ").ToUpper()}";
            jadeDetailText.text = $"Raw Material Item // Market Value: ${HoveredItem.value}";
            jadePromptText.text = "[E] Pick Up Item";
        }
        else if (HoveredNode != null)
        {
            jadePanel.SetActive(true);
            
            ResourceType rType = ResourceType.Coal;
            if (HoveredNode.minedItemPrefab != null)
            {
                ConveyorItem citem = HoveredNode.minedItemPrefab.GetComponent<ConveyorItem>();
                if (citem != null) rType = citem.resourceType;
            }

            if (IsResourceDiscovered(rType))
            {
                jadeTitleText.text = $"{rType.ToString().ToUpper()} DEPOSIT";
                jadeDetailText.text = $"Extraction Target // Content: {HoveredNode.oreCount} units remaining";
            }
            else
            {
                jadeTitleText.text = "UNKNOWN RESOURCE DEPOSIT";
                jadeDetailText.text = "Extraction Target // Identity: Undiscovered";
            }

            jadePromptText.text = "[E] Mine Manually (+1 Resource)";
        }
        else
        {
            jadePanel.SetActive(false);
        }
    }

    #endregion
}
