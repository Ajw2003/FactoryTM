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

        private readonly Queue<ConveyorItem> storedItems = new Queue<ConveyorItem>();

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

        public override void PerformAction()
        {
            TryReleaseItem();
            TryAbsorbItem();
        }

        private void TryReleaseItem()
        {
            if (storedItems.Count == 0) return;

            Vector2Int facing = GetFacingDirection();
            Vector2Int targetCell = GetNeighborCellInDirection(facing);

            List<ConveyorItem> itemsInTarget = ItemTracker.Instance != null ? ItemTracker.Instance.GetItemsInCell(targetCell) : null;
            if (itemsInTarget != null && itemsInTarget.Count > 0) return;

            ConveyorItem item = storedItems.Dequeue();
            if (item == null) return;

            Vector2 spawnPos = GetEdgeSpawnPosition(targetCell, facing);
            item.transform.position = spawnPos;
            item.gameObject.SetActive(true);
            item.Initialize(targetCell);
        }

        private void TryAbsorbItem()
        {
            if (storedItems.Count >= capacity) return;

            foreach (var inputCell in GetEdgeCells(-GetFacingDirection()))
            {
                List<ConveyorItem> items = ItemTracker.Instance != null ? ItemTracker.Instance.GetItemsInCell(inputCell) : null;
                if (items == null) continue;

                for (int i = items.Count - 1; i >= 0; i--)
                {
                    if (storedItems.Count >= capacity) return;

                    ConveyorItem item = items[i];
                    if (item != null && !item.IsMoving)
                    {
                        // Deactivating unregisters it from ItemTracker (see ConveyorItem.OnDisable) without
                        // returning it to the shared object pool, so it stays reserved for this chest.
                        item.gameObject.SetActive(false);
                        storedItems.Enqueue(item);
                    }
                }
            }
        }
    }

}
