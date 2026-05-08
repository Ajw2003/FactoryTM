using System.Collections.Generic;
using Buildings;
using UnityEngine;
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

        Vector3Int cell = GetMouseCell();
        
        // Rotate (R)
        if (Input.GetKeyDown(KeyCode.R)) rotationIndex = (rotationIndex + 1) % 4;// set the rotation index i.e. right,left,up,down

        // Preview
        previewTilemap.ClearAllTiles();
        previewTilemap.SetTile(cell, activeBuilding.rotatedTiles[rotationIndex]);

        // Place
        if (Input.GetMouseButtonDown(0))
        {
            if(CurrencyManager.Instance.currentCurrencyValue < activeBuilding.cost)
                Debug.Log("cost too high");
            
            mainTilemap.SetTile(cell, activeBuilding.rotatedTiles[rotationIndex]);
            switch (activeBuilding.type)
            {
                case (BuildingType.Chest):
                    break;
                case (BuildingType.Conveyor):
                    break;
                case(BuildingType.Miner):
                    SpawnMinerLogic(cell);
                    break;
                case (BuildingType.Seller):
                    SpawnSellerLogic(cell);
                    break;
            }
        }
            

        // Delete
        if (Input.GetMouseButton(1))
        {
            mainTilemap.SetTile(cell, null);

            if (activeMiners.ContainsKey(cell))
            {
                Destroy(activeMiners[cell]);
                activeMiners.Remove(cell);
            }
        }
    }

    public void ChangeSelection(BuildingData newBuilding)
    {
        activeBuilding = newBuilding;
        rotationIndex = 0;
    }

    Vector3Int GetMouseCell()
    {
        Vector3 p = cam.ScreenToWorldPoint(Input.mousePosition);
        return mainTilemap.WorldToCell(new Vector3(p.x, p.y, 0));
    }
    
    private Dictionary<Vector3Int, GameObject> activeMiners = new Dictionary<Vector3Int, GameObject>();

    void SpawnMinerLogic(Vector3Int cell)
    {
        // Create the logic object
        GameObject MinerObj = new GameObject("Miner_Logic_" + cell);
        MinerObj.transform.position = mainTilemap.GetCellCenterWorld(cell);
    
        MinerLogic logic = MinerObj.AddComponent<MinerLogic>();
        logic.Setup(activeBuilding, cell, rotationIndex);

        // Store it so we can delete it later if needed
        activeMiners.Add(cell, MinerObj);
    }

    void SpawnSellerLogic(Vector3Int cell)
    {
        GameObject sellerObj = new GameObject("Seller_Logic_" + cell);
        sellerObj.transform.position = mainTilemap.GetCellCenterWorld(cell);
        Seller seller = sellerObj.AddComponent<Seller>();
        
    }
}