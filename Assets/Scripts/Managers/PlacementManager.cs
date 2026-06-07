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
                    Transform t = gridObj.transform.Find("BuildingTilemap");
                    if (t != null) mainTilemap = t.GetComponent<Tilemap>();
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

        // Preview
        previewTilemap.ClearAllTiles();
        if (ZoneManager.Instance.IsTileInsideUnlockedZone(vector3Cell) && isWithinDistance)
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

            if (!ZoneManager.Instance.IsTileInsideUnlockedZone(vector3Cell))
            {
                Debug.Log("Cannot place outside unlocked zone!");
                return;
            }

            // Task 1: Check Inventory instead of Currency
            if (!InventoryManager.Instance.HasBuilding(activeBuilding))
            {
                Debug.Log("Don't have " + activeBuilding.buildingName + " in inventory!");
                return;
            }
            
            // Check if cell is occupied
            if (activeBuildings.ContainsKey(cell))
            {
                Debug.Log("Cell occupied!");
                return;
            }

            mainTilemap.SetTile(vector3Cell, activeBuilding.rotatedTiles[rotationIndex]);
            switch (activeBuilding.type)
            {
                case (BuildingType.Chest):
                    break;
                case (BuildingType.Conveyor):
                    SpawnBeltLogic(cell);
                    break;
                case(BuildingType.Miner):
                    SpawnMinerLogic(cell);
                    break;
                case (BuildingType.Seller):
                    SpawnSellerLogic(cell);
                    break;
                case (BuildingType.Furnace):
                    SpawnFurnaceLogic(cell);   
                    break;
            }
            // Task 1: Consume from Inventory
            InventoryManager.Instance.RemoveBuilding(activeBuilding);
        }
            

        // Delete
        if (Input.GetMouseButtonDown(1))
        {
            if (activeBuildings.ContainsKey(cell))
            {
                GameObject buildingObj = activeBuildings[cell];
                BuildingLogic logic = buildingObj.GetComponent<BuildingLogic>();
                
                if (logic != null && logic.data != null)
                {
                    // Task 1: Return to Inventory instead of refunding currency
                    InventoryManager.Instance.AddBuilding(logic.data);
                }

                mainTilemap.SetTile((vector3Cell), null);
                Destroy(buildingObj);
                activeBuildings.Remove(cell);
            }
        }
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
    
    private Dictionary<Vector2Int, GameObject> activeBuildings = new Dictionary<Vector2Int, GameObject>();

    void SpawnMinerLogic(Vector2Int cell)
    {
        // Create the logic object
        GameObject MinerObj = new GameObject("Miner_Logic_" + cell);
        MinerObj.transform.position = GridManager.Instance.CellToWorldConversion(cell);
        MinerLogic logic = MinerObj.AddComponent<MinerLogic>();
        logic.Setup(activeBuilding, cell, rotationIndex);

        // Store it so we can delete it later if needed
        activeBuildings.Add(cell, MinerObj);
    }

    void SpawnFurnaceLogic(Vector2Int cell)
    {
        GameObject FurnaceObj = new GameObject("Furnace_Logic_" + cell);
        FurnaceObj.transform.position = GridManager.Instance.CellToWorldConversion(cell);
        Furnace furnace = FurnaceObj.AddComponent<Furnace>();
        furnace.Setup(activeBuilding, cell, rotationIndex, activeBuilding.proccessingSpeed);
        activeBuildings.Add(cell, FurnaceObj);
    }

    void SpawnSellerLogic(Vector2Int cell)
    {
        GameObject sellerObj = new GameObject("Seller_Logic_" + cell);
        sellerObj.transform.position = GridManager.Instance.CellToWorldConversion(cell);
        Seller seller = sellerObj.AddComponent<Seller>();
        seller.Setup(activeBuilding, cell);
        activeBuildings.Add(cell, sellerObj);
    }

    void SpawnBeltLogic(Vector2Int cell)
    {
        GameObject beltObj = new GameObject("Belt_Logic_" + cell);
        beltObj.transform.position = GridManager.Instance.CellToWorldConversion(cell);
        
        ConveyorLogic logic = beltObj.AddComponent<ConveyorLogic>();
        Vector2Int dir = GameManager.Instance.GetDirectionFromRotationIndex(rotationIndex);
        logic.Setup(activeBuilding, cell, dir, activeBuilding.proccessingSpeed); // Pass data
        
        activeBuildings.Add(cell, beltObj);
    }
}