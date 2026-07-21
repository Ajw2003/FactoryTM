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
    using Buildings;
    
    public class ConveyorLogic : BuildingLogic
    {
        private Vector2Int direction;
        private float moveSpeed = 2f;
        private int lastEvaluatedPlacementVersion = -1;
        private BuildingLogic cachedReceiver = null;
        private bool isReceiverValid = false;
    
        public void Setup(Buildings.BuildingData buildingData, Vector2Int cell, Vector2Int dir, float speed)
        {
            base.Setup(buildingData, cell);
            direction = dir;
            // Sync item movement speed to the 8-frame tile animation timing (10 fps / 8 frames = 1.25 units per second)
            moveSpeed = 1.25f;
        }

        // Conveyors can be side-loaded from any direction; the belt itself doesn't gate intake.
        public override bool CanAcceptInputFrom(Vector2Int incomingDirection)
        {
            return true;
        }

        public override void PerformAction()
        {
            if (lastEvaluatedPlacementVersion != PlacementManager.PlacementVersion)
            {
                lastEvaluatedPlacementVersion = PlacementManager.PlacementVersion;
                cachedReceiver = null;
                isReceiverValid = false;
                Vector2Int targetCell = myCell + direction;
                if (PlacementManager.Instance != null)
                {
                    var activeBuildings = PlacementManager.Instance.GetActiveBuildings();
                    if (activeBuildings != null && activeBuildings.TryGetValue(targetCell, out GameObject targetBuildingObj))
                    {
                        if (targetBuildingObj != null)
                        {
                            BuildingLogic targetLogic = targetBuildingObj.GetComponent<BuildingLogic>();
                            if (targetLogic != null && targetLogic.CanAcceptInputFrom(direction))
                            {
                                cachedReceiver = targetLogic;
                                isReceiverValid = true;
                            }
                        }
                    }
                }
            }

            if (isReceiverValid)
            {
                Vector2Int targetCell = myCell + direction;
                List<ConveyorItem> itemsInTarget = ItemTracker.Instance != null ? ItemTracker.Instance.GetItemsInCell(targetCell) : null;
                bool isTargetOccupied = itemsInTarget != null && itemsInTarget.Count > 0;
                
                if (!isTargetOccupied)
                {
                    List<ConveyorItem> items = ItemTracker.Instance != null ? ItemTracker.Instance.GetItemsInCell(myCell) : null;
                    if (items != null)
                    {
                        for (int i = items.Count - 1; i >= 0; i--)
                        {
                            ConveyorItem item = items[i];
                            if (item != null && !item.IsMoving)
                            {
                                item.SetTarget(targetCell, moveSpeed);
                            }
                        }
                    }
                }
            }
        }
    }
    
}


