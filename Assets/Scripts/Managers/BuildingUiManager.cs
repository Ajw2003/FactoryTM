using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
using Items;

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
        private HashSet<ItemData> discoveredResources = new HashSet<ItemData>();
    
        [Header("Player Resource Inventory")]
        private Dictionary<ItemData, int> playerResources = new Dictionary<ItemData, int>();
        private Dictionary<specificItemType, int> playerResourcesByType = new Dictionary<specificItemType, int>();
    
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
    
        private GameObject idtIntakeDropZone;

        private GameObject chestInventoryZone;

        private GameObject portStatusZone;
        private Transform portInputRegion;
        private Transform portOutputRegion;
        private Transform idtFuelSlotRegion;
        private Transform idtSellSlotRegion;
        private Transform chestSlotGrid;

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
    
        #region Discovery
    
        public void DiscoverResource(ItemData type)
        {
            if (discoveredResources.Add(type))
            {
                if (UiManager.HasInstance)
                {
                    UiManager.Instance.ShowGeneralAlert($"NEW RESOURCE IDENTIFIED: {type.itemName.ToUpper()}", new Color(0.2f, 1f, 1f));
                }
            }
        }
    
        public bool IsResourceDiscovered(ItemData type)
        {
            return discoveredResources.Contains(type);
        }
    
        public int GetResourceCount(specificItemType type)
        {
            if (playerResourcesByType.TryGetValue(type, out int count))
            {
                return count;
            }
            return 0;
        }
    
        public void AddResource(ItemData data, int count = 1)
        {
            if (playerResources.ContainsKey(data))
            {
                playerResources[data] += count;
                playerResourcesByType[data.specificItemType] +=  count;
            }
            else
            {
                playerResources[data] = count;
                playerResourcesByType[data.specificItemType] = count;
            }

            ResourcesToHotBar(data, count);
            DiscoverResource(data);

            if (TutorialManager.HasInstance)
            {
                TutorialManager.Instance.UpdateObjectiveText();
            }
        }

        /// <summary>Adds `amount` of `itemData` into the first hotbar slot that already holds
        /// that type, or the first empty slot if none does. Shared entry point for both mined
        /// resources (AddResource) and purchased buildings (see UpgradeShopItem/UiItemButton).</summary>
        public void ResourcesToHotBar(ItemData itemData, int amount = 1)
        {
            foreach (var slot in HotbarUI.Instance.slots)
            {
                if (slot.TryAdd(itemData, amount))
                {
                    break;
                }
            }
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
    
            // Zone title
            GameObject labelGo = new GameObject("LabelText", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(idtIntakeDropZone.transform, false);
            RectTransform labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0f, 0.86f);
            labelRt.anchorMax = new Vector2(1f, 0.98f);
            labelRt.offsetMin = new Vector2(10f, 0f);
            labelRt.offsetMax = new Vector2(-10f, 0f);
            TextMeshProUGUI labelTxt = labelGo.GetComponent<TextMeshProUGUI>();
            labelTxt.text = "REACTOR INTAKE";
            labelTxt.fontSize = 26;
            labelTxt.fontStyle = FontStyles.Bold;
            labelTxt.color = new Color(0.2f, 1f, 0.2f);
            labelTxt.alignment = TextAlignmentOptions.Center;

            // Two regions the IDT's real FuelSlot/SellSlot get reparented into while its panel is
            // open - drag targets the same way any other InventorySlot is, side by side within the zone.
            idtFuelSlotRegion = CreatePortStatusRegion(idtIntakeDropZone.transform, "FuelSlot", "FUEL", new Vector2(0.05f, 0.05f), new Vector2(0.48f, 0.8f));
            idtSellSlotRegion = CreatePortStatusRegion(idtIntakeDropZone.transform, "SellSlot", "SELL", new Vector2(0.52f, 0.05f), new Vector2(0.95f, 0.8f));

            // Chest inventory zone - occupies the same region as the IDT intake drop zone, but shows
            // a small multi-slot grid of whatever resource types the chest currently holds.
            chestInventoryZone = new GameObject("ChestInventoryZone", typeof(RectTransform), typeof(Image));
            chestInventoryZone.transform.SetParent(buildingPanel.transform, false);
            RectTransform chestRt = chestInventoryZone.GetComponent<RectTransform>();
            chestRt.anchorMin = new Vector2(0.52f, 0.08f);
            chestRt.anchorMax = new Vector2(0.95f, 0.82f);
            chestRt.pivot = new Vector2(0.5f, 0.5f);
            chestRt.offsetMin = Vector2.zero;
            chestRt.offsetMax = Vector2.zero;

            Image chestImg = chestInventoryZone.GetComponent<Image>();
            chestImg.color = new Color(0.01f, 0.08f, 0.01f, 0.9f);
            Outline chestOutline = chestInventoryZone.AddComponent<Outline>();
            chestOutline.effectColor = new Color(0.2f, 0.9f, 0.2f, 0.8f);
            chestOutline.effectDistance = new Vector2(2f, -2f);

            GameObject chestGridGo = new GameObject("SlotGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            chestGridGo.transform.SetParent(chestInventoryZone.transform, false);
            RectTransform chestGridRt = chestGridGo.GetComponent<RectTransform>();
            chestGridRt.anchorMin = new Vector2(0.05f, 0.05f);
            chestGridRt.anchorMax = new Vector2(0.95f, 0.95f);
            chestGridRt.offsetMin = Vector2.zero;
            chestGridRt.offsetMax = Vector2.zero;
            GridLayoutGroup chestGridLayout = chestGridGo.GetComponent<GridLayoutGroup>();
            chestGridLayout.cellSize = new Vector2(100f, 100f);
            chestGridLayout.spacing = new Vector2(10f, 10f);
            chestGridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            chestGridLayout.constraintCount = 2;
            chestSlotGrid = chestGridGo.transform;

            chestInventoryZone.SetActive(false);

            // Port status zone - shared by Miner/Furnace, hosting whichever of those buildings is
            // currently open own real input/output InventorySlot(s).
            portStatusZone = new GameObject("PortStatusZone", typeof(RectTransform));
            portStatusZone.transform.SetParent(buildingPanel.transform, false);
            RectTransform portZoneRt = portStatusZone.GetComponent<RectTransform>();
            portZoneRt.anchorMin = new Vector2(0.05f, 0.32f);
            portZoneRt.anchorMax = new Vector2(0.95f, 0.48f);
            portZoneRt.offsetMin = Vector2.zero;
            portZoneRt.offsetMax = Vector2.zero;

            portInputRegion = CreatePortStatusRegion(portStatusZone.transform, "FuelSlot", "FUEL", new Vector2(0f, 0f), new Vector2(0.32f, 1f));
            portOutputRegion = CreatePortStatusRegion(portStatusZone.transform, "OutputSlot", "OUTPUT", new Vector2(0.68f, 0f), new Vector2(1f, 1f));
            portStatusZone.SetActive(false);

            buildingPanel.SetActive(false);
        }

        /// <summary>Builds the label + background chrome for a Miner/Furnace port slot and returns the
        /// inner region that the building's own real InventorySlot gets reparented into while open.</summary>
        private Transform CreatePortStatusRegion(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject slotGo = new GameObject(name, typeof(RectTransform), typeof(Image));
            slotGo.transform.SetParent(parent, false);
            RectTransform slotRt = slotGo.GetComponent<RectTransform>();
            slotRt.anchorMin = anchorMin;
            slotRt.anchorMax = anchorMax;
            slotRt.offsetMin = Vector2.zero;
            slotRt.offsetMax = Vector2.zero;

            Image slotImg = slotGo.GetComponent<Image>();
            slotImg.color = new Color(0f, 0.02f, 0f, 0.8f);
            Outline slotOutline = slotGo.AddComponent<Outline>();
            slotOutline.effectColor = new Color(0.2f, 0.9f, 0.2f, 0.4f);
            slotOutline.effectDistance = new Vector2(1f, -1f);

            GameObject labelGo = new GameObject("LabelText", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(slotGo.transform, false);
            RectTransform labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0f, 0f);
            labelRt.anchorMax = new Vector2(1f, 0.2f);
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;
            TextMeshProUGUI labelTxt = labelGo.GetComponent<TextMeshProUGUI>();
            labelTxt.text = label;
            labelTxt.fontSize = 16;
            labelTxt.fontStyle = FontStyles.Bold;
            labelTxt.color = new Color(0.2f, 0.8f, 0.2f, 0.8f);
            labelTxt.alignment = TextAlignmentOptions.Center;

            GameObject regionGo = new GameObject("SlotRegion", typeof(RectTransform));
            regionGo.transform.SetParent(slotGo.transform, false);
            RectTransform regionRt = regionGo.GetComponent<RectTransform>();
            regionRt.anchorMin = new Vector2(0.1f, 0.25f);
            regionRt.anchorMax = new Vector2(0.9f, 0.95f);
            regionRt.offsetMin = Vector2.zero;
            regionRt.offsetMax = Vector2.zero;

            return regionGo.transform;
        }
    
        
    
        public Sprite GetResourceSprite(specificItemType type)
        {
            if (GameManager.Instance != null && GameManager.Instance.resourceNodeDefinitions != null)
            {
                foreach (var def in GameManager.Instance.resourceNodeDefinitions)
                {
                    if (def != null && def.minedItemPrefab != null)
                    {
                        ConveyorItem citem = def.minedItemPrefab.GetComponentInChildren<ConveyorItem>();
                        if (citem != null && citem.itemType == type)
                        {
                            SpriteRenderer sr = def.minedItemPrefab.GetComponentInChildren<SpriteRenderer>();
                            if (sr != null) return sr.sprite;
                        }
                    }
                }
            }
            return null;
        }
    
        /// <summary>The raw-ore item prefab for a resource type, looked up the same way GetResourceSprite finds its icon.</summary>
        public GameObject GetResourceItemPrefab(specificItemType type)
        {
            if (GameManager.Instance != null && GameManager.Instance.resourceNodeDefinitions != null)
            {
                foreach (var def in GameManager.Instance.resourceNodeDefinitions)
                {
                    if (def != null && def.minedItemPrefab != null)
                    {
                        ConveyorItem citem = def.minedItemPrefab.GetComponentInChildren<ConveyorItem>();
                        if (citem != null && citem.itemType == type)
                        {
                            return def.minedItemPrefab;
                        }
                    }
                }
            }
            return null;
        }

        public float GetResourceValue(specificItemType type)
        {
            if (GameManager.Instance != null && GameManager.Instance.resourceNodeDefinitions != null)
            {
                foreach (var def in GameManager.Instance.resourceNodeDefinitions)
                {
                    if (def != null && def.minedItemPrefab != null)
                    {
                        ConveyorItem citem = def.minedItemPrefab.GetComponentInChildren<ConveyorItem>();
                        if (citem != null && citem.itemType == type)
                        {
                            return citem.value;
                        }
                    }
                }
            }
            switch (type)
            {
                case specificItemType.Stone: return 5f;
                case specificItemType.Copper: return 10f;
                case specificItemType.Iron: return 15f;
                case specificItemType.Quartz: return 20f;
                case specificItemType.Titanium: return 30f;
                case specificItemType.Diamond: return 50f;
                case specificItemType.Coal: return 4f;
                case specificItemType.Uranium: return 100f;
                default: return 10f;
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

        public bool IsMouseOverChestZone(Vector2 screenPosition)
        {
            if (chestInventoryZone == null || !chestInventoryZone.activeInHierarchy) return false;
            RectTransform rt = chestInventoryZone.GetComponent<RectTransform>();
            Camera cam = (HUDCanvas != null && HUDCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : Camera.main;
            return RectTransformUtility.RectangleContainsScreenPoint(rt, screenPosition, cam);
        }

        /// <summary>Hands whichever real InventorySlot(s) `building` owns back to it (reparented off
        /// the panel, deactivated) so the panel can display a different building next.</summary>
        private void HideBuildingSlots(BuildingLogic building)
        {
            if (building == null) return; // Unity-null: also true if the building was destroyed since it was last configured

            if (building is Furnace furnace)
            {
                ReturnSlotToOwner(furnace.FuelSlot, furnace.transform);
                ReturnSlotToOwner(furnace.OutputSlot, furnace.transform);
            }
            else if (building is MinerLogic miner)
            {
                ReturnSlotToOwner(miner.FuelInputSlot, miner.transform);
                ReturnSlotToOwner(miner.OutputSlot, miner.transform);
            }
            else if (building is InterDimensionalTransporter idt)
            {
                ReturnSlotToOwner(idt.FuelSlot, idt.transform);
                ReturnSlotToOwner(idt.SellSlot, idt.transform);
            }
            else if (building is Chest chest && chest.Slots != null)
            {
                foreach (var slot in chest.Slots)
                {
                    ReturnSlotToOwner(slot, chest.transform);
                }
            }
        }

        /// <summary>Reparents `building`'s own real InventorySlot(s) into this panel's designated
        /// region(s) and activates the surrounding zone, so this is the only building whose slots
        /// are currently live/interactable.</summary>
        private void ShowBuildingSlots(BuildingLogic building)
        {
            if (building == null) return;

            if (building is Furnace furnace)
            {
                portStatusZone.SetActive(true);
                PlaceSlotInRegion(furnace.FuelSlot, portInputRegion);
                PlaceSlotInRegion(furnace.OutputSlot, portOutputRegion);
            }
            else if (building is MinerLogic miner)
            {
                portStatusZone.SetActive(true);
                PlaceSlotInRegion(miner.FuelInputSlot, portInputRegion);
                PlaceSlotInRegion(miner.OutputSlot, portOutputRegion);
            }
            else if (building is InterDimensionalTransporter idt)
            {
                idtIntakeDropZone.SetActive(true);
                PlaceSlotInRegion(idt.FuelSlot, idtFuelSlotRegion);
                PlaceSlotInRegion(idt.SellSlot, idtSellSlotRegion);
            }
            else if (building is Chest chest && chest.Slots != null)
            {
                chestInventoryZone.SetActive(true);
                foreach (var slot in chest.Slots)
                {
                    if (slot == null) continue;
                    slot.transform.SetParent(chestSlotGrid, false);
                    slot.gameObject.SetActive(true);
                }
            }
        }

        private void ReturnSlotToOwner(InventorySlot slot, Transform owner)
        {
            if (slot == null) return;
            slot.transform.SetParent(owner, false);
            slot.gameObject.SetActive(false);
        }

        // The InventorySlot prefab is authored as a large (200x200) square meant for the hotbar -
        // stretching it to fill an odd-shaped region distorts it and throws off its internally
        // positioned name/count text. Center it at a fixed, panel-appropriate size instead.
        private static readonly Vector2 DefaultSlotSize = new Vector2(72f, 72f);

        private void PlaceSlotInRegion(InventorySlot slot, Transform region)
        {
            PlaceSlotInRegion(slot, region, DefaultSlotSize);
        }

        private void PlaceSlotInRegion(InventorySlot slot, Transform region, Vector2 size)
        {
            if (slot == null || region == null) return;

            slot.transform.SetParent(region, false);
            RectTransform rt = slot.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = size;
            slot.gameObject.SetActive(true);
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
                PlayerController.Instance.StateMachine.ChangeState(PlayerController.Instance.StateMachine.buildingUiState);
            }
        }
    
        public void ClosePanel()
        {
            if (buildingPanel == null || !buildingPanel.activeSelf) return;

            buildingPanel.SetActive(false);
            HideBuildingSlots(lastConfiguredBuilding);
            currentOpenBuilding = null;
            lastConfiguredBuilding = null;

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

            RectTransform fuelBarRt = fuelBarContainer.GetComponent<RectTransform>();

            if (needsLayoutSetup)
            {
                HideBuildingSlots(lastConfiguredBuilding);
                lastConfiguredBuilding = currentOpenBuilding;

                buildingTitleText.text = currentOpenBuilding.data != null ? currentOpenBuilding.data.buildingName.ToUpper() : "BUILDING";

                // Buttons, the description, and the fuel/progress bar are deprecated on every panel
                // except the Furnace's cook-progress bar - slots are the primary UI now.
                buildingDetailText.gameObject.SetActive(false);
                actionButton1Go.SetActive(false);
                actionButton2Go.SetActive(false);
                actionButton1.onClick.RemoveAllListeners();
                actionButton2.onClick.RemoveAllListeners();

                if (idtIntakeDropZone != null)
                {
                    idtIntakeDropZone.SetActive(false);
                }

                if (chestInventoryZone != null)
                {
                    chestInventoryZone.SetActive(false);
                }

                if (portStatusZone != null)
                {
                    portStatusZone.SetActive(false);
                }

                bool isFurnace = currentOpenBuilding is Furnace;
                fuelBarContainer.SetActive(isFurnace);
                if (isFurnace)
                {
                    fuelBarRt.anchorMin = new Vector2(0.5f, 0.2f);
                    fuelBarRt.anchorMax = new Vector2(0.5f, 0.2f);
                    fuelBarRt.pivot = new Vector2(0.5f, 0.5f);
                    fuelBarRt.anchoredPosition = Vector2.zero;
                    fuelBarRt.sizeDelta = new Vector2(750f, 50f);
                }

                if (currentOpenBuilding is InterDimensionalTransporter)
                {
                    buildingTitleText.text = "IDT REACTOR MODULE";
                }

                ShowBuildingSlots(currentOpenBuilding);
            }
        }


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
                        if (HoveredBuilding != null
                            && HoveredBuilding.data.type != BuildingType.Conveyor
                            && HoveredBuilding.data.type != BuildingType.Wall
                            && HoveredBuilding.data.type != BuildingType.Turret)
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
    


