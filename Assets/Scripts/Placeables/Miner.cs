using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
namespace Placeables
{
    using System.Collections.Generic;
    using Buildings;
    using Items;
    using UnityEngine;
    using UnityEngine.Tilemaps;

    public class MinerLogic : BuildingLogic
    {
        [Header("Fuel Settings")]
        public float fuelRemaining = 30f;
        public float maxFuel = 100f;

        private float timer;
        private ResourceNode assignedResourceNode;
        private GameObject currentMinedItemPrefab;
        private float currentMiningSpeed;
        private bool finiteOres;

        /// <summary>Live counted view backing this Miner's panel - reparented into
        /// BuildingUiManager's shared port slots while open.</summary>
        public InventorySlot OutputSlot { get; private set; }
        public InventorySlot FuelInputSlot { get; private set; }

        public override void Setup(Buildings.BuildingData buildingData, Vector2Int cell)
        {
            Setup(buildingData, cell, 0);
        }

        public void Setup(Buildings.BuildingData minerData, Vector2Int cell, int rotationIndex)
        {
            base.Setup(minerData, cell);
            this.rotationIndex = rotationIndex;
            finiteOres = GameManager.Instance != null ? GameManager.Instance.FiniteOres : false;

            // Setup runs before PlacementManager.SetOccupiedCells, so compute the footprint here
            // to find the assigned ResourceNode immediately.
            occupiedCells = ComputeFootprintCells();

            Vector2Int facing = GetFacingDirection();
            var outputEdges = GetEdgeCells(facing);
            var inputEdges = GetEdgeCells(-facing);
            if (outputEdges.Count > 0) CreatePortIndicator(outputEdges[0], facing, facing, new Color(0.3f, 1f, 0.3f), "Output");
            if (inputEdges.Count > 0) CreatePortIndicator(inputEdges[0], -facing, facing, new Color(0.3f, 0.8f, 1f), "Input");

            OutputSlot = BuildingSlotFactory.CreateSlot(transform);
            FuelInputSlot = BuildingSlotFactory.CreateSlot(transform);
            FuelInputSlot.allowedTypes = new HashSet<specificItemType> { specificItemType.Coal, specificItemType.Uranium };

            // Attempt to find a ResourceNode at any of the miner's positions
            if (ResourceManager.Instance != null)
            {
                foreach (var occupiedCell in occupiedCells)
                {
                    assignedResourceNode = ResourceManager.Instance.GetNodeAtPosition(occupiedCell);
                    if (assignedResourceNode != null) break;
                }
            }
    
            if (assignedResourceNode != null)
            {
                currentMinedItemPrefab = assignedResourceNode.minedItemPrefab;
                currentMiningSpeed = assignedResourceNode.miningSpeed / GetTierMultiplier();
                timer = currentMiningSpeed; // Initialize timer with the node's speed
            }
            else
            {
                if (!IsEnemyOwned)
                {
                    Debug.LogWarning($"Miner at {myCell} has no ResourceNode assigned. Disabling miner.");
                }
                enabled = false; // Disable the miner if no node is found
                return; // Exit setup early
            }
        }

        // Miners don't process belt items, but they can be auto-fed Coal for their boiler on
        // their input side - the same fuel a player can otherwise load manually via the panel.
        public override bool CanAcceptInputFrom(Vector2Int incomingDirection)
        {
            return incomingDirection == GetFacingDirection();
        }

        private const float FuelPerCoal = 25f;
        private const float FuelPerUranium = 75f;

        public override void PerformAction()
        {
            if (!enabled) return; // Ensure miner is enabled

            if (!IsEnemyOwned)
            {
                AbsorbFuelFromInput();
                DrainFuelSlot();

                if (fuelRemaining > 0f)
                {
                    fuelRemaining -= Time.deltaTime;
                    if (fuelRemaining < 0f) fuelRemaining = 0f;

                    timer -= Time.deltaTime;
                    if (timer <= 0)
                    {
                        SpawnItem();
                        timer = currentMiningSpeed; // Reset timer with the node's speed
                    }
                }
            }
            else
            {
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    SpawnItem();
                    timer = currentMiningSpeed;
                }
            }
        }

        /// <summary>Absorbs any stationary Coal/Uranium sitting on the input side into FuelInputSlot -
        /// TryAdd itself rejects anything else via the slot's allowedTypes filter.</summary>
        private void AbsorbFuelFromInput()
        {
            if (FuelInputSlot == null) return;

            foreach (var inputCell in GetInputCells())
            {
                var items = ItemTracker.Instance != null ? ItemTracker.Instance.GetItemsInCell(inputCell) : null;
                if (items == null) continue;

                for (int i = items.Count - 1; i >= 0; i--)
                {
                    ConveyorItem item = items[i];
                    if (item != null && !item.IsMoving && FuelInputSlot.TryAdd(item._itemData, 1))
                    {
                        ObjectPoolManager.Instance.ReturnToPool(item.gameObject);
                    }
                }
            }
        }

        /// <summary>Only pulls from the fuel reserve once the boiler actually runs dry, so the slot's
        /// count stays visible as a buffer instead of flickering back to empty every frame.</summary>
        private void DrainFuelSlot()
        {
            if (FuelInputSlot == null || !FuelInputSlot.slotFilled || fuelRemaining > 0f) return;

            float amount = FuelInputSlot._itemData.specificItemType == specificItemType.Uranium ? FuelPerUranium : FuelPerCoal;
            fuelRemaining = Mathf.Min(maxFuel, fuelRemaining + amount);
            FuelInputSlot.TryRemove(1);
        }
    
        void SpawnItem()
        {
            if (currentMinedItemPrefab == null || finiteOres && assignedResourceNode.oreCount <= 0)
            {
                Debug.LogError($"Miner at {myCell} has no item prefab to mine!");
                return;
            }
    
            Vector2 spawnPos = GetOutputSpawnPosition(GetFacingDirection(), out Vector2Int targetCell);

            // 2. Instantiate the item from pool
            GameObject newItem = ObjectPoolManager.Instance.GetPooledObject(currentMinedItemPrefab, spawnPos, Quaternion.identity);
            
            if (Managers.GameStatsManager.HasInstance)
            {
                Managers.GameStatsManager.Instance.IncrementOreMined();
            }
    
            ConveyorItem itemComp = newItem.GetComponent<ConveyorItem>();
            if (itemComp != null && !IsEnemyOwned && BuildingUiManager.Instance != null)
            {
                BuildingUiManager.Instance.DiscoverResource(itemComp._itemData);
            }
            if (itemComp != null)
            {
                itemComp.Initialize(targetCell);
                OutputSlot?.TryAdd(itemComp._itemData, 1);
            }
    
            if (finiteOres)
            {
                assignedResourceNode.oreCount--;
            }
        }
    }
    
}


