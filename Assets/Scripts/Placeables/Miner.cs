using Buildings;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MinerLogic : BuildingLogic
{
    private Vector3Int exportDirection;
    private float timer;
    private ResourceNode assignedResourceNode;
    private GameObject currentMinedItemPrefab;
    private float currentMiningSpeed;

    public void Setup(Buildings.BuildingData minerData, Vector3Int cell, int rotationIndex)
    {
        base.Setup(minerData, cell);
        
        // Attempt to find a ResourceNode at the miner's position
        if (ResourceManager.Instance != null)
        {
            assignedResourceNode = ResourceManager.Instance.GetNodeAtPosition(myCell);
        }

        if (assignedResourceNode != null)
        {
            currentMinedItemPrefab = assignedResourceNode.minedItemPrefab;
            currentMiningSpeed = assignedResourceNode.miningSpeed;
            timer = currentMiningSpeed; // Initialize timer with the node's speed
        }
        else
        {
            Debug.LogWarning($"Miner at {myCell} has no ResourceNode assigned. Disabling miner.");
            enabled = false; // Disable the miner if no node is found
            return; // Exit setup early
        }

        // Use the same rotation logic as the conveyors!
        exportDirection = GameManager.Instance.GetDirectionFromRotationIndex(rotationIndex);
    }

    public override void PerformAction()
    {
        if (!enabled) return; // Ensure miner is enabled

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            SpawnItem();
            timer = currentMiningSpeed; // Reset timer with the node's speed
        }
    }

    void SpawnItem()
    {
        if (currentMinedItemPrefab == null)
        {
            Debug.LogError($"Miner at {myCell} has no item prefab to mine!");
            return;
        }

        // 1. Calculate the neighbor cell in front of the miner
        Vector3Int targetCell = myCell + exportDirection;
        Vector3 spawnPos = GameManager.Instance.buildingTilemap.GetCellCenterWorld(targetCell);

        // 2. Instantiate the item
        GameObject newItem = Instantiate(currentMinedItemPrefab, spawnPos, Quaternion.identity);
        ConveyorItem itemComp = newItem.GetComponent<ConveyorItem>();
        if (itemComp != null)
        {
            itemComp.Initialize(targetCell);
        }
    }
}
