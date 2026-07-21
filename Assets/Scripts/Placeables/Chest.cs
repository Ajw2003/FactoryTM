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

    public class Chest : BuildingLogic
    {
        public int capacity = 20;

        // Stored as counts per resource type (mirrors the player's abstract resource pool in
        // BuildingUiManager) rather than live ConveyorItem instances, so the UI can show slot
        // counts and the player can freely deposit/withdraw without needing a physical item on hand.
        private readonly Dictionary<ResourceType, int> storedResources = new Dictionary<ResourceType, int>();

        public override void Setup(Buildings.BuildingData buildingData, Vector2Int cell)
        {
            Setup(buildingData, cell, 0);
        }

        public void Setup(Buildings.BuildingData chestData, Vector2Int cell, int rotationIndex)
        {
            base.Setup(chestData, cell);
            this.rotationIndex = rotationIndex;

            occupiedCells = ComputeFootprintCells();

            Vector2Int facing = GetFacingDirection();
            var outputEdges = GetEdgeCells(facing);
            var inputEdges = GetEdgeCells(-facing);
            if (outputEdges.Count > 0) CreatePortIndicator(outputEdges[0], facing, facing, new Color(0.3f, 1f, 0.3f), "Output");
            if (inputEdges.Count > 0) CreatePortIndicator(inputEdges[0], -facing, facing, new Color(0.3f, 0.8f, 1f), "Input");
        }

        // Chests only accept items entering through their single input side, opposite the output.
        public override bool CanAcceptInputFrom(Vector2Int incomingDirection)
        {
            return incomingDirection == GetFacingDirection();
        }

        public int GetStoredCount(ResourceType type)
        {
            return storedResources.TryGetValue(type, out int count) ? count : 0;
        }

        public int GetTotalStored()
        {
            int total = 0;
            foreach (var count in storedResources.Values) total += count;
            return total;
        }

        public IReadOnlyDictionary<ResourceType, int> GetStoredResources()
        {
            return storedResources;
        }

        /// <summary>Adds up to `count` of `type`, capped by remaining capacity. Returns how many were actually deposited.</summary>
        public int Deposit(ResourceType type, int count)
        {
            int spaceLeft = capacity - GetTotalStored();
            int actual = Mathf.Min(count, spaceLeft);
            if (actual <= 0) return 0;

            storedResources[type] = GetStoredCount(type) + actual;
            return actual;
        }

        /// <summary>Removes up to `count` of `type`. Returns how many were actually withdrawn.</summary>
        public int Withdraw(ResourceType type, int count)
        {
            int available = GetStoredCount(type);
            int actual = Mathf.Min(count, available);
            if (actual <= 0) return 0;

            int remaining = available - actual;
            if (remaining <= 0) storedResources.Remove(type);
            else storedResources[type] = remaining;
            return actual;
        }

        public override void PerformAction()
        {
            TryReleaseItem();
            TryAbsorbItem();
        }

        private void TryReleaseItem()
        {
            if (GetTotalStored() == 0) return;

            Vector2Int facing = GetFacingDirection();
            Vector2Int targetCell = GetNeighborCellInDirection(facing);

            List<ConveyorItem> itemsInTarget = ItemTracker.Instance != null ? ItemTracker.Instance.GetItemsInCell(targetCell) : null;
            if (itemsInTarget != null && itemsInTarget.Count > 0) return;

            ResourceType typeToRelease = default;
            bool found = false;
            foreach (var pair in storedResources)
            {
                if (pair.Value > 0) { typeToRelease = pair.Key; found = true; break; }
            }
            if (!found) return;

            GameObject prefab = BuildingUiManager.HasInstance ? BuildingUiManager.Instance.GetResourceItemPrefab(typeToRelease) : null;
            if (prefab == null) return;

            Withdraw(typeToRelease, 1);

            Vector2 spawnPos = GetEdgeSpawnPosition(targetCell, facing);
            GameObject newItem = ObjectPoolManager.Instance.GetPooledObject(prefab, spawnPos, Quaternion.identity);
            ConveyorItem itemComp = newItem.GetComponent<ConveyorItem>();
            if (itemComp != null)
            {
                itemComp.Initialize(targetCell);
            }
        }

        private void TryAbsorbItem()
        {
            if (GetTotalStored() >= capacity) return;

            foreach (var inputCell in GetInputCells())
            {
                List<ConveyorItem> items = ItemTracker.Instance != null ? ItemTracker.Instance.GetItemsInCell(inputCell) : null;
                if (items == null) continue;

                for (int i = items.Count - 1; i >= 0; i--)
                {
                    if (GetTotalStored() >= capacity) return;

                    ConveyorItem item = items[i];
                    if (item != null && !item.IsMoving)
                    {
                        Deposit(item.resourceType, 1);
                        ObjectPoolManager.Instance.ReturnToPool(item.gameObject);
                    }
                }
            }
        }
    }

}
