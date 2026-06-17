using System.Collections.Generic;
using UnityEngine;

public class Seller : BuildingLogic
{
    public override void Setup(Buildings.BuildingData buildingData, Vector2Int cell)
    {
        base.Setup(buildingData, cell);
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
                    GameManager.Instance.ProccessSale(item.gameObject);
                }
            }
        }
    }
}
