using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Buildings;
using Singleton;
using Unity.VisualScripting;

public class GameManager : SingletonBase<GameManager>
{

    [Header("Data Config")]
    public BuildingData[] allBuildings;
    public Tilemap buildingTilemap;

    // The "Brain": Maps a specific Tile asset to a Direction
    private Dictionary<TileBase, Vector3Int> tileDirectionMap = new Dictionary<TileBase, Vector3Int>();

    protected override void Awake()
    {
        base.Awake();
        InitializeData();
    }

    void InitializeData()
    {
        foreach (var building in allBuildings)
        {
            if (building.type == BuildingType.Conveyor || building.type == BuildingType.Miner)
            {
                // Map the 4 rotated tiles to their corresponding directions
                tileDirectionMap.Add(building.rotatedTiles[0], new Vector3Int(1, 0, 0));  // Right
                tileDirectionMap.Add(building.rotatedTiles[1], new Vector3Int(0, -1, 0)); // Down
                tileDirectionMap.Add(building.rotatedTiles[2], new Vector3Int(-1, 0, 0)); // Left
                tileDirectionMap.Add(building.rotatedTiles[3], new Vector3Int(0, 1, 0));  // Up
            }
        }
    }
    
    public Vector3Int GetDirectionFromRotationIndex(int index)
    {
        switch (index)
        {
            case 0: return new Vector3Int(1, 0, 0);  // Right
            case 1: return new Vector3Int(0, -1, 0); // Down
            case 2: return new Vector3Int(-1, 0, 0); // Left
            case 3: return new Vector3Int(0, 1, 0);  // Up
            default: return Vector3Int.right;
        }
    }

    public Vector3Int GetDirectionFromTile(TileBase tile)
    {
        if (tile != null && tileDirectionMap.ContainsKey(tile))
        {
            return tileDirectionMap[tile];
        }
        return Vector3Int.zero;
    }
    
}