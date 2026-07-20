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
    
        private GameObject resourceInventoryPanel;
        private Dictionary<ResourceType, GameObject> resourceSlots = new Dictionary<ResourceType, GameObject>();
        private GameObject idtIntakeDropZone;
    
        [Header("State")]
        private BuildingLogic currentOpenBuilding;
        public BuildingLogic CurrentOpenBuilding => currentOpenBuilding;
        private BuildingLogic lastConfiguredBuilding = null;
        private Canvas HUDCanvas;
    
        public bool IsPanelOpen => buildingPanel != null && buildingPanel.activeSelf;
        
        public BuildingLogic HoveredBuilding { get; private set; }
        public ResourceNode HoveredNode { get; private set; }
        public ConveyorItem HoveredItem { get; private set; }
        public BuildingLogic HoveredInteractable { get; private set; }
        
        protected override void Awake()
        {
            persistBetweenScenes = false;
            base.Awake();
        }
    
        private void Start()
        {
            FindCanvas();
            CreateBuildingUiPanel();
            CreateResourceInventoryPanel();
            
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
            RefreshResourceInventoryPanel();
        }
    
        public bool RemoveResource(ResourceType type, int count = 1)
        {
            if (count <= 0) return true;
            if (GetResourceCount(type) >= count)
            {
                playerResources[type] -= count;
                
                if (TutorialManager.HasInstance)
                {
                    TutorialManager.Instance.UpdateObjectiveText();
                }
                RefreshResourceInventoryPanel();
                return true;
            }
            return false;
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
            rt.anchorMin = new Vector2(1f, 0.5f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(1f, 0.5f);
            rt.anchoredPosition = new Vector2(-50f, 0f);
            rt.sizeDelta = new Vector2(1000f, 688f);
    
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
            buildingTitleText.fontSize = 52;
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
            closeRt.sizeDelta = new Vector2(56f, 56f);
    
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
            closeTxt.fontSize = 38;
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
            buildingDetailText.fontSize = 40;
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
            barRt.sizeDelta = new Vector2(750f, 50f);
    
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
            fuelBarText.fontSize = 35;
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
            actionButton1Text.fontSize = 30;
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
            actionButton2Text.fontSize = 30;
            actionButton2Text.color = new Color(0.2f, 1f, 0.2f);
            actionButton2Text.alignment = TextAlignmentOptions.Center;
    
            actionButton2 = actionButton2Go.GetComponent<Button>();
    
            // Create IDT Intake Drop Zone
            idtIntakeDropZone = new GameObject("IdtIntakeDropZone", typeof(RectTransform), typeof(Image));
            idtIntakeDropZone.transform.SetParent(buildingPanel.transform, false);
            RectTransform intakeRt = idtIntakeDropZone.GetComponent<RectTransform>();
            intakeRt.anchorMin = new Vector2(0.52f, 0.08f);
            intakeRt.anchorMax = new Vector2(0.95f, 0.82f);
            intakeRt.pivot = new Vector2(0.5f, 0.5f);
            intakeRt.offsetMin = Vector2.zero;
            intakeRt.offsetMax = Vector2.zero;
    
            Image intakeImg = idtIntakeDropZone.GetComponent<Image>();
            intakeImg.color = new Color(0.01f, 0.08f, 0.01f, 0.9f); // Dark translucent green background
    
            Outline intakeOutline = idtIntakeDropZone.AddComponent<Outline>();
            intakeOutline.effectColor = new Color(0.2f, 0.9f, 0.2f, 0.8f);
            intakeOutline.effectDistance = new Vector2(2f, -2f);
    
            // Arrow Symbol
            GameObject arrowGo = new GameObject("ArrowText", typeof(RectTransform), typeof(TextMeshProUGUI));
            arrowGo.transform.SetParent(idtIntakeDropZone.transform, false);
            RectTransform arrowRt = arrowGo.GetComponent<RectTransform>();
            arrowRt.anchorMin = new Vector2(0f, 0.55f);
            arrowRt.anchorMax = new Vector2(1f, 0.95f);
            arrowRt.offsetMin = Vector2.zero;
            arrowRt.offsetMax = Vector2.zero;
            TextMeshProUGUI arrowTxt = arrowGo.GetComponent<TextMeshProUGUI>();
            arrowTxt.text = "▼";
            arrowTxt.fontSize = 80;
            arrowTxt.fontStyle = FontStyles.Bold;
            arrowTxt.color = new Color(0.2f, 1f, 0.2f);
            arrowTxt.alignment = TextAlignmentOptions.Center;
    
            // Label: REACTOR INTAKE PORT
            GameObject labelGo = new GameObject("LabelText", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(idtIntakeDropZone.transform, false);
            RectTransform labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0f, 0.35f);
            labelRt.anchorMax = new Vector2(1f, 0.55f);
            labelRt.offsetMin = new Vector2(10f, 0f);
            labelRt.offsetMax = new Vector2(-10f, 0f);
            TextMeshProUGUI labelTxt = labelGo.GetComponent<TextMeshProUGUI>();
            labelTxt.text = "REACTOR INTAKE PORT";
            labelTxt.fontSize = 30;
            labelTxt.fontStyle = FontStyles.Bold;
            labelTxt.color = new Color(0.2f, 1f, 0.2f);
            labelTxt.alignment = TextAlignmentOptions.Center;
    
            // Instructions
            GameObject descGo = new GameObject("DescText", typeof(RectTransform), typeof(TextMeshProUGUI));
            descGo.transform.SetParent(idtIntakeDropZone.transform, false);
            RectTransform descRt = descGo.GetComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0f, 0.05f);
            descRt.anchorMax = new Vector2(1f, 0.35f);
            descRt.offsetMin = new Vector2(15f, 0f);
            descRt.offsetMax = new Vector2(-15f, 0f);
            TextMeshProUGUI descTxt = descGo.GetComponent<TextMeshProUGUI>();
            descTxt.text = "Drop Coal/Uranium to fuel\nor other resources to sell";
            descTxt.fontSize = 22;
            descTxt.color = new Color(0.2f, 0.8f, 0.2f, 0.85f);
            descTxt.alignment = TextAlignmentOptions.Center;
            descTxt.enableWordWrapping = true;
    
            buildingPanel.SetActive(false);
        }
    
        private void CreateResourceInventoryPanel()
        {
            if (HUDCanvas == null) return;
    
            // Resource Inventory main background panel
            resourceInventoryPanel = new GameObject("ResourceInventoryPanel", typeof(RectTransform), typeof(Image));
            resourceInventoryPanel.transform.SetParent(HUDCanvas.transform, false);
    
            RectTransform rt = resourceInventoryPanel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0.5f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(1f, 0.5f);
            rt.anchoredPosition = new Vector2(-1070f, 0f);
            rt.sizeDelta = new Vector2(475f, 688f);
    
            Image img = resourceInventoryPanel.GetComponent<Image>();
            img.color = new Color(0.01f, 0.05f, 0.01f, 0.97f);
    
            Outline outline = resourceInventoryPanel.AddComponent<Outline>();
            outline.effectColor = new Color(0.2f, 0.9f, 0.2f, 0.9f);
            outline.effectDistance = new Vector2(3f, -3f);
    
            // Title label
            GameObject titleGo = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(resourceInventoryPanel.transform, false);
            RectTransform titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 0.88f);
            titleRt.anchorMax = new Vector2(1f, 0.98f);
            titleRt.offsetMin = new Vector2(10f, 0f);
            titleRt.offsetMax = new Vector2(-10f, 0f);
    
            TextMeshProUGUI titleText = titleGo.GetComponent<TextMeshProUGUI>();
            titleText.fontSize = 38;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = new Color(0.2f, 1f, 0.2f);
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.text = "RESOURCE INVENTORY";
    
            // Instruction label
            GameObject instGo = new GameObject("InstructionText", typeof(RectTransform), typeof(TextMeshProUGUI));
            instGo.transform.SetParent(resourceInventoryPanel.transform, false);
            RectTransform instRt = instGo.GetComponent<RectTransform>();
            instRt.anchorMin = new Vector2(0f, 0.72f);
            instRt.anchorMax = new Vector2(1f, 0.88f);
            instRt.offsetMin = new Vector2(10f, 0f);
            instRt.offsetMax = new Vector2(-10f, 0f);
    
            TextMeshProUGUI instText = instGo.GetComponent<TextMeshProUGUI>();
            instText.fontSize = 28;
            instText.color = new Color(0.2f, 0.8f, 0.2f, 0.8f);
            instText.alignment = TextAlignmentOptions.Center;
            instText.enableWordWrapping = true;
            instText.text = "Drag resources from slot to IDT to fuel reactor or sell items.";
    
            // Create Grid of 8 Slots (4 columns, 2 rows)
            ResourceType[] types = (ResourceType[])System.Enum.GetValues(typeof(ResourceType));
            
            float startX = 17.5f;
            float startY = 300f;
            float slotW = 95f;
            float slotH = 125f;
            float spacingX = 20f;
            float spacingY = 30f;
    
            for (int i = 0; i < types.Length; i++)
            {
                ResourceType type = types[i];
                int col = i % 4;
                int row = i / 4;
    
                float x = startX + col * (slotW + spacingX);
                float y = startY - row * (slotH + spacingY);
    
                // Create Slot Container
                GameObject slotGo = new GameObject("Slot_" + type.ToString(), typeof(RectTransform), typeof(Image));
                slotGo.transform.SetParent(resourceInventoryPanel.transform, false);
                RectTransform slotRt = slotGo.GetComponent<RectTransform>();
                slotRt.anchorMin = new Vector2(0f, 0f);
                slotRt.anchorMax = new Vector2(0f, 0f);
                slotRt.pivot = new Vector2(0f, 0f);
                slotRt.anchoredPosition = new Vector2(x, y);
                slotRt.sizeDelta = new Vector2(slotW, slotH);
    
                Image slotImg = slotGo.GetComponent<Image>();
                slotImg.color = new Color(0f, 0.02f, 0f, 0.8f);
    
                Outline slotOutline = slotGo.AddComponent<Outline>();
                slotOutline.effectColor = new Color(0.2f, 0.9f, 0.2f, 0.4f);
                slotOutline.effectDistance = new Vector2(1f, -1f);
    
                // Icon Image
                GameObject iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconGo.transform.SetParent(slotGo.transform, false);
                RectTransform iconRt = iconGo.GetComponent<RectTransform>();
                iconRt.anchorMin = new Vector2(0.5f, 0.5f);
                iconRt.anchorMax = new Vector2(0.5f, 0.5f);
                iconRt.pivot = new Vector2(0.5f, 0.5f);
                iconRt.anchoredPosition = new Vector2(0f, 5f);
                iconRt.sizeDelta = new Vector2(60f, 60f);
    
                Image iconImg = iconGo.GetComponent<Image>();
                iconImg.color = Color.white;
                iconImg.raycastTarget = true;
    
                // Count Text (Bottom-right of slot)
                GameObject countGo = new GameObject("CountText", typeof(RectTransform), typeof(TextMeshProUGUI));
                countGo.transform.SetParent(slotGo.transform, false);
                RectTransform countRt = countGo.GetComponent<RectTransform>();
                countRt.anchorMin = new Vector2(0f, 0f);
                countRt.anchorMax = new Vector2(1f, 0.3f);
                countRt.offsetMin = new Vector2(2f, 2f);
                countRt.offsetMax = new Vector2(-4f, 2f);
    
                TextMeshProUGUI countText = countGo.GetComponent<TextMeshProUGUI>();
                countText.fontSize = 22;
                countText.fontStyle = FontStyles.Bold;
                countText.color = new Color(0.2f, 1f, 0.2f);
                countText.alignment = TextAlignmentOptions.BottomRight;
    
                // Value Text (Top-left of slot)
                GameObject valGo = new GameObject("ValueText", typeof(RectTransform), typeof(TextMeshProUGUI));
                valGo.transform.SetParent(slotGo.transform, false);
                RectTransform valRt = valGo.GetComponent<RectTransform>();
                valRt.anchorMin = new Vector2(0f, 0.7f);
                valRt.anchorMax = new Vector2(1f, 1f);
                valRt.offsetMin = new Vector2(4f, -2f);
                valRt.offsetMax = new Vector2(-2f, -2f);
    
                TextMeshProUGUI valText = valGo.GetComponent<TextMeshProUGUI>();
                valText.fontSize = 20;
                valText.color = new Color(1f, 0.8f, 0.2f);
                valText.alignment = TextAlignmentOptions.TopLeft;
    
                // Add ResourceDragHandler
                ResourceDragHandler dragHandler = iconGo.AddComponent<ResourceDragHandler>();
                dragHandler.resourceType = type;
    
                resourceSlots[type] = slotGo;
            }
    
            resourceInventoryPanel.SetActive(false);
        }
    
        public Sprite GetResourceSprite(ResourceType type)
        {
            if (GameManager.Instance != null && GameManager.Instance.resourceNodeDefinitions != null)
            {
                foreach (var def in GameManager.Instance.resourceNodeDefinitions)
                {
                    if (def != null && def.minedItemPrefab != null)
                    {
                        ConveyorItem citem = def.minedItemPrefab.GetComponentInChildren<ConveyorItem>();
                        if (citem != null && citem.resourceType == type)
                        {
                            SpriteRenderer sr = def.minedItemPrefab.GetComponentInChildren<SpriteRenderer>();
                            if (sr != null) return sr.sprite;
                        }
                    }
                }
            }
            return null;
        }
    
        public float GetResourceValue(ResourceType type)
        {
            if (GameManager.Instance != null && GameManager.Instance.resourceNodeDefinitions != null)
            {
                foreach (var def in GameManager.Instance.resourceNodeDefinitions)
                {
                    if (def != null && def.minedItemPrefab != null)
                    {
                        ConveyorItem citem = def.minedItemPrefab.GetComponentInChildren<ConveyorItem>();
                        if (citem != null && citem.resourceType == type)
                        {
                            return citem.value;
                        }
                    }
                }
            }
            switch (type)
            {
                case ResourceType.Ston: return 5f;
                case ResourceType.Copper: return 10f;
                case ResourceType.Iron: return 15f;
                case ResourceType.Quartz: return 20f;
                case ResourceType.Titanium: return 30f;
                case ResourceType.Diamond: return 50f;
                case ResourceType.Coal: return 4f;
                case ResourceType.Uranium: return 100f;
                default: return 10f;
            }
        }
    
        public void RefreshResourceInventoryPanel()
        {
            if (resourceInventoryPanel == null || !resourceInventoryPanel.activeSelf) return;
    
            foreach (var pair in resourceSlots)
            {
                ResourceType type = pair.Key;
                GameObject slotGo = pair.Value;
    
                Image iconImg = slotGo.transform.Find("Icon").GetComponent<Image>();
                TextMeshProUGUI countText = slotGo.transform.Find("CountText").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI valText = slotGo.transform.Find("ValueText").GetComponent<TextMeshProUGUI>();
                ResourceDragHandler dragHandler = iconImg.GetComponent<ResourceDragHandler>();
    
                int count = GetResourceCount(type);
                float value = GetResourceValue(type);
                Sprite sprite = GetResourceSprite(type);
    
                bool isDiscovered = discoveredResources.Contains(type);
    
                if (isDiscovered)
                {
                    iconImg.sprite = sprite;
                    iconImg.enabled = sprite != null;
                    dragHandler.iconSprite = sprite;
                    valText.text = $"${value}";
                    countText.text = count > 0 ? count.ToString() : "0";
    
                    if (count > 0)
                    {
                        iconImg.color = Color.white;
                        countText.color = new Color(0.2f, 1f, 0.2f);
                        valText.color = new Color(1f, 0.8f, 0.2f);
                    }
                    else
                    {
                        iconImg.color = new Color(1f, 1f, 1f, 0.25f);
                        countText.color = new Color(0.2f, 1f, 0.2f, 0.25f);
                        valText.color = new Color(1f, 0.8f, 0.2f, 0.25f);
                    }
                }
                else
                {
                    iconImg.enabled = false;
                    dragHandler.iconSprite = null;
                    valText.text = "???";
                    countText.text = "0";
    
                    iconImg.color = new Color(1f, 1f, 1f, 0.1f);
                    countText.color = new Color(0.2f, 1f, 0.2f, 0.1f);
                    valText.color = new Color(1f, 0.8f, 0.2f, 0.1f);
                }
            }
        }
    
        public bool IsMouseOverBuildingPanel(Vector2 screenPosition)
        {
            if (buildingPanel == null) return false;
            RectTransform rt = buildingPanel.GetComponent<RectTransform>();
            return RectTransformUtility.RectangleContainsScreenPoint(rt, screenPosition, null);
        }
    
        public bool IsMouseOverIntakeZone(Vector2 screenPosition)
        {
            if (idtIntakeDropZone == null || !idtIntakeDropZone.activeInHierarchy) return false;
            RectTransform rt = idtIntakeDropZone.GetComponent<RectTransform>();
            Camera cam = (HUDCanvas != null && HUDCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : Camera.main;
            return RectTransformUtility.RectangleContainsScreenPoint(rt, screenPosition, cam);
        }
    
        public void HandleResourceDropped(ResourceType type)
        {
            int count = GetResourceCount(type);
            if (count <= 0) return;
    
            if (currentOpenBuilding is InterDimensionalTransporter seller)
            {
                if (type == ResourceType.Coal)
                {
                    if (RemoveResource(type, count))
                    {
                        seller.fuelRemaining = Mathf.Min(seller.maxFuel, seller.fuelRemaining + 20f * count);
                        if (UiManager.HasInstance)
                        {
                            UiManager.Instance.ShowGeneralAlert($"REACTOR FUELED: +{20f * count}s", new Color(0.2f, 1f, 0.2f));
                        }
    
                        if (TutorialManager.HasInstance)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                TutorialManager.Instance.HandleFuelAdded(ResourceType.Coal);
                            }
                        }
                    }
                }
                else if (type == ResourceType.Uranium)
                {
                    if (RemoveResource(type, count))
                    {
                        seller.fuelRemaining = Mathf.Min(seller.maxFuel, seller.fuelRemaining + 60f * count);
                        seller.isUraniumBoosted = true;
                        seller.uraniumBoostDuration = Mathf.Min(120f, seller.uraniumBoostDuration + 30f * count);
                        if (UiManager.HasInstance)
                        {
                            UiManager.Instance.ShowGeneralAlert($"REACTOR BOOSTED: +{60f * count}s", new Color(0.3f, 1f, 1f));
                        }
    
                        if (TutorialManager.HasInstance)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                TutorialManager.Instance.HandleFuelAdded(ResourceType.Uranium);
                            }
                        }
                    }
                }
                else
                {
                    float val = GetResourceValue(type);
                    float mult = seller.isUraniumBoosted ? 2f : 1f;
                    float finalEarnings = count * val * mult;
    
                    if (RemoveResource(type, count))
                    {
                        CurrencyManager.Instance.AddCurrency(finalEarnings);
                        if (UiManager.HasInstance)
                        {
                            UiManager.Instance.ShowGeneralAlert($"ORES SOLD: +${finalEarnings:F0}", new Color(0.2f, 1f, 0.2f));
                        }
                    }
                }
    
                RefreshBuildingPanel();
                RefreshResourceInventoryPanel();
            }
        }
    
        #endregion
    
        #region Panel Control (Open/Close/Interact)
    
        public void OpenPanel(BuildingLogic building)
        {
            if (building == null || buildingPanel == null) return;
    
            currentOpenBuilding = building;
            buildingPanel.SetActive(true);
            RefreshBuildingPanel();
    
            if (building is InterDimensionalTransporter)
            {
                if (resourceInventoryPanel != null)
                {
                    resourceInventoryPanel.SetActive(true);
                    RefreshResourceInventoryPanel();
                }
            }
            else
            {
                if (resourceInventoryPanel != null)
                {
                    resourceInventoryPanel.SetActive(false);
                }
            }
    
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.StateMachine.ChangeState(PlayerController.Instance.StateMachine.buildingUiState);
            }
        }
    
        public void ClosePanel()
        {
            if (buildingPanel == null || !buildingPanel.activeSelf) return;
    
            buildingPanel.SetActive(false);
            currentOpenBuilding = null;
            lastConfiguredBuilding = null;
    
            if (resourceInventoryPanel != null)
            {
                resourceInventoryPanel.SetActive(false);
            }
    
            if (PlayerController.Instance != null && PlayerController.Instance.StateMachine.CurrentState == PlayerController.Instance.StateMachine.buildingUiState)
            {
                PlayerController.Instance.StateMachine.ChangeState(PlayerController.Instance.StateMachine.idleState);
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
    
            bool needsLayoutSetup = currentOpenBuilding != lastConfiguredBuilding;
    
            RectTransform detailRt = buildingDetailText.GetComponent<RectTransform>();
            RectTransform fuelBarRt = fuelBarContainer.GetComponent<RectTransform>();
    
            if (needsLayoutSetup)
            {
                lastConfiguredBuilding = currentOpenBuilding;
    
                // Default layout reset
                detailRt.anchorMin = new Vector2(0f, 0.48f);
                detailRt.anchorMax = new Vector2(1f, 0.85f);
                detailRt.offsetMin = new Vector2(30f, 0f);
                detailRt.offsetMax = new Vector2(-30f, 0f);
    
                fuelBarRt.anchorMin = new Vector2(0.5f, 0.38f);
                fuelBarRt.anchorMax = new Vector2(0.5f, 0.38f);
                fuelBarRt.pivot = new Vector2(0.5f, 0.5f);
                fuelBarRt.anchoredPosition = Vector2.zero;
                fuelBarRt.sizeDelta = new Vector2(750f, 50f);
    
                if (idtIntakeDropZone != null)
                {
                    idtIntakeDropZone.SetActive(false);
                }
    
                // Default buttons state
                actionButton1Go.SetActive(true);
                actionButton2Go.SetActive(true);
                actionButton1.onClick.RemoveAllListeners();
                actionButton2.onClick.RemoveAllListeners();
    
                // 1. InterDimensionalTransporter (Reactor) UI Setup
                if (currentOpenBuilding is InterDimensionalTransporter)
                {
                    buildingTitleText.text = "IDT REACTOR MODULE";
                    
                    // Hide the buttons as drag & drop is used
                    actionButton1Go.SetActive(false);
                    actionButton2Go.SetActive(false);
    
                    // Two-column layout overrides
                    if (idtIntakeDropZone != null)
                    {
                        idtIntakeDropZone.SetActive(true);
                    }
    
                    detailRt.anchorMin = new Vector2(0.05f, 0.32f);
                    detailRt.anchorMax = new Vector2(0.48f, 0.82f);
                    detailRt.offsetMin = Vector2.zero;
                    detailRt.offsetMax = Vector2.zero;
    
                    fuelBarRt.anchorMin = new Vector2(0.05f, 0.08f);
                    fuelBarRt.anchorMax = new Vector2(0.48f, 0.22f);
                    fuelBarRt.offsetMin = Vector2.zero;
                    fuelBarRt.offsetMax = Vector2.zero;
                }
                // 2. Miner UI Setup
                else if (currentOpenBuilding is MinerLogic miner)
                {
                    buildingTitleText.text = miner.data.buildingName.ToUpper();
                    fuelBarContainer.SetActive(true);
    
                    // Add Coal
                    actionButton1Text.text = "LOAD 1 COAL";
                    actionButton1.onClick.AddListener(() => {
                        if (RemoveResource(ResourceType.Coal, 1))
                        {
                            miner.fuelRemaining = Mathf.Min(miner.maxFuel, miner.fuelRemaining + 25f);
                            RefreshBuildingPanel();
                        }
                    });
    
                    // Deposit all Coal
                    actionButton2Text.text = "LOAD ALL COAL";
                    actionButton2.onClick.AddListener(() => {
                        int avail = GetResourceCount(ResourceType.Coal);
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
                // 3. Furnace UI Setup
                else if (currentOpenBuilding is Furnace furnace)
                {
                    buildingTitleText.text = furnace.data.buildingName.ToUpper();
                    fuelBarContainer.SetActive(true);
    
                    // Add Coal
                    actionButton1Text.text = "LOAD 1 COAL";
                    actionButton1.onClick.AddListener(() => {
                        if (RemoveResource(ResourceType.Coal, 1))
                        {
                            furnace.fuelRemaining = Mathf.Min(furnace.maxFuel, furnace.fuelRemaining + 25f);
                            RefreshBuildingPanel();
                        }
                    });
    
                    // Deposit all Coal
                    actionButton2Text.text = "LOAD ALL COAL";
                    actionButton2.onClick.AddListener(() => {
                        int avail = GetResourceCount(ResourceType.Coal);
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
                // 4. Turret UI Setup
                else if (currentOpenBuilding is TurretLogic turret)
                {
                    buildingTitleText.text = turret.data.buildingName.ToUpper();
                    fuelBarContainer.SetActive(true);
    
                    // Insert 30 Ammo
                    actionButton1Text.text = "INSERT 30 AMMO";
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
    
            // ================= DYNAMIC UPDATES (runs every frame) =================
            if (currentOpenBuilding is InterDimensionalTransporter seller)
            {
                string statusStr = seller.fuelRemaining > 0f ? "OPERATIONAL" : "OFFLINE (COAL REQUIRED)";
                if (seller.isUraniumBoosted) statusStr = "BOOSTED // URANIUM ACTIVE (2X VALUE)";
                
                buildingDetailText.text = $"Reactor Status: {statusStr}\n\n" +
                                          $"[DRAG COAL TO INTAKE PORT TO FUEL (+20s)]\n" +
                                          $"[DRAG URANIUM TO INTAKE PORT TO BOOST (+60s)]\n" +
                                          $"[DRAG OTHER ORES TO INTAKE PORT TO SELL]";
    
                fuelBarContainer.SetActive(true);
                float fuelPct = seller.fuelRemaining / seller.maxFuel;
                fuelBarFillImage.rectTransform.anchorMax = new Vector2(fuelPct, 1f);
                fuelBarText.text = $"Reactor Fuel: {Mathf.CeilToInt(seller.fuelRemaining)}s / {Mathf.CeilToInt(seller.maxFuel)}s";
            }
            else if (currentOpenBuilding is MinerLogic miner)
            {
                string statusStr = miner.fuelRemaining > 0f ? "EXTRACTING" : "OUT OF FUEL (COAL REQUIRED)";
                buildingDetailText.text = $"Machine Status: {statusStr}\n" +
                                          $"Extraction Speed: {miner.data.proccessingSpeed / miner.GetTierMultiplier():F1}s / cycle\n" +
                                          $"Carried Coal: {GetResourceCount(ResourceType.Coal)} available";
    
                fuelBarContainer.SetActive(true);
                float fuelPct = miner.fuelRemaining / miner.maxFuel;
                fuelBarFillImage.rectTransform.anchorMax = new Vector2(fuelPct, 1f);
                fuelBarText.text = $"Boiler Fuel: {Mathf.CeilToInt(miner.fuelRemaining)}% / 100%";
    
                actionButton1.interactable = GetResourceCount(ResourceType.Coal) > 0;
                actionButton2.interactable = GetResourceCount(ResourceType.Coal) > 0;
            }
            else if (currentOpenBuilding is Furnace furnace)
            {
                string statusStr = furnace.fuelRemaining > 0f ? "SMELTING" : "OUT OF FUEL (COAL REQUIRED)";
                buildingDetailText.text = $"Smelter Status: {statusStr}\n" +
                                          $"Cooking Speed: {furnace.data.proccessingSpeed / furnace.GetTierMultiplier():F1}s / cycle\n" +
                                          $"Carried Coal: {GetResourceCount(ResourceType.Coal)} available";
    
                fuelBarContainer.SetActive(true);
                float fuelPct = furnace.fuelRemaining / furnace.maxFuel;
                fuelBarFillImage.rectTransform.anchorMax = new Vector2(fuelPct, 1f);
                fuelBarText.text = $"Furnace Fuel: {Mathf.CeilToInt(furnace.fuelRemaining)}% / 100%";
    
                actionButton1.interactable = GetResourceCount(ResourceType.Coal) > 0;
                actionButton2.interactable = GetResourceCount(ResourceType.Coal) > 0;
            }
            else if (currentOpenBuilding is TurretLogic turret)
            {
                int reserve = PlayerController.Instance != null ? PlayerController.Instance.ammoReserve : 0;
                string statusStr = turret.ammoRemaining > 0 ? "ARMED // SCANNING" : "OUT OF AMMO // DEFENSE SYSTEM HALTED";
    
                buildingDetailText.text = $"Turret Status: {statusStr}\n" +
                                          $"Targeting Range: {turret.targetRange} tiles\n" +
                                          $"Ammo Reserves: {reserve} rounds carried";
    
                fuelBarContainer.SetActive(true);
                float ammoPct = (float)turret.ammoRemaining / turret.maxAmmo;
                fuelBarFillImage.rectTransform.anchorMax = new Vector2(ammoPct, 1f);
                fuelBarText.text = $"Mag Capacity: {turret.ammoRemaining} / {turret.maxAmmo}";
    
                actionButton1.interactable = reserve > 0 && turret.ammoRemaining < turret.maxAmmo;
                actionButton2.interactable = reserve > 0 && turret.ammoRemaining < turret.maxAmmo;
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
            bool isHoveringUi = false;
    
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
                                break;
                            }
                        }
    
                        // Check if it's a shop upgrade item
                        UpgradeShopItem shopItem = result.gameObject.GetComponentInParent<UpgradeShopItem>();
                        if (shopItem != null && shopItem.Definition != null)
                        {
                            isHoveringUi = true;
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
        }
    
        #endregion
    }
}
    


