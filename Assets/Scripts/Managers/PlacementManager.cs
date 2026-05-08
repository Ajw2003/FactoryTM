using UnityEngine;
using UnityEngine.Tilemaps;

public class PlacementManager : MonoBehaviour
{
    public Tilemap conveyorTilemap;
    public Tilemap previewTilemap;
    
    [Header("Inventory")]
    public BuildingData activeBuilding; 
    private int currentRotationIndex = 0; // 0-3
    
    private Camera mainCam;

    void Start() => mainCam = Camera.main;

    void Update()
    {
        if (activeBuilding == null) {
            previewTilemap.ClearAllTiles();
            return;
        }

        Vector3Int mouseCell = GetMouseCell();

        // 1. Handle Rotation (R)
        if (Input.GetKeyDown(KeyCode.R))
            currentRotationIndex = (currentRotationIndex + 1) % 4;

        // 2. Update Ghost Preview
        previewTilemap.ClearAllTiles();
        previewTilemap.SetTile(mouseCell, activeBuilding.rotatedTiles[currentRotationIndex]);

        // 3. Place Building
        if (Input.GetMouseButton(0))
            conveyorTilemap.SetTile(mouseCell, activeBuilding.rotatedTiles[currentRotationIndex]);

        // 4. Delete Building
        if (Input.GetMouseButton(1))
            conveyorTilemap.SetTile(mouseCell, null);
    }

    // Call this from your UI Buttons to change what you are building
    public void SetActiveBuilding(BuildingData newData)
    {
        activeBuilding = newData;
        currentRotationIndex = 0;
    }

    Vector3Int GetMouseCell()
    {
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cell = conveyorTilemap.WorldToCell(mouseWorldPos);
        cell.z = 0;
        return cell;
    }
}