using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton for easy access

    [Header("Data Config")]
    public BuildingData[] allBuildings;

    // The "Brain": Maps a specific Tile asset to a Direction
    private Dictionary<TileBase, Vector3Int> tileDirectionMap = new Dictionary<TileBase, Vector3Int>();

    void Awake()
    {
        Instance = this;
        InitializeData();
    }

    void InitializeData()
    {
        foreach (var building in allBuildings)
        {
            if (building.isConveyor)
            {
                // Map the 4 rotated tiles to their corresponding directions
                tileDirectionMap.Add(building.rotatedTiles[0], new Vector3Int(1, 0, 0));  // Right
                tileDirectionMap.Add(building.rotatedTiles[1], new Vector3Int(0, -1, 0)); // Down
                tileDirectionMap.Add(building.rotatedTiles[2], new Vector3Int(-1, 0, 0)); // Left
                tileDirectionMap.Add(building.rotatedTiles[3], new Vector3Int(0, 1, 0));  // Up
            }
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