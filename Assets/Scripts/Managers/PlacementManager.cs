using System.Collections.Generic;
using Buildings;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class PlacementManager : MonoBehaviour
{
    public Tilemap mainTilemap;
    public Tilemap previewTilemap;
    
    private BuildingData activeBuilding;
    private int rotationIndex = 0;
    private Camera cam;

    void Start() => cam = Camera.main;

    void Update()
    {
        if (activeBuilding == null) return;

        Vector2Int cell = GetMouseCell();
        Vector3Int vector3Cell = new Vector3Int(cell.x, cell.y, 0);
        
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // If it is, clear the ghost preview and stop here
            previewTilemap.ClearAllTiles();
            return; 
        }
        
        // Rotate (R)
        if (Input.GetKeyDown(KeyCode.R)) rotationIndex = (rotationIndex + 1) % 4;// set the rotation index i.e. right,left,up,down

        // Preview
        previewTilemap.ClearAllTiles();
        if (ZoneManager.Instance.IsTileInsideUnlockedZone(vector3Cell))
        {
            previewTilemap.SetTile(vector3Cell, activeBuilding.rotatedTiles[rotationIndex]);
        }

        // Place
        if (Input.GetMouseButtonDown(0))
        {
            if (!ZoneManager.Instance.IsTileInsideUnlockedZone(vector3Cell))
            {
                Debug.Log("Cannot place outside unlocked zone!");
                return;
            }

            if (CurrencyManager.Instance.currentCurrencyValue < activeBuilding.cost)
            {
                Debug.Log("cost too high");
                return;
            }
            
            // Check if cell is occupied
            if (activeBuildings.ContainsKey(cell))
            {
                // Optionally: Auto-delete the old one? Or just return?
                // For now, let's just return to prevent overlapping.
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
            CurrencyManager.Instance.RemoveCurrency(activeBuilding.cost);
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
                    CurrencyManager.Instance.AddCurrency(logic.data.cost);
                }

                int temp = 0;
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