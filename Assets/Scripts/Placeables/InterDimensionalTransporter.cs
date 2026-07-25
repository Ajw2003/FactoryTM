using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
namespace Placeables
{
    using System.Collections.Generic;
    using Items;
    using UnityEngine;

    public class InterDimensionalTransporter : BuildingLogic
    {
        [Header("Fuel Settings")]
        public float fuelRemaining = 0f;
        public float maxFuel = 100f;
        public bool isUraniumBoosted = false;
        public float uraniumBoostDuration = 0f;

        private float lastFuelWarningTime = -999f;
        private const float FuelWarningCooldown = 8f;

        public static InterDimensionalTransporter Instance { get; private set; }

        public delegate void FuelAddedAction(specificItemType type);
        public event FuelAddedAction OnFuelAdded;

        public delegate void ItemSoldAction(ConveyorItem item);
        public event ItemSoldAction OnItemSold;

        /// <summary>Real drag targets, reparented into BuildingUiManager's intake zone while this
        /// panel is open. FuelSlot only accepts Coal/Uranium and is the visible fuel reserve; SellSlot
        /// takes anything else and sells it once fuel is available.</summary>
        public InventorySlot FuelSlot { get; private set; }
        public InventorySlot SellSlot { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public override void Setup(Buildings.BuildingData buildingData, Vector2Int cell)
        {
            Setup(buildingData, cell, 0);
        }

        public void Setup(Buildings.BuildingData buildingData, Vector2Int cell, int rotationIndex)
        {
            base.Setup(buildingData, cell);
            this.rotationIndex = rotationIndex;
            fuelRemaining = 0f; // Start empty to force tutorial Coal extraction

            Vector2Int[] allDirections = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var dir in allDirections)
            {
                CreatePortIndicator(myCell, dir, -dir, new Color(0.3f, 0.8f, 1f), "Input_" + dir);
            }

            FuelSlot = BuildingSlotFactory.CreateSlot(transform);
            FuelSlot.allowedTypes = new HashSet<specificItemType> { specificItemType.Coal, specificItemType.Uranium };
            SellSlot = BuildingSlotFactory.CreateSlot(transform);
        }

        // The IDT accepts items from any side.
        public override bool CanAcceptInputFrom(Vector2Int incomingDirection)
        {
            return true;
        }
    
        private void Update()
        {
            if (PauseManager.IsPaused) return;
    
            if (fuelRemaining > 0f)
            {
                fuelRemaining -= Time.deltaTime;
                if (fuelRemaining < 0f) fuelRemaining = 0f;
            }
    
            if (isUraniumBoosted)
            {
                uraniumBoostDuration -= Time.deltaTime;
                if (uraniumBoostDuration <= 0f)
                {
                    isUraniumBoosted = false;
                    uraniumBoostDuration = 0f;
                    if (UiManager.HasInstance)
                    {
                        UiManager.Instance.ShowGeneralAlert("IDT EFFICIENCY BOOST ENDED", new Color(0.5f, 0.5f, 0.5f));
                    }
                }
            }
        }
    
        public override void PerformAction()
        {
            bool hasNonFuelItemWaiting = false;

            List<ConveyorItem> items = ItemTracker.Instance.GetItemsInCell(myCell);
            if (items != null)
            {
                for (int i = items.Count - 1; i >= 0; i--)
                {
                    ConveyorItem item = items[i];
                    if (item == null) continue;

                    if (!item.IsMoving)
                    {
                        if (item.itemType == specificItemType.Coal)
                        {
                            fuelRemaining = Mathf.Min(maxFuel, fuelRemaining + 20f);
                            OnFuelAdded?.Invoke(specificItemType.Coal);
                            if (UiManager.HasInstance)
                            {
                                UiManager.Instance.ShowGeneralAlert("IDT FUELED: COAL (+20s)", new Color(0.3f, 0.9f, 0.3f));
                            }
                            ObjectPoolManager.Instance.ReturnToPool(item.gameObject);
                        }
                        else if (item.itemType == specificItemType.Uranium)
                        {
                            fuelRemaining = Mathf.Min(maxFuel, fuelRemaining + 60f);
                            isUraniumBoosted = true;
                            uraniumBoostDuration = 30f;
                            OnFuelAdded?.Invoke(specificItemType.Uranium);
                            if (UiManager.HasInstance)
                            {
                                UiManager.Instance.ShowGeneralAlert("IDT BOOSTED: URANIUM (+60s, 2X OUTPUT)", new Color(0.3f, 1f, 1f));
                            }
                            ObjectPoolManager.Instance.ReturnToPool(item.gameObject);
                        }
                        else
                        {
                            if (fuelRemaining > 0f)
                            {
                                float saleValue = item.value;
                                if (isUraniumBoosted)
                                {
                                    saleValue *= 2f;
                                }

                                CurrencyManager.Instance.AddCurrency(saleValue);
                                OnItemSold?.Invoke(item);
                                ObjectPoolManager.Instance.ReturnToPool(item.gameObject);
                            }
                            else
                            {
                                hasNonFuelItemWaiting = true;
                            }
                        }
                    }
                }
            }

            // Player-dragged resources land in these two slots instead of on the physical belt cell.
            ProcessFuelSlot();
            ProcessSellSlot(ref hasNonFuelItemWaiting);

            if (hasNonFuelItemWaiting && fuelRemaining <= 0f)
            {
                if (Time.time - lastFuelWarningTime >= FuelWarningCooldown)
                {
                    lastFuelWarningTime = Time.time;
                    if (UiManager.HasInstance)
                    {
                        UiManager.Instance.ShowGeneralAlert("IDT OFFLINE: COAL FUEL REQUIRED!", new Color(1f, 0.3f, 0.3f));
                    }
                }
            }
        }

        /// <summary>Only pulls from the fuel reserve once it actually runs dry, so FuelSlot's count
        /// stays visible as a buffer - it IS the fuel gauge, instead of flickering back to empty
        /// every frame.</summary>
        private void ProcessFuelSlot()
        {
            if (FuelSlot == null || !FuelSlot.slotFilled || fuelRemaining > 0f) return;

            specificItemType type = FuelSlot._itemData.specificItemType;
            if (type == specificItemType.Uranium)
            {
                fuelRemaining = Mathf.Min(maxFuel, fuelRemaining + 60f);
                isUraniumBoosted = true;
                uraniumBoostDuration = 30f;
                OnFuelAdded?.Invoke(specificItemType.Uranium);
                if (UiManager.HasInstance)
                {
                    UiManager.Instance.ShowGeneralAlert("IDT BOOSTED: URANIUM (+60s, 2X OUTPUT)", new Color(0.3f, 1f, 1f));
                }
            }
            else
            {
                fuelRemaining = Mathf.Min(maxFuel, fuelRemaining + 20f);
                OnFuelAdded?.Invoke(specificItemType.Coal);
                if (UiManager.HasInstance)
                {
                    UiManager.Instance.ShowGeneralAlert("IDT FUELED: COAL (+20s)", new Color(0.3f, 0.9f, 0.3f));
                }
            }

            FuelSlot.TryRemove(1);
        }

        /// <summary>Sells whatever's stacked in SellSlot in one go, once fuel is available.</summary>
        private void ProcessSellSlot(ref bool hasNonFuelItemWaiting)
        {
            if (SellSlot == null || !SellSlot.slotFilled) return;

            if (fuelRemaining <= 0f)
            {
                hasNonFuelItemWaiting = true;
                return;
            }

            specificItemType type = SellSlot._itemData.specificItemType;
            float unitValue = BuildingUiManager.HasInstance ? BuildingUiManager.Instance.GetResourceValue(type) : 10f;
            if (isUraniumBoosted)
            {
                unitValue *= 2f;
            }

            int count = SellSlot.itemCount;
            CurrencyManager.Instance.AddCurrency(unitValue * count);
            OnItemSold?.Invoke(null);
            SellSlot.TryRemove(count);
        }
    }
    
}


