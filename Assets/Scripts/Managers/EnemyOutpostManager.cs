using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
using System.Collections.Generic;
using UnityEngine;
using Singleton;

public class EnemyOutpost
{
    public List<BuildingLogic> buildings = new List<BuildingLogic>();
    public List<CartelMember> spawnedEnemies = new List<CartelMember>();
    public List<EnemySpawnerLogic> spawners = new List<EnemySpawnerLogic>();

    public bool IsCleared { get; private set; } = false;
    public bool IsClaimed { get; private set; } = false;

    public void RegisterBuilding(BuildingLogic building)
    {
        if (building == null) return;
        
        building.outpost = this;
        buildings.Add(building);
        
        if (building is EnemySpawnerLogic spawner)
        {
            spawners.Add(spawner);
        }
    }

    public void RegisterSpawnedEnemy(CartelMember enemy)
    {
        if (enemy == null) return;
        if (!spawnedEnemies.Contains(enemy))
        {
            spawnedEnemies.Add(enemy);
        }
    }

    public void RemoveBuilding(BuildingLogic building)
    {
        if (building == null) return;
        buildings.Remove(building);
        if (building is EnemySpawnerLogic spawner)
        {
            spawners.Remove(spawner);
        }
        CheckClearedStatus();
    }

    public void UpdateStatus()
    {
        CheckClearedStatus();
    }

    private void CheckClearedStatus()
    {
        if (IsCleared) return;

        spawners.RemoveAll(s => s == null);

        // Clear the outpost immediately when the spawner building is destroyed
        if (spawners.Count == 0)
        {
            IsCleared = true;
            Debug.Log("Enemy Outpost Cleared!");
            
            if (PlayerController.Instance != null)
            {
                FloatingTextSettings settings = Resources.Load<FloatingTextSettings>("FloatingTextSettings/OutpostClearedSettings");
                FloatingTextManager.Instance.Spawn("OUTPOST CLEARED!", PlayerController.Instance.transform.position, settings);
            }
            
            Code.Scripts.EventSystems.EventManager.Instance?.Publish(new EnemyOutpostClearedEvent(this));
        }
    }

    public void ClaimAllBuildings()
    {
        if (!IsCleared || IsClaimed) return;

        IsClaimed = true;

        foreach (var building in buildings)
        {
            if (building != null && building.isEnemyOwned)
            {
                building.isEnemyOwned = false;
                
                // Reset color tint of the tile to white
                Vector2Int cell = building.GetMyCell();
                Vector3Int tilePos = new Vector3Int(cell.x, cell.y, 0);
                if (GameManager.Instance != null && GameManager.Instance.BuildingTileMap != null)
                {
                    GameManager.Instance.BuildingTileMap.SetTileFlags(tilePos, UnityEngine.Tilemaps.TileFlags.None);
                    GameManager.Instance.BuildingTileMap.SetColor(tilePos, Color.white);
                }

                // If it's a turret, reset weapon flag
                if (building is TurretLogic turret)
                {
                    var turretWeapon = turret.GetComponentInChildren<TurretWeapon>();
                    if (turretWeapon != null)
                    {
                        turretWeapon.isEnemyFired = false;
                    }
                }

                // Spawn floating "+CLAIMED!" text
                FloatingTextSettings settings = Resources.Load<FloatingTextSettings>("FloatingTextSettings/ClaimedSettings");
                FloatingTextManager.Instance.Spawn("CLAIMED!", building.transform.position, settings);
            }
        }

        Debug.Log("Enemy Outpost Claimed by Player!");
    }
}

namespace Managers
{
    public class EnemyOutpostManager : SingletonBase<EnemyOutpostManager>
    {
        [Header("Outpost Spawning Settings")]
        public int minOutpostSize = 4;
        public int maxOutpostSize = 8;
        public float outpostSpawnChance = 0.003f; // Reduced from 0.015f to prevent overcrowding
        public float minDistanceBetweenOutposts = 20f; // Minimum distance in tiles between outposts

        private List<EnemyOutpost> activeOutposts = new List<EnemyOutpost>();
        private List<Vector2Int> outpostCenters = new List<Vector2Int>();

        protected override void Awake()
        {
            persistBetweenScenes = false;
            base.Awake();
        }

        private void Start()
        {
            GenerateOutposts();
        }

        private void Update()
        {
            for (int i = activeOutposts.Count - 1; i >= 0; i--)
            {
                if (activeOutposts[i] != null)
                {
                    activeOutposts[i].UpdateStatus();
                }
            }
        }

        private void GenerateOutposts()
        {
            if (TutorialManager.HasInstance && !TutorialManager.Instance.IsTutorialCompleted())
            {
                Debug.Log("EnemyOutpostManager: Tutorial is active. Skipping random outpost generation.");
                return;
            }

            if (GameManager.Instance == null || GameManager.Instance.MainTileMap == null)
            {
                Debug.LogWarning("EnemyOutpostManager: GameManager or MainTileMap is null. Cannot generate outposts.");
                return;
            }

            BoundsInt bounds = GameManager.Instance.MainTileMap.cellBounds;
            HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();

            // Find existing buildings/nodes to exclude
            if (PlacementManager.HasInstance)
            {
                foreach (var cell in PlacementManager.Instance.GetActiveBuildings().Keys)
                {
                    occupiedCells.Add(cell);
                }
            }

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector2Int cell = new Vector2Int(x, y);

                    // Skip if occupied, no tile present, too close to starting center (0,0), or chance fails
                    if (occupiedCells.Contains(cell) || !GameManager.Instance.MainTileMap.HasTile(new Vector3Int(x, y, 0)))
                    {
                        continue;
                    }

                    // Prevent generating in starting zone (0, 0)
                    if (ZoneManager.Instance != null)
                    {
                        Vector2Int zoneCoords = ZoneManager.Instance.GetZoneCoordsFromTile(new Vector3Int(cell.x, cell.y, 0));
                        if (zoneCoords == Vector2Int.zero)
                        {
                            continue;
                        }
                    }

                    // Keep starting area clear (within 10 tiles of (0,0) or GridManager center)
                    Vector2Int center = GridManager.Instance != null ? GridManager.Instance.center : Vector2Int.zero;
                    if (Vector2Int.Distance(cell, center) < 12f)
                    {
                        continue;
                    }

                    // Ensure minimum distance from all other outposts
                    bool tooClose = false;
                    foreach (var outpostCenter in outpostCenters)
                    {
                        if (Vector2Int.Distance(cell, outpostCenter) < minDistanceBetweenOutposts)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                    if (tooClose)
                    {
                        continue;
                    }

                    if (Random.value > outpostSpawnChance)
                    {
                        continue;
                    }

                    int patchSize = Random.Range(minOutpostSize, maxOutpostSize + 1);
                    SpawnOutpostPatch(cell, patchSize, occupiedCells);
                }
            }

            Debug.Log($"EnemyOutpostManager: Generated {activeOutposts.Count} enemy outposts.");
        }

        private void SpawnOutpostPatch(Vector2Int startCell, int patchSize, HashSet<Vector2Int> occupiedCells)
        {
            outpostCenters.Add(startCell);
            Queue<Vector2Int> cellsToProcess = new Queue<Vector2Int>();
            cellsToProcess.Enqueue(startCell);
            occupiedCells.Add(startCell);

            List<Vector2Int> patchCells = new List<Vector2Int>();

            while (cellsToProcess.Count > 0 && patchCells.Count < patchSize)
            {
                Vector2Int currentCell = cellsToProcess.Dequeue();
                patchCells.Add(currentCell);

                Vector2Int[] neighbors = new Vector2Int[]
                {
                    currentCell + Vector2Int.right,
                    currentCell + Vector2Int.left,
                    currentCell + Vector2Int.up,
                    currentCell + Vector2Int.down
                };

                foreach (var neighbor in neighbors)
                {
                    Vector3Int tempNeighbor = new Vector3Int(neighbor.x, neighbor.y, 0);
                    if (!GameManager.Instance.MainTileMap.HasTile(tempNeighbor) || occupiedCells.Contains(neighbor))
                    {
                        continue;
                    }

                    // Check resource nodes
                    if (ResourceManager.Instance != null && ResourceManager.Instance.GetNodeAtPosition(neighbor) != null)
                    {
                        occupiedCells.Add(neighbor);
                        continue;
                    }

                    // Check other buildings
                    if (PlacementManager.HasInstance && PlacementManager.Instance.GetActiveBuildings().ContainsKey(neighbor))
                    {
                        occupiedCells.Add(neighbor);
                        continue;
                    }

                    occupiedCells.Add(neighbor);
                    cellsToProcess.Enqueue(neighbor);
                }
            }

            if (patchCells.Count > 0)
            {
                CreateOutpostAt(patchCells);
            }
        }

        private bool CanPlaceStructureOfSize(Vector2Int baseCell, Vector2Int size)
        {
            if (GameManager.Instance == null || GameManager.Instance.MainTileMap == null) return false;

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector2Int cell = baseCell + new Vector2Int(x, y);
                    Vector3Int tilePos = new Vector3Int(cell.x, cell.y, 0);

                    // Must be on the map
                    if (!GameManager.Instance.MainTileMap.HasTile(tilePos))
                        return false;

                    // Must not have a resource node
                    if (ResourceManager.Instance != null && ResourceManager.Instance.GetNodeAtPosition(cell) != null)
                        return false;

                    // Must not have another active building
                    if (PlacementManager.HasInstance && PlacementManager.Instance.GetActiveBuildings().ContainsKey(cell))
                        return false;
                }
            }
            return true;
        }

        private void CreateOutpostAt(List<Vector2Int> cells)
        {
            EnemyOutpost outpost = new EnemyOutpost();
            activeOutposts.Add(outpost);

            for (int i = 0; i < cells.Count; i++)
            {
                Vector2Int cell = cells[i];

                // Skip if this cell is already occupied by a previously placed multi-cell building
                if (PlacementManager.HasInstance && PlacementManager.Instance.GetActiveBuildings().ContainsKey(cell))
                {
                    continue;
                }

                BuildingLogic building = null;

                if (i == 0)
                {
                    // Center is the spawner
                    building = SpawnEnemySpawner(cell);
                }
                else if (i % 3 == 0)
                {
                    // Every 3rd block is a turret
                    building = SpawnEnemyTurret(cell);
                }
                else
                {
                    // Other blocks are walls or generic factory buildings
                    float rand = Random.value;
                    if (rand < 0.6f)
                    {
                        building = SpawnEnemyWall(cell);
                    }
                    else
                    {
                        building = SpawnEnemyFactoryBlock(cell);
                    }
                }

                if (building != null)
                {
                    outpost.RegisterBuilding(building);
                }
            }
        }

        private BuildingLogic SpawnEnemyBuilding(Vector2Int cell, Buildings.BuildingData buildingData, System.Type logicType)
        {
            if (buildingData == null || buildingData.rotatedTiles == null || buildingData.rotatedTiles.Length == 0)
            {
                return null;
            }

            Vector3Int tilePos = new Vector3Int(cell.x, cell.y, 0);
            
            if (GameManager.Instance != null && GameManager.Instance.BuildingTileMap != null)
            {
                GameManager.Instance.BuildingTileMap.SetTile(tilePos, buildingData.rotatedTiles[0]);
                GameManager.Instance.BuildingTileMap.SetTileFlags(tilePos, UnityEngine.Tilemaps.TileFlags.None);
                // GameManager.Instance.BuildingTileMap.SetColor(tilePos, new Color(1f, 0.4f, 0.4f, 1f));
            }

            GameObject buildingObj = new GameObject(buildingData.buildingName + "_Enemy_Logic_" + cell);
            buildingObj.transform.position = GridManager.Instance.CellToWorldConversion(cell);
            
            BuildingLogic logic = buildingObj.AddComponent(logicType) as BuildingLogic;
            if (logic == null)
            {
                Destroy(buildingObj);
                return null;
            }
            
            logic.isEnemyOwned = true;
            logic.Setup(buildingData, cell);
 
            // Calculate all occupied cells based on size
            List<Vector2Int> occupied = new List<Vector2Int>();
            Vector2Int size = buildingData.size;
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    occupied.Add(cell + new Vector2Int(x, y));
                }
            }
            logic.SetOccupiedCells(occupied);
 
            if (PlacementManager.HasInstance)
            {
                foreach (var occupiedCell in occupied)
                {
                    PlacementManager.Instance.RegisterActiveBuilding(occupiedCell, buildingObj);
                }
            }
 
            return logic;
        }

        private EnemySpawnerLogic SpawnEnemySpawner(Vector2Int cell)
        {
            Buildings.BuildingData spawnerData = Resources.Load<Buildings.BuildingData>("BuildingData/EnemySpawner");
            EnemySpawnerLogic spawner = SpawnEnemyBuilding(cell, spawnerData, typeof(EnemySpawnerLogic)) as EnemySpawnerLogic;
            if (spawner != null)
            {
                spawner.activationRange = 9f;
            }
            return spawner;
        }

        private TurretLogic SpawnEnemyTurret(Vector2Int cell)
        {
            Buildings.BuildingData turretData = Resources.Load<Buildings.BuildingData>("BuildingData/EnemyTurret");
            TurretLogic turret = SpawnEnemyBuilding(cell, turretData, typeof(TurretLogic)) as TurretLogic;
            return turret;
        }

        private WallLogic SpawnEnemyWall(Vector2Int cell)
        {
            Buildings.BuildingData wallData = Resources.Load<Buildings.BuildingData>("BuildingData/EnemyWall");
            WallLogic wall = SpawnEnemyBuilding(cell, wallData, typeof(WallLogic)) as WallLogic;
            return wall;
        }

        private BuildingLogic SpawnEnemyFactoryBlock(Vector2Int cell)
        {
            float rand = Random.value;
            if (rand < 0.4f)
            {
                Buildings.BuildingData data = Resources.Load<Buildings.BuildingData>("BuildingData/EnemyFurnace");
                if (data != null && CanPlaceStructureOfSize(cell, data.size))
                {
                    BuildingLogic furnace = SpawnEnemyBuilding(cell, data, typeof(Furnace));
                    return furnace;
                }
            }
            else if (rand < 0.8f)
            {
                Buildings.BuildingData data = Resources.Load<Buildings.BuildingData>("BuildingData/EnemyMine");
                if (data != null && CanPlaceStructureOfSize(cell, data.size))
                {
                    BuildingLogic miner = SpawnEnemyBuilding(cell, data, typeof(MinerLogic));
                    return miner;
                }
            }

            // Spawning Wall (1x1)
            Buildings.BuildingData wallData = Resources.Load<Buildings.BuildingData>("BuildingData/EnemyWall");
            BuildingLogic wall = SpawnEnemyBuilding(cell, wallData, typeof(WallLogic));
            return wall;
        }

        public EnemyOutpost SpawnTutorialOutpost(Vector2Int startCell, int patchSize)
        {
            List<Vector2Int> patchCells = new List<Vector2Int>();
            Queue<Vector2Int> cellsToProcess = new Queue<Vector2Int>();
            HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();
            
            cellsToProcess.Enqueue(startCell);
            occupiedCells.Add(startCell);
            
            while (cellsToProcess.Count > 0 && patchCells.Count < patchSize)
            {
                Vector2Int currentCell = cellsToProcess.Dequeue();
                patchCells.Add(currentCell);
                
                Vector2Int[] neighbors = new Vector2Int[]
                {
                    currentCell + Vector2Int.right,
                    currentCell + Vector2Int.left,
                    currentCell + Vector2Int.up,
                    currentCell + Vector2Int.down
                };
                
                foreach (var neighbor in neighbors)
                {
                    Vector3Int tempNeighbor = new Vector3Int(neighbor.x, neighbor.y, 0);
                    if (!GameManager.Instance.MainTileMap.HasTile(tempNeighbor) || occupiedCells.Contains(neighbor))
                    {
                        continue;
                    }
                    if (ResourceManager.Instance != null && ResourceManager.Instance.GetNodeAtPosition(neighbor) != null)
                    {
                        occupiedCells.Add(neighbor);
                        continue;
                    }
                    if (PlacementManager.HasInstance && PlacementManager.Instance.GetActiveBuildings().ContainsKey(neighbor))
                    {
                        occupiedCells.Add(neighbor);
                        continue;
                    }
                    
                    occupiedCells.Add(neighbor);
                    cellsToProcess.Enqueue(neighbor);
                }
            }
            
            if (patchCells.Count > 0)
            {
                EnemyOutpost outpost = new EnemyOutpost();
                activeOutposts.Add(outpost);
                for (int i = 0; i < patchCells.Count; i++)
                {
                    Vector2Int cell = patchCells[i];
                    if (PlacementManager.HasInstance && PlacementManager.Instance.GetActiveBuildings().ContainsKey(cell)) continue;
                    
                    BuildingLogic building = null;
                    if (i == 0) building = SpawnEnemySpawner(cell);
                    else if (i % 3 == 0) building = SpawnEnemyTurret(cell);
                    else
                    {
                        float rand = Random.value;
                        if (rand < 0.6f) building = SpawnEnemyWall(cell);
                        else building = SpawnEnemyFactoryBlock(cell);
                    }
                    
                    if (building != null)
                    {
                        outpost.RegisterBuilding(building);
                    }
                }
                outpostCenters.Add(startCell);
                return outpost;
            }
            return null;
        }
    }
}

