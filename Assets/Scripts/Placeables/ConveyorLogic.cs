using System.Collections.Generic;
using UnityEngine;

public class ConveyorLogic : BuildingLogic
{
    private Vector3Int direction;
    private float moveSpeed = 2f;

    public void Setup(Buildings.BuildingData buildingData, Vector3Int cell, Vector3Int dir, float speed)
    {
        base.Setup(buildingData, cell);
        direction = dir;
        moveSpeed = speed;
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
                    item.SetTarget(myCell + direction, moveSpeed);
                }
            }
        }
    }
}
