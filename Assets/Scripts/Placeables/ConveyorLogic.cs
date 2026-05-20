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
