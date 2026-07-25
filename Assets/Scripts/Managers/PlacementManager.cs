using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
namespace Managers
{
    using System.Collections.Generic;
    using Buildings;
    using Singleton;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.Tilemaps;
    
    public class PlacementManager : SingletonBase<PlacementManager>
    {
        public static int PlacementVersion { get; private set; } = 0;

        public Tilemap mainTilemap;
        public Tilemap previewTilemap;
        
        public event System.Action<BuildingData> OnBuildingPlaced;
    
        private BuildingData activeBuilding;
        private int rotationIndex = 0;
        private Camera cam;
    
        public float maxPlacementDistance = 5f;
        private Transform playerTransform;
        private PlayerController playerController;
    
        protected override void Awake()
        {
            persistBetweenScenes = false;
            base.Awake();
    
            if (mainTilemap == null || previewTilemap == null)
            {
                GameObject gridObj = GameObject.Find("Grid");
                if (gridObj != null)
                {
                    if (mainTilemap == null)
                    {
                        mainTilemap = GameManager.Instance.BuildingTileMap ;
                    }
                    if (previewTilemap == null)
                    {
                        Transform t = gridObj.transform.Find("PreviewTilemap");
                        if (t != null) previewTilemap = t.GetComponent<Tilemap>();
                    }
                }
            }
        }
    
        void Start()
        {
            mainTilemap = GameManager.Instance.BuildingTileMap;
            cam = Camera.main;
            playerController = GameManager.Instance.playerController;
        }
    
        void Update()
        {
            if (PauseManager.IsPaused)
            {
                if (previewTilemap != null) previewTilemap.ClearAllTiles();
                return;
            }
    
            if (PlayerController.Instance == null || PlayerController.Instance.currentMode == PlayerController.PlayerMode.Combat)
            {
                if (previewTilemap != null) previewTilemap.ClearAllTiles();
                return;
            }

            // Placement is only for free-roam (idle/walking) - block it whenever a building panel,
            // the store, or any other UI has taken over player input, even if that UI happens to be
            // rendered somewhere the mouse can click "through" onto the world underneath.
            var playerStateMachine = PlayerController.Instance.StateMachine;
            if (playerStateMachine.CurrentState != playerStateMachine.idleState && playerStateMachine.CurrentState != playerStateMachine.walkState)
            {
                if (previewTilemap != null) previewTilemap.ClearAllTiles();
                return;
            }

            // Dragging an inventory item holds the left mouse button down and moves it across the
            // screen - the same gesture as click-and-drag placement. Don't place buildings out from
            // under a drag in progress.
            if (DragLayer.HasInstance && DragLayer.Instance.IsDragging)
            {
                if (previewTilemap != null) previewTilemap.ClearAllTiles();
                return;
            }

            if (activeBuilding == null) return;
    
            Vector2Int cell = GetMouseCell();
            Vector3Int vector3Cell = new Vector3Int(cell.x, cell.y, 0);
            Vector3 worldPos = GridManager.Instance.CellToWorldConversion(cell);
            
            bool isStoreOpen = StoreUiScript.HasInstance && StoreUiScript.Instance.gameObject.activeInHierarchy;
            if (isStoreOpen && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                // If it is, clear the ghost preview and stop here
                previewTilemap.ClearAllTiles();
                return; 
            }
    
            // Task 2: Placement Distance Check
            bool isWithinDistance = true;
            if (playerController != null)
            {
                float distance = Vector2.Distance(playerController.transform.position, worldPos);
                if (distance > maxPlacementDistance)
                {
                    isWithinDistance = false;
                }
            }
            
            // Rotate (R)
            if (Input.GetKeyDown(KeyCode.R)) rotationIndex = (rotationIndex + 1) % 4;// set the rotation index i.e. right,left,up,down
    
            List<Vector2Int> occupiedCells = GetOccupiedCells(cell, activeBuilding.size, rotationIndex);
            bool canPlace = true;
    
            // Preview
            previewTilemap.ClearAllTiles();
            
            foreach (var occupiedCell in occupiedCells)
            {
                Vector3Int pos3 = new Vector3Int(occupiedCell.x, occupiedCell.y, 0);
                if (!ZoneManager.Instance.IsTileInsideUnlockedZone(pos3) || !isWithinDistance || activeBuildings.ContainsKey(occupiedCell))
                {
                    canPlace = false;
                    // Optional: show red preview?
                }
            }
    
            if (canPlace)
            {
                previewTilemap.SetTile(vector3Cell, activeBuilding.rotatedTiles[rotationIndex]);
            }
    
            // Place
            if (Input.GetMouseButton(0))
            {
                if (!isWithinDistance)
                {
                    Debug.Log("Too far to place!");
                    return;
                }
    
                if (!canPlace)
                {
                    Debug.Log("Cannot place here (blocked or outside zone)!");
                    return;
                }
    
                // Task 1: Check Inventory instead of Currency
                if (!InventoryManager.Instance.HasBuilding(activeBuilding))
                {
                    Debug.Log("Don't have " + activeBuilding.buildingName + " in inventory!");
                    return;
                }
    
                foreach (var occupiedCell in occupiedCells)
                {
                    if (ItemTracker.Instance != null)
                    {
                        List<ConveyorItem> itemsInCell = ItemTracker.Instance.GetItemsInCell(occupiedCell);
                        if (itemsInCell != null)
                        {
                            for (int i = itemsInCell.Count - 1; i >= 0; i--)
                            {
                                ConveyorItem item = itemsInCell[i];
                                if (item != null)
                                {
                                    ItemData rType = item._itemData;
                                    if (BuildingUiManager.Instance != null)
                                    {
                                        BuildingUiManager.Instance.AddResource(rType, 1);
                                    }
                                    FloatingTextSettings settings = Resources.Load<FloatingTextSettings>("FloatingTextSettings/PlayerPickupSettings");
                                    if (FloatingTextManager.Instance != null && settings != null)
                                    {
                                        FloatingTextManager.Instance.Spawn("+1 " + rType.ToString().ToUpper(), item.transform.position, settings);
                                    }
                                    if (ObjectPoolManager.Instance != null)
                                    {
                                        ObjectPoolManager.Instance.ReturnToPool(item.gameObject);
                                    }
                                    else
                                    {
                                        Destroy(item.gameObject);
                                    }
                                }
                            }
                        }
                    }
                }
    
                mainTilemap.SetTile(vector3Cell, activeBuilding.rotatedTiles[rotationIndex]);
                GameObject buildingObj = null;
                switch (activeBuilding.type)
                {
                    case (BuildingType.Chest):
                        buildingObj = SpawnChestLogic(cell);
                        break;
                    case (BuildingType.Conveyor):
                        buildingObj = SpawnBeltLogic(cell);
                        break;
                    case(BuildingType.Miner):
                        buildingObj = SpawnMinerLogic(cell);
                        break;
                    case (BuildingType.Seller):
                        buildingObj = SpawnSellerLogic(cell);
                        break;
                    case (BuildingType.Furnace):
                        buildingObj = SpawnFurnaceLogic(cell);   
                        break;
                    case (BuildingType.Wall):
                        buildingObj = SpawnWallLogic(cell);
                        break;
                    case (BuildingType.Turret):
                        buildingObj = SpawnTurretLogic(cell);
                        break;
                }
    
                if (buildingObj != null)
                {
                    BuildingLogic logic = buildingObj.GetComponent<BuildingLogic>();
                    if (logic != null)
                    {
                        logic.SetOccupiedCells(occupiedCells);
                        foreach (var occupiedCell in occupiedCells)
                        {
                            if (occupiedCell != cell) // base cell already added in SpawnXLogic
                            {
                                activeBuildings[occupiedCell] = buildingObj;
                            }
                        }
                    }
                }
    
                // Task 1: Consume from Inventory
                InventoryManager.Instance.RemoveBuilding(activeBuilding);
                if (HotbarManager.HasInstance)
                {
                    HotbarManager.Instance.RemoveFromHotbar(activeBuilding);
                }

                OnBuildingPlaced?.Invoke(activeBuilding);
            }
                
    
            // Delete / Claim
            if (Input.GetMouseButtonDown(1))
            {
                if (activeBuildings.ContainsKey(cell))
                {
                    GameObject buildingObj = activeBuildings[cell];
                    if (buildingObj != null)
                    {
                        BuildingLogic logic = buildingObj.GetComponent<BuildingLogic>();
                        if (logic != null)
                        {
                            if (logic.isEnemyOwned)
                            {
                                if (logic.outpost != null && logic.outpost.IsCleared)
                                {
                                    // Claim the entire outpost!
                                    logic.outpost.ClaimAllBuildings();
                                }
                                else
                                {
                                    // Enemies still remain
                                    FloatingTextSettings settings = Resources.Load<FloatingTextSettings>("FloatingTextSettings/EnemiesRemainSettings");
                                    FloatingTextManager.Instance.Spawn("ENEMIES REMAIN!", logic.transform.position, settings);
                                    Debug.LogWarning("Cannot claim or destroy this building. Defeat all enemies in the outpost first!");
                                }
                            }
                            else
                            {
                                // Player-owned, destroy normally
                                DestroyBuilding(cell, true);
                            }
                        }
                        else
                        {
                            DestroyBuilding(cell, true);
                        }
                    }
                }
            }
        }
    
        public void DestroyBuilding(Vector2Int cell, bool returnToInventory)
        {
            if (activeBuildings.ContainsKey(cell))
            {
                GameObject buildingObj = activeBuildings[cell];
                BuildingLogic logic = buildingObj.GetComponent<BuildingLogic>();
                
                if (logic != null)
                {
                    PlacementVersion++;
                    if (returnToInventory && logic.data != null)
                    {
                        Buildings.BuildingData buildingToGive = logic.data;
                        if (logic.data.name != null && logic.data.name.StartsWith("Enemy"))
                        {
                            string standardName = logic.data.name.Substring("Enemy".Length);
                            Buildings.BuildingData standardData = Resources.Load<Buildings.BuildingData>("BuildingData/" + standardName);
                            if (standardData != null)
                            {
                                buildingToGive = standardData;
                            }
                        }
                        // Task 1: Return to Inventory instead of refunding currency
                        InventoryManager.Instance.AddBuilding(buildingToGive);
                        if (BuildingUiManager.HasInstance && buildingToGive.itemData != null)
                        {
                            BuildingUiManager.Instance.ResourcesToHotBar(buildingToGive.itemData, 1);
                        }
                    }
    
                    // Clear all occupied tiles
                    foreach (var occupiedCell in logic.occupiedCells)
                    {
                        activeBuildings.Remove(occupiedCell);
                    }
                    
                    mainTilemap.SetTile(new Vector3Int(logic.GetMyCell().x, logic.GetMyCell().y, 0), null);
    
                    Destroy(buildingObj);
                }
            }
        }
    
        public List<Vector2Int> GetOccupiedCells(Vector2Int baseCell, Vector2Int size, int rotationIndex)
        {
            List<Vector2Int> cells = new List<Vector2Int>();
            Vector2Int actualSize = size;
            if (rotationIndex % 2 != 0) // 1: Down, 3: Up
            {
                actualSize = new Vector2Int(size.y, size.x);
            }
    
            for (int x = 0; x < actualSize.x; x++)
            {
                for (int y = 0; y < actualSize.y; y++)
                {
                    cells.Add(baseCell + new Vector2Int(x, y));
                }
            }
            return cells;
        }
    
        public void ChangeSelection(BuildingData newBuilding)
        {
            activeBuilding = newBuilding;
            rotationIndex = 0;
            
            // Clear preview if we deselected
            if (activeBuilding == null)
            {
                previewTilemap.ClearAllTiles();
            }
        }
    
        Vector2Int GetMouseCell()
        {
            Vector2 p = cam.ScreenToWorldPoint(Input.mousePosition);
            return GridManager.Instance.WorldToCellConversion(new Vector2(p.x, p.y));
        }
        
        public bool IsCellWalkable(Vector2Int cell)
        {
            if (activeBuildings.TryGetValue(cell, out GameObject obj))
            {
                BuildingLogic logic = obj.GetComponent<BuildingLogic>();
                if (logic != null && logic.data != null)
                {
                    // Can only walk through conveyors, all other buildings block movement
                    if (logic.data.type == BuildingType.Conveyor)
                    {
                        return true;
                    }
                    return false;
                }
            }
            return true;
        }
    
        public Dictionary<Vector2Int, GameObject> GetActiveBuildings()
        {
            return activeBuildings;
        }
    
        public void RegisterActiveBuilding(Vector2Int cell, GameObject buildingObj)
        {
            activeBuildings[cell] = buildingObj;
        }
    
        public void SpawnSellerProgrammatically(BuildingData building, Vector2Int cell, int rotation = 0)
        {
            PlacementVersion++;
            Vector3Int vector3Cell = new Vector3Int(cell.x, cell.y, 0);
            if (mainTilemap == null) mainTilemap = GameManager.Instance.BuildingTileMap;
            mainTilemap.SetTile(vector3Cell, building.rotatedTiles[rotation]);
    
            BuildingData prevActive = activeBuilding;
            int prevRotation = rotationIndex;
    
            activeBuilding = building;
            rotationIndex = rotation;
    
            GameObject buildingObj = SpawnSellerLogic(cell);
            BuildingLogic logic = buildingObj.GetComponent<BuildingLogic>();
            if (logic != null)
            {
                List<Vector2Int> occupiedCells = GetOccupiedCells(cell, building.size, rotation);
                logic.SetOccupiedCells(occupiedCells);
                foreach (var occupiedCell in occupiedCells)
                {
                    if (occupiedCell != cell)
                    {
                        activeBuildings[occupiedCell] = buildingObj;
                    }
                }
            }
    
            activeBuilding = prevActive;
            rotationIndex = prevRotation;
        }
    
        private Dictionary<Vector2Int, GameObject> activeBuildings = new Dictionary<Vector2Int, GameObject>();
    
        GameObject SpawnMinerLogic(Vector2Int cell)
        {
            PlacementVersion++;
            // Create the logic object
            GameObject MinerObj = new GameObject("Miner_Logic_" + cell);
            MinerObj.transform.position = GridManager.Instance.CellToWorldConversion(cell);
            MinerLogic logic = MinerObj.AddComponent<MinerLogic>();
            logic.Setup(activeBuilding, cell, rotationIndex);
    
            // Store it so we can delete it later if needed
            activeBuildings.Add(cell, MinerObj);
            return MinerObj;
        }
    
        GameObject SpawnFurnaceLogic(Vector2Int cell)
        {
            PlacementVersion++;
            GameObject FurnaceObj = new GameObject("Furnace_Logic_" + cell);
            FurnaceObj.transform.position = GridManager.Instance.CellToWorldConversion(cell);
            Furnace furnace = FurnaceObj.AddComponent<Furnace>();
            furnace.Setup(activeBuilding, cell, rotationIndex, activeBuilding.proccessingSpeed);
            activeBuildings.Add(cell, FurnaceObj);
            return FurnaceObj;
        }
    
        GameObject SpawnSellerLogic(Vector2Int cell)
        {
            PlacementVersion++;
            GameObject sellerObj = new GameObject("Seller_Logic_" + cell);
            sellerObj.transform.position = GridManager.Instance.CellToWorldConversion(cell);
            InterDimensionalTransporter interDimensionalTransporter = sellerObj.AddComponent<InterDimensionalTransporter>();
            interDimensionalTransporter.Setup(activeBuilding, cell, rotationIndex);
            activeBuildings.Add(cell, sellerObj);
            return sellerObj;
        }
    
        GameObject SpawnBeltLogic(Vector2Int cell)
        {
            PlacementVersion++;
            GameObject beltObj = new GameObject("Belt_Logic_" + cell);
            beltObj.transform.position = GridManager.Instance.CellToWorldConversion(cell);
            
            ConveyorLogic logic = beltObj.AddComponent<ConveyorLogic>();
            Vector2Int dir = GameManager.Instance.GetDirectionFromRotationIndex(rotationIndex);
            logic.Setup(activeBuilding, cell, dir, activeBuilding.proccessingSpeed); // Pass data
            
            activeBuildings.Add(cell, beltObj);
            return beltObj;
        }
    
        GameObject SpawnChestLogic(Vector2Int cell)
        {
            PlacementVersion++;
            GameObject chestObj = new GameObject("Chest_Logic_" + cell);
            chestObj.transform.position = GridManager.Instance.CellToWorldConversion(cell);
            Chest logic = chestObj.AddComponent<Chest>();
            logic.Setup(activeBuilding, cell, rotationIndex);

            activeBuildings.Add(cell, chestObj);
            return chestObj;
        }

        GameObject SpawnWallLogic(Vector2Int cell)
        {
            PlacementVersion++;
            GameObject wallObj = new GameObject("Wall_Logic_" + cell);
            wallObj.transform.position = GridManager.Instance.CellToWorldConversion(cell);
            WallLogic logic = wallObj.AddComponent<WallLogic>();
            logic.Setup(activeBuilding, cell);
            
            activeBuildings.Add(cell, wallObj);
            return wallObj;
        }
    
        GameObject SpawnTurretLogic(Vector2Int cell)
        {
            PlacementVersion++;
            GameObject turretObj = new GameObject("Turret_Logic_" + cell);
            turretObj.transform.position = GridManager.Instance.CellToWorldConversion(cell);
            TurretLogic logic = turretObj.AddComponent<TurretLogic>();
            logic.Setup(activeBuilding, cell);
            
            activeBuildings.Add(cell, turretObj);
            return turretObj;
        }
    }
}


