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
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class Furnace : BuildingLogic
    {
        [Header("Fuel Settings")]
        public float fuelRemaining = 30f;
        public float maxFuel = 100f;

        private float timer;
        private ConveyorItem currentItemPrefab;
        private ConveyorItem platePrefab;
        private float currentCookingSpeed;

        public override void Setup(Buildings.BuildingData buildingData, Vector2Int cell)
        {
            Setup(buildingData, cell, 0, buildingData != null ? buildingData.proccessingSpeed : 2f);
        }

        public void Setup(Buildings.BuildingData furnace, Vector2Int cell, int rotationIndex, float speed)
        {
            base.Setup(furnace, cell);
            this.rotationIndex = rotationIndex;
            currentCookingSpeed = speed / GetTierMultiplier();

            occupiedCells = ComputeFootprintCells();

            Vector2Int facing = GetFacingDirection();
            var outputEdges = GetEdgeCells(facing);
            var inputEdges = GetEdgeCells(-facing);
            if (outputEdges.Count > 0) CreatePortIndicator(outputEdges[0], facing, facing, new Color(0.3f, 1f, 0.3f), "Output");
            if (inputEdges.Count > 0) CreatePortIndicator(inputEdges[0], -facing, facing, new Color(0.3f, 0.8f, 1f), "Input");
        }

        // Furnaces only accept items entering through their single input side, opposite the output.
        public override bool CanAcceptInputFrom(Vector2Int incomingDirection)
        {
            return incomingDirection == GetFacingDirection();
        }
    
        private ResourceType lastProcessedResourceType = (ResourceType)(-1);
        private HashSet<ConveyorItem> itemsInProcess = new HashSet<ConveyorItem>();
    
        private void Update()
        {
            if (PauseManager.IsPaused) return;
    
            if (!isEnemyOwned)
            {
                if (fuelRemaining > 0f)
                {
                    fuelRemaining -= Time.deltaTime;
                    if (fuelRemaining < 0f) fuelRemaining = 0f;
                }
            }
        }
    
        public override void PerformAction()
        {
            if (!isEnemyOwned && fuelRemaining <= 0f) return;

            foreach (var inputCell in GetEdgeCells(-GetFacingDirection()))
            {
                List<ConveyorItem> items = ItemTracker.Instance.GetItemsInCell(inputCell);
                if (items != null)
                {
                    for (int i = items.Count - 1; i >= 0; i--)
                    {
                        ConveyorItem item = items[i];
                        if (!item.IsMoving && !itemsInProcess.Contains(item))
                        {
                            StartCoroutine(ProccessItem(item));
                        }
                    }
                }
            }
        }
        
        public IEnumerator ProccessItem(ConveyorItem item)
        {
            itemsInProcess.Add(item);
            
            float cookTimer = 0f;
            while (cookTimer < currentCookingSpeed)
            {
                if (PauseManager.IsPaused)
                {
                    yield return null;
                    continue;
                }
    
                if (!isEnemyOwned && fuelRemaining <= 0f)
                {
                    yield return null;
                    continue;
                }
    
                cookTimer += Time.deltaTime;
                yield return null;
            }
            
            if (item == null)
            {
                itemsInProcess.Remove(null);
                yield break;
            }
    
            // Only find the plate prefab if it's null or the resource type changed
            if (platePrefab == null || item.resourceType != lastProcessedResourceType)
            {
                string prefix = GameManager.GetPrefix(item.name);
    
                Debug.Log($"Furnace: Finding new plate prefab for '{item.name}' (prefix: '{prefix}')");
    
                ConveyorItem prefabGo = GameManager.FindPlatePrefabWithPrefix(prefix);
                if (prefabGo != null)
                {
                    platePrefab = prefabGo.GetComponent<ConveyorItem>();
                    lastProcessedResourceType = item.resourceType;
                }
                else
                {
                    Debug.LogWarning($"Furnace: Could not find plate prefab for prefix '{prefix}'");
                    itemsInProcess.Remove(item);
                    yield break;
                }
            }
    
            if (platePrefab != null)
            {
                CookItem(platePrefab);
                ObjectPoolManager.Instance.ReturnToPool(item.gameObject);
            }
            
            itemsInProcess.Remove(item);
        }
    
        void CookItem(ConveyorItem CookedItemToRecive)
        {
            if (CookedItemToRecive == null)
            {
                Debug.LogError($"Furnace at {myCell} has no item to cook!"); // Changed from Miner to Furnace
                return;
            }
            currentItemPrefab = CookedItemToRecive;

            Vector2 spawnPos = GetOutputSpawnPosition(GetFacingDirection(), out Vector2Int targetCell);

            // 2. Instantiate the item from pool
            GameObject newItem = ObjectPoolManager.Instance.GetPooledObject(currentItemPrefab.gameObject, spawnPos, Quaternion.identity);
            ConveyorItem itemComp = newItem.GetComponent<ConveyorItem>();
            if (itemComp != null)
            {
                itemComp.Initialize(targetCell);
            }
        }
    }
    
}


