using Buildings;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MinerLogic : MonoBehaviour
{
    private BuildingData data;
    private Vector3Int myCell;
    private Vector3Int exportDirection;
    private float timer;

    public void Setup(BuildingData minerData, Vector3Int cell, int rotationIndex)
    {
        data = minerData;
        myCell = cell;
        timer = data.spawnInterval;

        // Use the same rotation logic as the conveyors!
        exportDirection = GameManager.Instance.GetDirectionFromRotationIndex(rotationIndex);
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            SpawnItem();
            timer = data.spawnInterval;
        }
    }

    void SpawnItem()
    {
        // 1. Calculate the neighbor cell in front of the miner
        Vector3Int targetCell = myCell + exportDirection;
        Vector3 spawnPos = GameManager.Instance.buildingTilemap.GetCellCenterWorld(targetCell);

        // 2. Instantiate the item
        GameObject newItem = Instantiate(data.itemPrefab, spawnPos, Quaternion.identity);
    }
}