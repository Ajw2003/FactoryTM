using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
namespace Placeables
{
    using System.Collections.Generic;
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
            List<ConveyorItem> items = ItemTracker.Instance.GetItemsInCell(myCell);
            if (items == null || items.Count == 0) return;
    
            bool hasNonFuelItemWaiting = false;
    
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
    }
    
}


