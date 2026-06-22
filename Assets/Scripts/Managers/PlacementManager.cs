using System.Collections.Generic;
using Buildings;
using Singleton;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class PlacementManager : SingletonBase<PlacementManager>
{
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
        if (activeBuilding == null) return;

        Vector2Int cell = GetMouseCell();
        Vector3Int vector3Cell = new Vector3Int(cell.x, cell.y, 0);
        Vector3 worldPos = GridManager.Instance.CellToWorldConversion(cell);
        
        if (EventSystem.current.IsPointerOverGameObject())
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
        if (Input.GetMouseButtonDown(0))
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

            mainTilemap.SetTile(vector3Cell, activeBuilding.rotatedTiles[rotationIndex]);
            GameObject buildingObj = null;
            switch (activeBuilding.type)
            {
                case (BuildingType.Chest):
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
            
            OnBuildingPlaced?.Invoke(activeBuilding);
        }
            

        // Delete
        if (Input.GetMouseButtonDown(1))
        {
            if (activeBuildings.ContainsKey(cell))
            {
                DestroyBuilding(cell, true);
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
                if (returnToInventory && logic.data != null)
                {
                    // Task 1: Return to Inventory instead of refunding currency
                    InventoryManager.Instance.AddBuilding(logic.data);
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

    private Dictionary<Vector2Int, GameObject> activeBuildings = new Dictionary<Vector2Int, GameObject>();

    GameObject SpawnMinerLogic(Vector2Int cell)
    {
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
        GameObject FurnaceObj = new GameObject("Furnace_Logic_" + cell);
        FurnaceObj.transform.position = GridManager.Instance.CellToWorldConversion(cell);
        Furnace furnace = FurnaceObj.AddComponent<Furnace>();
        furnace.Setup(activeBuilding, cell, rotationIndex, activeBuilding.proccessingSpeed);
        activeBuildings.Add(cell, FurnaceObj);
        return FurnaceObj;
    }

    GameObject SpawnSellerLogic(Vector2Int cell)
    {
        GameObject sellerObj = new GameObject("Seller_Logic_" + cell);
        sellerObj.transform.position = GridManager.Instance.CellToWorldConversion(cell);
        Seller seller = sellerObj.AddComponent<Seller>();
        seller.Setup(activeBuilding, cell);
        activeBuildings.Add(cell, sellerObj);
        return sellerObj;
    }

    GameObject SpawnBeltLogic(Vector2Int cell)
    {
        GameObject beltObj = new GameObject("Belt_Logic_" + cell);
        beltObj.transform.position = GridManager.Instance.CellToWorldConversion(cell);
        
        ConveyorLogic logic = beltObj.AddComponent<ConveyorLogic>();
        Vector2Int dir = GameManager.Instance.GetDirectionFromRotationIndex(rotationIndex);
        logic.Setup(activeBuilding, cell, dir, activeBuilding.proccessingSpeed); // Pass data
        
        activeBuildings.Add(cell, beltObj);
        return beltObj;
    }

    GameObject SpawnWallLogic(Vector2Int cell)
    {
        GameObject wallObj = new GameObject("Wall_Logic_" + cell);
        wallObj.transform.position = GridManager.Instance.CellToWorldConversion(cell);
        WallLogic logic = wallObj.AddComponent<WallLogic>();
        logic.Setup(activeBuilding, cell);
        
        activeBuildings.Add(cell, wallObj);
        return wallObj;
    }

    GameObject SpawnTurretLogic(Vector2Int cell)
    {
        GameObject turretObj = new GameObject("Turret_Logic_" + cell);
        turretObj.transform.position = GridManager.Instance.CellToWorldConversion(cell);
        TurretLogic logic = turretObj.AddComponent<TurretLogic>();
        logic.Setup(activeBuilding, cell);
        
        activeBuildings.Add(cell, turretObj);
        return turretObj;
    }
}