using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
using System.Collections.Generic;
using Buildings;
using Singleton;
using UnityEngine;

public class AssetScanner : SingletonBase<AssetScanner>
{
    // This method will now load all ConveyorItem assets from a specified path within Resources folders.
    // The path should be relative to any "Resources" folder (e.g., "prefabs/Items").
    public List<ConveyorItem> GetAllConveyorItemsInResources(string path)
    {
        List<ConveyorItem> allConveyorItems = new List<ConveyorItem>();

        // Loads all assets of type ConveyorItem from the specified path within any "Resources" folder
        ConveyorItem[] loadedItems = Resources.LoadAll<ConveyorItem>(path);

        foreach (ConveyorItem item in loadedItems)
        {
            if (item != null)
            {
                allConveyorItems.Add(item);
            }
        }

        Debug.Log($"AssetScanner: Found {allConveyorItems.Count} ConveyorItems in Resources path: '{path}'");
        return allConveyorItems;
    }
}

