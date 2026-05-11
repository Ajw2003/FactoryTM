using System.Collections.Generic;
using Singleton;
using UnityEngine;

public class ItemTracker : SingletonBase<ItemTracker>
{
    private Dictionary<Vector3Int, List<ConveyorItem>> itemsByCell = new Dictionary<Vector3Int, List<ConveyorItem>>();

    public void RegisterItem(ConveyorItem item, Vector3Int cell)
    {
        if (!itemsByCell.ContainsKey(cell))
        {
            itemsByCell[cell] = new List<ConveyorItem>();
        }
        
        if (!itemsByCell[cell].Contains(item))
        {
            itemsByCell[cell].Add(item);
        }
    }

    public void UnregisterItem(ConveyorItem item, Vector3Int cell)
    {
        if (itemsByCell.ContainsKey(cell))
        {
            itemsByCell[cell].Remove(item);
            if (itemsByCell[cell].Count == 0)
            {
                itemsByCell.Remove(cell);
            }
        }
    }

    public void UpdateItemCell(ConveyorItem item, Vector3Int oldCell, Vector3Int newCell)
    {
        if (oldCell == newCell) return;
        UnregisterItem(item, oldCell);
        RegisterItem(item, newCell);
    }

    public List<ConveyorItem> GetItemsInCell(Vector3Int cell)
    {
        if (itemsByCell.TryGetValue(cell, out List<ConveyorItem> items))
        {
            return items;
        }
        return null;
    }
}
