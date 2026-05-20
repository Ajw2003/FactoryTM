using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Furnace : BuildingLogic
{
    private Vector2Int exportDirection;
    private float timer;
    private ConveyorItem currentItemPrefab;
    private ConveyorItem platePrefab;
    private float currentCookingSpeed;
    
    public void Setup(Buildings.BuildingData furnace, Vector2Int cell, int rotationIndex, float speed)
    {
        base.Setup(furnace, cell);
        exportDirection = GameManager.Instance.GetDirectionFromRotationIndex(rotationIndex);
        currentCookingSpeed = speed;
        Debug.Log($"Furnace Setup at {myCell}: Initial exportDirection set to {exportDirection}");
    }

    private ResourceType lastProcessedResourceType = (ResourceType)(-1);
    private HashSet<ConveyorItem> itemsInProcess = new HashSet<ConveyorItem>();

    public override void PerformAction()
    {
        List<ConveyorItem> items = ItemTracker.Instance.GetItemsInCell(myCell);
        if (items != null)
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                ConveyorItem item = items[i];
                if (!itemsInProcess.Contains(item))
                {
                    StartCoroutine(ProccessItem(item));
                }
            }
        }
    }
    
    public IEnumerator ProccessItem(ConveyorItem item)
    {
        itemsInProcess.Add(item);
        yield return new WaitForSeconds(currentCookingSpeed);
        
        if (item == null)
        {
            itemsInProcess.Remove(null);
            yield break;
        }

        // Only find the plate prefab if it's null or the resource type changed
        if (platePrefab == null || item.resourceType != lastProcessedResourceType)
        {
            string prefix = GameManager.GetPrefix(item.name);

            Debug.Log($"Furnace: Finding new plate prefab for '{item.name}' (prefix: '{prefix}')");

            ConveyorItem prefabGo = GameManager.FindPlatePrefabWithPrefix(prefix);
            if (prefabGo != null)
            {
                platePrefab = prefabGo.GetComponent<ConveyorItem>();
                lastProcessedResourceType = item.resourceType;
            }
            else
            {
                Debug.LogWarning($"Furnace: Could not find plate prefab for prefix '{prefix}'");
                itemsInProcess.Remove(item);
                yield break;
            }
        }

        if (platePrefab != null)
        {
            CookItem(platePrefab);
            Destroy(item.gameObject);
        }
        
        itemsInProcess.Remove(item);
    }

    void CookItem(ConveyorItem CookedItemToRecive)
    {
        if (CookedItemToRecive == null)
        {
            Debug.LogError($"Furnace at {myCell} has no item to cook!"); // Changed from Miner to Furnace
            return;
        }
        currentItemPrefab = CookedItemToRecive;

        // Log values before calculation

        // 1. Calculate the neighbor cell in front of the furnace
        Vector2Int targetCell = myCell + exportDirection;

        Vector2 spawnPos = GridManager.Instance.CellToWorldConversion(targetCell);
        

        // 2. Instantiate the item
        GameObject newItem = Instantiate(currentItemPrefab.gameObject, spawnPos, Quaternion.identity);
        ConveyorItem itemComp = newItem.GetComponent<ConveyorItem>();
        if (itemComp != null)
        {
            itemComp.Initialize(targetCell);
        }
    }
}
