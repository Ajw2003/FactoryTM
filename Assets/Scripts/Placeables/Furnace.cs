using System.Collections.Generic;
using UnityEngine;

public class Furnace : BuildingLogic
{
    private Vector3Int exportDirection;
    private float timer;
    private GameObject currentItemPrefab;
    private GameObject platePrefab;
    private float currentCookingSpeed;
    
    public void Setup(Buildings.BuildingData furnace, Vector3Int cell, int rotationIndex)
    {
        base.Setup(furnace, cell);
        
        exportDirection = GameManager.Instance.GetDirectionFromRotationIndex(rotationIndex);
    }

    public override void PerformAction()
    {
        
        List<ConveyorItem> items = ItemTracker.Instance.GetItemsInCell(myCell);
        if (items != null)
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                ProccessItem(items[i].gameObject);
                
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
            
                    timer = currentCookingSpeed; // Reset timer with the node's speed
                }
            }
        }
    }
    
    public void ProccessItem(GameObject item)
    {
        if(item == null) return;
        item.TryGetComponent(out ConveyorItem citem);
        if (item != currentItemPrefab)
        {
            // Debug.Log("ahhh"); // Removed redundant log
        
            string originalItemName = item.name;
            // Use GameManager.GetPrefix to handle "(Clone)" and other suffixes
            string prefix = GameManager.GetPrefix(originalItemName);

            Debug.Log($"Furnace: Processing item '{originalItemName}'. Extracted prefix: '{prefix}'");

            // 2. Use the prefix scan function to find the corresponding Plate prefab
            platePrefab = GameManager.FindPlatePrefabWithPrefix(prefix);
            Debug.Log($"Furnace: Found plate prefab for prefix '{prefix}': {(platePrefab != null ? platePrefab.name : "NULL")}"); 
            CookItem(platePrefab);
            Destroy(item.gameObject);
        }
        else
        {
            CookItem(platePrefab);
            Destroy(item.gameObject); 
        }
        
       
        
        
    }

    void CookItem(GameObject itemToCook)
    {
        if(itemToCook == null) return;
        currentItemPrefab = itemToCook;
        //1. calculate direction
        Vector3Int targetCell = myCell + exportDirection;
        Vector3 spawnPos = GameManager.Instance.buildingTilemap.GetCellCenterWorld(targetCell);

        // 2. Instantiate the item
        GameObject newItem = Instantiate(currentItemPrefab, spawnPos, Quaternion.identity);
        ConveyorItem itemComp = newItem.GetComponent<ConveyorItem>();
        if (itemComp != null)
        {
            itemComp.Initialize(targetCell);
        }
    }
}
