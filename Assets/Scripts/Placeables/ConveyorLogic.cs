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
    
    public class ConveyorLogic : BuildingLogic
    {
        private Vector2Int direction;
        private float moveSpeed = 2f;
    
        public void Setup(Buildings.BuildingData buildingData, Vector2Int cell, Vector2Int dir, float speed)
        {
            base.Setup(buildingData, cell);
            direction = dir;
            // Sync item movement speed to the 8-frame tile animation timing (10 fps / 8 frames = 1.25 units per second)
            moveSpeed = 1.25f;
        }
    
        public override void PerformAction()
        {
            List<ConveyorItem> items = ItemTracker.Instance.GetItemsInCell(myCell);
            if (items != null)
            {
                for (int i = items.Count - 1; i >= 0; i--)
                {
                    ConveyorItem item = items[i];
                    if (!item.IsMoving)
                    {
                        Vector2Int targetCell = myCell + direction;
                        bool isValidReceiver = false;

                        if (Managers.PlacementManager.HasInstance)
                        {
                            var activeBuildings = Managers.PlacementManager.Instance.GetActiveBuildings();
                            if (activeBuildings.TryGetValue(targetCell, out GameObject targetObj))
                            {
                                BuildingLogic targetLogic = targetObj.GetComponent<BuildingLogic>();
                                if (targetLogic != null && targetLogic.data != null)
                                {
                                    Buildings.BuildingType type = targetLogic.data.type;
                                    if (type == Buildings.BuildingType.Conveyor || 
                                        type == Buildings.BuildingType.Furnace || 
                                        type == Buildings.BuildingType.Seller)
                                    {
                                        isValidReceiver = true;
                                    }
                                }
                            }
                        }

                        if (isValidReceiver)
                        {
                            item.SetTarget(targetCell, moveSpeed);
                        }
                    }
                }
            }
        }
    }
    
}


