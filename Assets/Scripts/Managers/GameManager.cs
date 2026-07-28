using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
namespace Managers
{
    using System;
    using UnityEngine;
    using UnityEngine.Tilemaps;
    using System.Collections.Generic;
    using Buildings;
    using Managers;
    using Singleton;
    using UnityEngine.Rendering.Universal;
    using Random = UnityEngine.Random;
    
    // using Unity.VisualScripting; // This might not be needed anymore, check later
    
    public class GameManager : SingletonBase<GameManager>
    {
        [Header("Global Variables")]
        [SerializeField] private Camera mainCamera;
        public Camera MainCamera => mainCamera;
        
        [Header("Data Config")]
        [SerializeField] private BuildingData[] allBuildings;
        public Tilemap MainTileMap;
        public Tilemap BuildingTileMap;
        public Tilemap OreTileMap; // Ground tile painted under a spawned ResourceNode, see ResourceNode.Setup
        [SerializeField] private TileBase sellerTile;
        [SerializeField] private bool finiteOres;

        [Header("Resource Node Spawning")]
        [SerializeField] private ResourceNodeDefinition[] resourceNodeDefinitions;
        [SerializeField] private int minPatchSize = 3;
        [SerializeField] private int maxPatchSize = 7;
        [Range(0f, 1f)]
        [SerializeField] private float patchSpawnChance = 0.05f; // Chance for a patch to start at a given cell

        // This list will store the ConveyorItem components of the plate prefabs
        public List<ConveyorItem> Plates;
        [SerializeField] private PlayerController playerController;

        public BuildingData[] AllBuildings => allBuildings;
        public TileBase SellerTile => sellerTile;
        public bool FiniteOres => finiteOres;
        public ResourceNodeDefinition[] ResourceNodeDefinitions => resourceNodeDefinitions;
        public int MinPatchSize => minPatchSize;
        public int MaxPatchSize => maxPatchSize;
        public float PatchSpawnChance => patchSpawnChance;
        public PlayerController PlayerController => playerController;
    
        // The "Brain": Maps a specific Tile asset to a Direction
        private Dictionary<TileBase, Vector2Int> tileDirectionMap = new Dictionary<TileBase, Vector2Int>();
    
        public List<CartelMember> ActiveEnemies = new List<CartelMember>();
    
        protected override void Awake()
        {
            persistBetweenScenes = false;
            base.Awake();
            playerController = FindFirstObjectByType<PlayerController>();
            mainCamera = FindFirstObjectByType<Camera>();
    
            if (MainTileMap == null)
            {
                GameObject gridObj = GameObject.Find("Grid");
                if (gridObj != null)
                {
                    Transform t = gridObj.transform.Find("MainTilemap");
                    if (t != null) MainTileMap = t.GetComponent<Tilemap>();
                }
    
                if (MainTileMap == null)
                {
                    MainTileMap = GameObject.FindFirstObjectByType<Tilemap>(); // Fallback
                }
            }
    
            if (BuildingTileMap == null)
            {
                GameObject buildingmap = GameObject.Find("BuildingTilemap");
                BuildingTileMap = buildingmap.GetComponent<Tilemap>();
            }
    
        InitializeData();
    
            // Initialize Roguelike/DayNight cycle and Upgrade systems
            _ = DayNightManager.Instance;
            _ = UpgradeManager.Instance;
            _ = UpgradeUi.Instance;
            _ = Managers.GameStatsManager.Instance;
            _ = PauseManager.Instance;
            _ = BuildingUiManager.Instance;
        }
    
        void Start()
        {
            SpawnResourceNodes();
            
            gameObject.AddComponent<Managers.EnemyOutpostManager>();
            
            // --- NEW LOGIC FOR POPULATING PLATES LIST ---
            Plates = new List<ConveyorItem>();
            if (AssetScanner.Instance != null)
            {
                // Load all ConveyorItems from the "prefabs/Items" Resources folder
                List<ConveyorItem> allConveyorItems = AssetScanner.Instance.GetAllConveyorItemsInResources("prefabs/Items");
    
                // Filter for items that end with "Plate"
                foreach (ConveyorItem item in allConveyorItems)
                {
                    if (item != null && item.gameObject != null && item.gameObject.name.EndsWith("Plate"))
                    {
                        Plates.Add(item);
                    }
                }
            }
            else
            {
                Debug.LogError("AssetScanner.Instance is null. Cannot populate Plates list.");
            }
            // --- END NEW LOGIC ---
    
            // Add a debug log to show what's in the Plates list after initialization
            Debug.Log($"GameManager: Plates list initialized with {Plates.Count} items.");
            foreach (var plateItem in Plates)
            {
                if (plateItem != null && plateItem.gameObject != null)
                {
                    Debug.Log($"GameManager: Plate in list: {plateItem.gameObject.name}");
                }
                else
                {
                    Debug.LogWarning("GameManager: Null or invalid plate item found in Plates list.");
                }
            }
            SpawnStartingIDT();
        }
    
        private void SpawnStartingIDT()
        {
            if (allBuildings == null)
            {
                Debug.LogWarning("GameManager: allBuildings is null! Cannot spawn starting IDT.");
                return;
            }
    
            BuildingData idtData = null;
            foreach (var building in allBuildings)
            {
                if (building != null && building.type == BuildingType.Seller)
                {
                    idtData = building;
                    break;
                }
            }
    
            if (idtData == null)
            {
                Debug.LogWarning("GameManager: IDT/InterDimensionalTransporter building data not found in allBuildings!");
                return;
            }
    
            Vector2Int centerCell = Vector2Int.zero;
            if (ZoneManager.Instance != null)
            {
                Vector2Int zoneSize = ZoneManager.Instance.ZoneSizeInTiles;
                centerCell = new Vector2Int(zoneSize.x / 2, zoneSize.y / 2);
            }
            else if (GridManager.Instance != null)
            {
                centerCell = GridManager.Instance.Center;
            }
    
            if (PlacementManager.Instance != null)
            {
                PlacementManager.Instance.SpawnSellerProgrammatically(idtData, centerCell, 0);
                Debug.Log($"GameManager: Programmatically spawned starting IDT at {centerCell}");
            }
        }
    
        void InitializeData()
        {
            if (allBuildings == null)
            {
                Debug.LogWarning("GameManager: allBuildings is null! Make sure it is assigned in the inspector.");
                return;
            }
    
            foreach (var building in allBuildings)
            {
                if (building == null) continue;
    
                if (building.type == BuildingType.Conveyor || building.type == BuildingType.Miner)
                {
                    if (building.rotatedTiles == null || building.rotatedTiles.Length < 4)
                    {
                        Debug.LogWarning($"GameManager: Building '{building.buildingName}' does not have 4 rotated tiles!");
                        continue;
                    }
    
                    // Map the 4 rotated tiles to their corresponding directions
                    if (!tileDirectionMap.ContainsKey(building.rotatedTiles[0]))
                        tileDirectionMap.Add(building.rotatedTiles[0], new Vector2Int(1, 0));  // Right
                    if (!tileDirectionMap.ContainsKey(building.rotatedTiles[1]))
                        tileDirectionMap.Add(building.rotatedTiles[1], new Vector2Int(0, -1)); // Down
                    if (!tileDirectionMap.ContainsKey(building.rotatedTiles[2]))
                        tileDirectionMap.Add(building.rotatedTiles[2], new Vector2Int(-1, 0)); // Left
                    if (!tileDirectionMap.ContainsKey(building.rotatedTiles[3]))
                        tileDirectionMap.Add(building.rotatedTiles[3], new Vector2Int(0, 1));  // Up
                }
            }
        }
    
        public void ProccessSale(GameObject item)
        {
            item.TryGetComponent(out ConveyorItem citem);
            ObjectPoolManager.Instance.ReturnToPool(item);
            CurrencyManager.Instance.AddCurrency(citem.value);
        }
        
        public static string GetPrefix(string originalID)
        {
            // Remove "(Clone)" suffix if present
            if (originalID.EndsWith("(Clone)"))
            {
                originalID = originalID.Substring(0, originalID.Length - "(Clone)".Length);
            }
    
            // Find the first underscore to get the base prefix
            int underscoreIndex = originalID.IndexOf('_');
            if (underscoreIndex != -1)
            {
                return originalID.Substring(0, underscoreIndex); // "Iron_Ore" becomes "Iron"
            }
            return originalID; // If no underscore, the whole name is the prefix
        }
        
        // Refactored to use the pre-loaded Plates list
        public static ConveyorItem FindPlatePrefabWithPrefix(string prefix)
        {
            Debug.Log($"GameManager: Searching for plate prefab with prefix: '{prefix}'");
    
            if (Instance == null || Instance.Plates == null)
            {
                Debug.LogError("GameManager or its Plates list is not initialized.");
                return null;
            }
    
            if (Instance.Plates.Count == 0)
            {
                Debug.LogWarning("GameManager: Plates list is empty. No plate prefabs to search.");
                return null;
            }
    
            foreach (ConveyorItem plateConveyorItem in Instance.Plates)
            {
                if (plateConveyorItem == null || plateConveyorItem.gameObject == null)
                {
                    Debug.LogWarning("GameManager: Found null or invalid plate item in Plates list during search.");
                    continue;
                }
    
                string plateName = plateConveyorItem.gameObject.name;
                bool startsWithPrefix = plateName.StartsWith(prefix);
                bool endsWithPlate = plateName.EndsWith("Plate");
    
                Debug.Log($"GameManager: Checking plate '{plateName}' (Starts with '{prefix}': {startsWithPrefix}, Ends with 'Plate': {endsWithPlate})");
    
                if (startsWithPrefix && endsWithPlate)
                {
                    return plateConveyorItem;
                }
            }
            Debug.Log($"GameManager: No plate prefab found for prefix '{prefix}'.");
            return null;
        }
        
        public Vector2Int GetDirectionFromRotationIndex(int index)
        {
            switch (index)
            {
                case 0: return new Vector2Int(1, 0);  // Right
                case 1: return new Vector2Int(0, -1); // Down
                case 2: return new Vector2Int(-1, 0); // Left
                case 3: return new Vector2Int(0, 1);  // Up
                default: return Vector2Int.right;
            }
        }
    
        private void SpawnResourceNodes()
        {
            if (MainTileMap == null)
            {
                Debug.LogWarning("GameManager: MainTileMap is null! Cannot spawn resource nodes.");
                return;
            }
    
            if (resourceNodeDefinitions == null || resourceNodeDefinitions.Length == 0)
            {
                Debug.LogWarning("No ResourceNodeDefinitions assigned in GameManager. Cannot spawn nodes.");
                return;
            }
    
            // Calculate total spawn weight
            float totalWeight = 0;
            foreach (var def in resourceNodeDefinitions)
            {
                totalWeight += def.spawnWeight;
            }
    
            // Iterate through the tilemap bounds
            BoundsInt bounds = MainTileMap.cellBounds;
            HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>(); // Keep track of cells where nodes are spawned
    
            // Find IDT building data and center cell
            BuildingData idtData = null;
            if (allBuildings != null)
            {
                foreach (var b in allBuildings)
                {
                    if (b != null && b.type == BuildingType.Seller)
                    {
                        idtData = b;
                        break;
                    }
                }
            }
    
            Vector2Int centerCell = Vector2Int.zero;
            if (ZoneManager.Instance != null)
            {
                Vector2Int zoneSize = ZoneManager.Instance.ZoneSizeInTiles;
                centerCell = new Vector2Int(zoneSize.x / 2, zoneSize.y / 2);
            }
            else if (GridManager.Instance != null)
            {
                centerCell = GridManager.Instance.Center;
            }
    
            // Protect IDT cells so nodes don't spawn under/on the IDT
            if (idtData != null)
            {
                Vector2Int idtSize = idtData.size;
                for (int dx = 0; dx < idtSize.x; dx++)
                {
                    for (int dy = 0; dy < idtSize.y; dy++)
                    {
                        occupiedCells.Add(centerCell + new Vector2Int(dx, dy));
                    }
                }
            }
    
            // Find Coal definition
            ResourceNodeDefinition coalDefinition = null;
            if (resourceNodeDefinitions != null)
            {
                foreach (var def in resourceNodeDefinitions)
                {
                    if (def != null)
                    {
                        if (def.minedItemPrefab != null && def.minedItemPrefab.name.ToLower().Contains("coal"))
                        {
                            coalDefinition = def;
                            break;
                        }
                        if (def.resourceNodePrefab != null && def.resourceNodePrefab.name.ToLower().Contains("coal"))
                        {
                            coalDefinition = def;
                            break;
                        }
                    }
                }
                if (coalDefinition == null && resourceNodeDefinitions.Length > 0)
                {
                    coalDefinition = resourceNodeDefinitions[0];
                }
            }
    
            // Spawn a guaranteed Coal deposit near IDT
            if (coalDefinition != null)
            {
                Vector2Int coalStartCell = centerCell + new Vector2Int(-5, 0);
                
                // Validate starting tile exists on the MainTileMap, otherwise search nearby
                if (!MainTileMap.HasTile(new Vector3Int(coalStartCell.x, coalStartCell.y, 0)))
                {
                    Vector2Int[] fallbacks = {
                        new Vector2Int(-4, 0), new Vector2Int(-5, 1), new Vector2Int(-5, -1),
                        new Vector2Int(-6, 0), new Vector2Int(-3, 0)
                    };
                    foreach (var offset in fallbacks)
                    {
                        Vector2Int potential = centerCell + offset;
                        if (MainTileMap.HasTile(new Vector3Int(potential.x, potential.y, 0)))
                        {
                            coalStartCell = potential;
                            break;
                        }
                    }
                }
    
                SpawnPatch(coalStartCell, 5, coalDefinition, occupiedCells);
                Debug.Log($"GameManager: Spawned guaranteed Coal starting patch at {coalStartCell}");
            }
    
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector2Int cell = new Vector2Int(x, y);
    
                    // Skip if already occupied, if no tile is present, or if random chance fails
                    if (occupiedCells.Contains(cell) || !MainTileMap.HasTile(new Vector3Int(x, y, 0)) || Random.value > patchSpawnChance)
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
    
        private ResourceNodeDefinition GetRandomResourceNodeDefinition(float totalWeight)
        {
            float randomWeight = Random.Range(0, totalWeight);
            float currentWeight = 0;
    
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
    
        private void SpawnPatch(Vector2Int startCell, int patchSize, ResourceNodeDefinition definition, HashSet<Vector2Int> occupiedCells)
        {
            Queue<Vector2Int> cellsToProcess = new Queue<Vector2Int>();
            cellsToProcess.Enqueue(startCell);
            occupiedCells.Add(startCell); // Mark the starting cell as occupied
    
            int spawnedCount = 0;
    
            while (cellsToProcess.Count > 0 && spawnedCount < patchSize)
            {
                Vector2Int currentCell = cellsToProcess.Dequeue();
    
                // Spawn the node at currentCell
                SpawnSingleResourceNode(currentCell, definition);
                spawnedCount++;
    
                // Add neighbors to the queue if they are within bounds and not occupied
                Vector2Int[] neighbors = new Vector2Int[]
                {
                    currentCell + Vector2Int.right,
                    currentCell + Vector2Int.left,
                    currentCell + Vector2Int.up,
                    currentCell + Vector2Int.down
                };
    
                foreach (Vector2Int neighbor in neighbors)
                {
                    Vector3Int tempNeighbor = new Vector3Int(neighbor.x, neighbor.y, 0);
                    if (!MainTileMap.HasTile(tempNeighbor) || occupiedCells.Contains(neighbor)) // Check if tilemap has a tile (meaning it's a valid place) and not already occupied
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
    
    
    
        private void SpawnSingleResourceNode(Vector2Int cell, ResourceNodeDefinition definition)
        {
            // Get world position for instantiation
            Vector2 worldPos = GridManager.Instance.CellToWorldConversion(cell);
    
            // Instantiate the resource node prefab
            GameObject nodeGO = Instantiate(definition.resourceNodePrefab, worldPos, Quaternion.identity);
            nodeGO.transform.parent = this.transform; // Optional: parent to GameManager for organization
    
            // Get the ResourceNode component and set it up
            ResourceNode resourceNode = nodeGO.GetComponent<ResourceNode>();
            if (resourceNode != null)
            {
                resourceNode.Setup(cell, definition.oreGroundTile);
                // Assign properties from definition
                resourceNode.minedItemPrefab = definition.minedItemPrefab;
                resourceNode.miningSpeed = definition.miningSpeed;
                // Without this the node's reserve stays 0, so every miner immediately fails its
                // "oreCount <= 0" check and finite ores produce nothing at all.
                resourceNode.oreCount = definition.oreCount;
            }
            else
            {
                Debug.LogError($"ResourceNodePrefab '{definition.resourceNodePrefab.name}' is missing ResourceNode component!");
                Destroy(nodeGO); // Clean up if component is missing
            }
        }
    }
    
}


