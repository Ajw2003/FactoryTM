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
    using Items;
    using UnityEngine;

    public class Chest : BuildingLogic
    {
        private const int SlotCount = 6;

        /// <summary>Multi-slot storage - reparented into BuildingUiManager's chest grid while open,
        /// and drained/filled directly by belts on the input/output side.</summary>
        public InventorySlot[] Slots { get; private set; }

        public override void Setup(Buildings.BuildingData buildingData, Vector2Int cell)
        {
            Setup(buildingData, cell, 0);
        }

        public void Setup(Buildings.BuildingData buildingData, Vector2Int cell, int rotationIndex)
        {
            base.Setup(buildingData, cell);
            this.rotationIndex = rotationIndex;

            occupiedCells = ComputeFootprintCells();

            Vector2Int facing = GetFacingDirection();
            var outputEdges = GetEdgeCells(facing);
            var inputEdges = GetEdgeCells(-facing);
            if (outputEdges.Count > 0) CreatePortIndicator(outputEdges[0], facing, facing, new Color(0.3f, 1f, 0.3f), "Output");
            if (inputEdges.Count > 0) CreatePortIndicator(inputEdges[0], -facing, facing, new Color(0.3f, 0.8f, 1f), "Input");

            Slots = new InventorySlot[SlotCount];
            for (int i = 0; i < SlotCount; i++)
            {
                Slots[i] = BuildingSlotFactory.CreateSlot(transform);
            }
        }

        // Chests only accept items entering through their single input side, opposite the output.
        public override bool CanAcceptInputFrom(Vector2Int incomingDirection)
        {
            return incomingDirection == GetFacingDirection();
        }

        public override void PerformAction()
        {
            AbsorbInputItems();
            ReleaseStoredItem();
        }

        private void AbsorbInputItems()
        {
            foreach (var inputCell in GetInputCells())
            {
                List<ConveyorItem> items = ItemTracker.Instance.GetItemsInCell(inputCell);
                if (items == null) continue;

                for (int i = items.Count - 1; i >= 0; i--)
                {
                    ConveyorItem item = items[i];
                    if (item == null || item.IsMoving) continue;

                    foreach (var slot in Slots)
                    {
                        if (slot.TryAdd(item._itemData, 1))
                        {
                            ObjectPoolManager.Instance.ReturnToPool(item.gameObject);
                            break;
                        }
                    }
                }
            }
        }

        private void ReleaseStoredItem()
        {
            Vector2Int facing = GetFacingDirection();
            Vector2 spawnPos = GetOutputSpawnPosition(facing, out Vector2Int targetCell);

            if (!CanReleaseInto(targetCell, facing)) return;

            foreach (var slot in Slots)
            {
                if (!slot.slotFilled) continue;

                GameObject prefab = BuildingUiManager.HasInstance
                    ? BuildingUiManager.Instance.GetResourceItemPrefab(slot._itemData.specificItemType)
                    : null;
                if (prefab == null) continue;

                slot.TryRemove(1);

                GameObject newItem = ObjectPoolManager.Instance.GetPooledObject(prefab, spawnPos, Quaternion.identity);
                ConveyorItem itemComp = newItem.GetComponent<ConveyorItem>();
                if (itemComp != null)
                {
                    itemComp.Initialize(targetCell);
                }
                break;
            }
        }

        /// <summary>Only release onto a cell that's both unoccupied and actually leads somewhere - a
        /// conveyor or another building willing to accept input from this direction. Otherwise the
        /// chest holds onto what it has rather than littering an item nobody will ever pick up.</summary>
        private bool CanReleaseInto(Vector2Int targetCell, Vector2Int facing)
        {
            List<ConveyorItem> itemsThere = ItemTracker.Instance.GetItemsInCell(targetCell);
            if (itemsThere != null && itemsThere.Count > 0) return false;

            if (!PlacementManager.HasInstance) return false;
            if (!PlacementManager.Instance.GetActiveBuildings().TryGetValue(targetCell, out GameObject targetObj) || targetObj == null) return false;

            BuildingLogic targetLogic = targetObj.GetComponent<BuildingLogic>();
            return targetLogic != null && targetLogic.CanAcceptInputFrom(facing);
        }
    }
}
