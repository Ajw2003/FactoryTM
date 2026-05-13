using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Furnace : BuildingLogic
{
    private Vector3Int exportDirection;
    private float timer;
    private GameObject currentItemPrefab;
    private GameObject platePrefab;
    private float currentCookingSpeed;
    
    public void Setup(Buildings.BuildingData furnace, Vector3Int cell, int rotationIndex, float speed)
    {
        base.Setup(furnace, cell);
        
        exportDirection = GameManager.Instance.GetDirectionFromRotationIndex(rotationIndex);
        currentCookingSpeed = speed;
        Debug.Log($"Furnace Setup at {myCell}: Initial exportDirection set to {exportDirection}");
    }

    public override void PerformAction()
    {
        
        List<ConveyorItem> items = ItemTracker.Instance.GetItemsInCell(myCell);
        if (items != null)
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                StartCoroutine(ProccessItem(items[i].gameObject));
                
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    timer = currentCookingSpeed; // Reset timer with the node's speed
                }
            }
        }
    }
    
    public IEnumerator ProccessItem(GameObject item)
    {
        yield return new WaitForSeconds(currentCookingSpeed);
        if (item == null)
        {
            yield break;
        }
        if (item != currentItemPrefab)
        {
            string originalItemName = item.name;
            string prefix = GameManager.GetPrefix(originalItemName);

            Debug.Log($"Furnace: Processing item '{originalItemName}'. Extracted prefix: '{prefix}'");

            platePrefab = GameManager.FindPlatePrefabWithPrefix(prefix);
            if (platePrefab is null) yield break;
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

    void CookItem(GameObject CookedItemToRecive)
    {
        if (CookedItemToRecive == null)
        {
            Debug.LogError($"Furnace at {myCell} has no item to cook!"); // Changed from Miner to Furnace
            return;
        }
        currentItemPrefab = CookedItemToRecive;

        // Log values before calculation
        Debug.Log($"Furnace CookItem at {myCell}: Current exportDirection: {exportDirection}");

        // 1. Calculate the neighbor cell in front of the furnace
        Vector3Int targetCell = myCell + exportDirection;
        Debug.Log($"Furnace CookItem at {myCell}: Calculated targetCell: {targetCell}");

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
