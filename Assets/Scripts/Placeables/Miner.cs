using Buildings;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MinerLogic : BuildingLogic
{
    private Vector3Int exportDirection;
    private float timer;

    public void Setup(Buildings.BuildingData minerData, Vector3Int cell, int rotationIndex)
    {
        base.Setup(minerData, cell);
        timer = data.proccessingSpeed;

        // Use the same rotation logic as the conveyors!
        exportDirection = GameManager.Instance.GetDirectionFromRotationIndex(rotationIndex);
    }

    public override void PerformAction()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            SpawnItem();
            timer = data.proccessingSpeed;
        }
    }

    void SpawnItem()
    {
        // 1. Calculate the neighbor cell in front of the miner
        Vector3Int targetCell = myCell + exportDirection;
        Vector3 spawnPos = GameManager.Instance.buildingTilemap.GetCellCenterWorld(targetCell);

        // 2. Instantiate the item
        GameObject newItem = Instantiate(data.itemPrefab, spawnPos, Quaternion.identity);
        ConveyorItem itemComp = newItem.GetComponent<ConveyorItem>();
        if (itemComp != null)
        {
            itemComp.Initialize(targetCell);
        }
    }
}
