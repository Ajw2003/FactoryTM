using System.Collections.Generic;
using UnityEngine;

public class Seller : BuildingLogic
{
    public override void Setup(Buildings.BuildingData buildingData, Vector3Int cell)
    {
        base.Setup(buildingData, cell);
    }

    public override void PerformAction()
    {
        if (!IsBeingWorked) return;

        List<ConveyorItem> items = ItemTracker.Instance.GetItemsInCell(myCell);
        if (items != null)
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                GameManager.Instance.ProccessSale(items[i].gameObject);
            }
        }
    }
}
