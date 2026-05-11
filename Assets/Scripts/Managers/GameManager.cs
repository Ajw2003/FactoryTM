using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Buildings;
using Singleton;
using Unity.VisualScripting; // This might not be needed anymore, check later

public class GameManager : SingletonBase<GameManager>
{

    [Header("Data Config")]
    public BuildingData[] allBuildings;
    public Tilemap buildingTilemap;
    public TileBase sellerTile;

    [Header("Resource Node Spawning")]
    public ResourceNodeDefinition[] resourceNodeDefinitions;
    public int minPatchSize = 3;
    public int maxPatchSize = 7;
    [Range(0f, 1f)]
    public float patchSpawnChance = 0.05f; // Chance for a patch to start at a given cell

    // The "Brain": Maps a specific Tile asset to a Direction
    private Dictionary<TileBase, Vector3Int> tileDirectionMap = new Dictionary<TileBase, Vector3Int>();

    protected override void Awake()
    {
        base.Awake();
        InitializeData();
    }

    void Start()
    {
        SpawnResourceNodes();
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

    public bool IsSellerTile(TileBase tile)
    {
        return tile == sellerTile;
    }

    public void ProccessSale(GameObject item)
    {
        item.TryGetComponent(out ConveyorItem citem);
        Destroy(item.gameObject);
        CurrencyManager.Instance.AddCurrency(citem.value);
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

    private void SpawnResourceNodes()
    {
        if (resourceNodeDefinitions == null || resourceNodeDefinitions.Length == 0)
        {
            Debug.LogWarning("No ResourceNodeDefinitions assigned in GameManager. Cannot spawn nodes.");
            return;
        }

        // Calculate total spawn weight
        int totalWeight = 0;
        foreach (var def in resourceNodeDefinitions)
        {
            totalWeight += def.spawnWeight;
        }

        // Iterate through the tilemap bounds
        BoundsInt bounds = buildingTilemap.cellBounds;
        HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>(); // Keep track of cells where nodes are spawned

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);

                // Skip if already occupied or if random chance fails
                if (occupiedCells.Contains(cell) || Random.value > patchSpawnChance)
                {
                    continue;
                }

                // Randomly select a resource node definition based on weights
                ResourceNodeDefinition selectedDefinition = GetRandomResourceNodeDefinition(totalWeight);
                if (selectedDefinition == null || selectedDefinition.resourceNodePrefab == null)
                {
                    Debug.LogWarning("Selected ResourceNodeDefinition or its prefab is null. Skipping.");
                    continue;
                }

                // Determine patch size
                int patchSize = Random.Range(minPatchSize, maxPatchSize + 1);
                
                // Attempt to spawn a patch
                SpawnPatch(cell, patchSize, selectedDefinition, occupiedCells);
            }
        }
    }

    private ResourceNodeDefinition GetRandomResourceNodeDefinition(int totalWeight)
    {
        int randomWeight = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var def in resourceNodeDefinitions)
        {
            currentWeight += def.spawnWeight;
            if (randomWeight < currentWeight)
            {
                return def;
            }
        }
        return null; // Should not happen if totalWeight is calculated correctly and definitions exist
    }

    private void SpawnPatch(Vector3Int startCell, int patchSize, ResourceNodeDefinition definition, HashSet<Vector3Int> occupiedCells)
    {
        Queue<Vector3Int> cellsToProcess = new Queue<Vector3Int>();
        cellsToProcess.Enqueue(startCell);
        occupiedCells.Add(startCell); // Mark the starting cell as occupied

        int spawnedCount = 0;

        while (cellsToProcess.Count > 0 && spawnedCount < patchSize)
        {
            Vector3Int currentCell = cellsToProcess.Dequeue();

            // Spawn the node at currentCell
            SpawnSingleResourceNode(currentCell, definition);
            spawnedCount++;

            // Add neighbors to the queue if they are within bounds and not occupied
            Vector3Int[] neighbors = new Vector3Int[]
            {
                currentCell + Vector3Int.right,
                currentCell + Vector3Int.left,
                currentCell + Vector3Int.up,
                currentCell + Vector3Int.down
            };

            foreach (Vector3Int neighbor in neighbors)
            {
                if (buildingTilemap.HasTile(neighbor) || occupiedCells.Contains(neighbor)) // Check if tilemap has a tile (meaning it's a valid place) and not already occupied
                {
                    continue;
                }
                
                // Check if there's already a building at this position
                if (ResourceManager.Instance.GetNodeAtPosition(neighbor) != null)
                {
                    occupiedCells.Add(neighbor); // Mark as occupied even if it's another node
                    continue;
                }

                occupiedCells.Add(neighbor);
                cellsToProcess.Enqueue(neighbor);
            }
        }
    }

    private void SpawnSingleResourceNode(Vector3Int cell, ResourceNodeDefinition definition)
    {
        // Get world position for instantiation
        Vector3 worldPos = buildingTilemap.GetCellCenterWorld(cell);

        // Instantiate the resource node prefab
        GameObject nodeGO = Instantiate(definition.resourceNodePrefab, worldPos, Quaternion.identity);
        nodeGO.transform.parent = this.transform; // Optional: parent to GameManager for organization

        // Get the ResourceNode component and set it up
        ResourceNode resourceNode = nodeGO.GetComponent<ResourceNode>();
        if (resourceNode != null)
        {
            resourceNode.Setup(cell);
            // Assign properties from definition
            resourceNode.minedItemPrefab = definition.minedItemPrefab;
            resourceNode.miningSpeed = definition.miningSpeed;
        }
        else
        {
            Debug.LogError($"ResourceNodePrefab '{definition.resourceNodePrefab.name}' is missing ResourceNode component!");
            Destroy(nodeGO); // Clean up if component is missing
        }
    }
}
