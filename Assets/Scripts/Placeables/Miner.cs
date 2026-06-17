using System.Collections.Generic;
using Buildings;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MinerLogic : BuildingLogic
{
    private Vector2Int exportDirection;
    private int rotationIndex;
    private float timer;
    private ResourceNode assignedResourceNode;
    private GameObject currentMinedItemPrefab;
    private float currentMiningSpeed;
    private bool finiteOres;

    public void Setup(Buildings.BuildingData minerData, Vector2Int cell, int rotationIndex)
    {
        base.Setup(minerData, cell);
        this.rotationIndex = rotationIndex;
        finiteOres = GameManager.Instance.finiteOres;
        
        // Calculate occupied cells if not already set (though PlacementManager sets them)
        // For safety, let's calculate them here too or assume they will be set.
        // Actually, Setup is called before SetOccupiedCells in PlacementManager.
        // Let's calculate them here so we can find the node immediately.
        Vector2Int actualSize = data.size;
        if (rotationIndex % 2 != 0) actualSize = new Vector2Int(data.size.y, data.size.x);
        
        List<Vector2Int> cells = new List<Vector2Int>();
        for (int x = 0; x < actualSize.x; x++)
        {
            for (int y = 0; y < actualSize.y; y++)
            {
                cells.Add(myCell + new Vector2Int(x, y));
            }
        }
        occupiedCells = cells;

        // Attempt to find a ResourceNode at any of the miner's positions
        if (ResourceManager.Instance != null)
        {
            foreach (var occupiedCell in occupiedCells)
            {
                assignedResourceNode = ResourceManager.Instance.GetNodeAtPosition(occupiedCell);
                if (assignedResourceNode != null) break;
            }
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
        if (currentMinedItemPrefab == null || finiteOres && assignedResourceNode.oreCount <= 0)
        {
            Debug.LogError($"Miner at {myCell} has no item prefab to mine!");
            return;
        }

        // Calculate the target cell based on size and direction
        Vector2Int actualSize = data.size;
        if (rotationIndex % 2 != 0) actualSize = new Vector2Int(data.size.y, data.size.x);

        Vector2Int offset = Vector2Int.zero;
        if (exportDirection.x > 0) offset = new Vector2Int(actualSize.x, 0); // Right
        else if (exportDirection.x < 0) offset = new Vector2Int(-1, 0);      // Left
        else if (exportDirection.y > 0) offset = new Vector2Int(0, actualSize.y); // Up
        else if (exportDirection.y < 0) offset = new Vector2Int(0, -1);      // Down

        Vector2Int targetCell = myCell + offset;
        Vector2 spawnPos = GridManager.Instance.CellToWorldConversion(targetCell);

        // Align spawnPos to the exact exit boundary of the chute on the tile transition (aligned with bottom/left cell for 1x1 conveyors)
        Vector2 tileSize = GridManager.Instance.tileSize;
        if (exportDirection.x > 0) // Right boundary
        {
            spawnPos.x = targetCell.x * tileSize.x;
            spawnPos.y = (myCell.y + 0.5f) * tileSize.y;
        }
        else if (exportDirection.x < 0) // Left boundary
        {
            spawnPos.x = (targetCell.x + 1) * tileSize.x;
            spawnPos.y = (myCell.y + 0.5f) * tileSize.y;
        }
        else if (exportDirection.y > 0) // Top boundary
        {
            spawnPos.x = (myCell.x + 0.5f) * tileSize.x;
            spawnPos.y = targetCell.y * tileSize.y;
        }
        else if (exportDirection.y < 0) // Bottom boundary
        {
            spawnPos.x = (myCell.x + 0.5f) * tileSize.x;
            spawnPos.y = (targetCell.y + 1) * tileSize.y;
        }

        // 2. Instantiate the item
        GameObject newItem = Instantiate(currentMinedItemPrefab, spawnPos, Quaternion.identity);
        ConveyorItem itemComp = newItem.GetComponent<ConveyorItem>();
        if (itemComp != null)
        {
            itemComp.Initialize(targetCell);
        }

        if (finiteOres)
        {
            assignedResourceNode.oreCount--;
        }
    }
}
