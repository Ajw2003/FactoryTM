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
    using Buildings;
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

        public override void Setup(Buildings.BuildingData buildingData, Vector2Int cell)
        {
            Setup(buildingData, cell, 0);
        }

        public void Setup(Buildings.BuildingData minerData, Vector2Int cell, int rotationIndex)
        {
            base.Setup(minerData, cell);
            this.rotationIndex = rotationIndex;
            finiteOres = GameManager.Instance != null ? GameManager.Instance.finiteOres : false;

            // Setup runs before PlacementManager.SetOccupiedCells, so compute the footprint here
            // to find the assigned ResourceNode immediately.
            occupiedCells = ComputeFootprintCells();

            Vector2Int facing = GetFacingDirection();
            var outputEdges = GetEdgeCells(facing);
            if (outputEdges.Count > 0) CreatePortIndicator(outputEdges[0], facing, facing, new Color(0.3f, 1f, 0.3f), "Output");

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
                if (!isEnemyOwned)
                {
                    Debug.LogWarning($"Miner at {myCell} has no ResourceNode assigned. Disabling miner.");
                }
                enabled = false; // Disable the miner if no node is found
                return; // Exit setup early
            }
        }
    
        public override void PerformAction()
        {
            if (!enabled) return; // Ensure miner is enabled
    
            if (!isEnemyOwned)
            {
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
            if (itemComp != null && !isEnemyOwned && BuildingUiManager.Instance != null)
            {
                BuildingUiManager.Instance.DiscoverResource(itemComp.resourceType);
            }
            if (itemComp != null)
            {
                itemComp.Initialize(targetCell);
            }
    
            if (finiteOres)
            {
                assignedResourceNode.oreCount--;
            }
        }
    }
    
}


