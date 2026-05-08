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
        if (Input.GetKeyDown(KeyCode.R)) rotationIndex = (rotationIndex + 1) % 4;

        // Preview
        previewTilemap.ClearAllTiles();
        previewTilemap.SetTile(cell, activeBuilding.rotatedTiles[rotationIndex]);

        // Place
        if (Input.GetMouseButton(0))
            mainTilemap.SetTile(cell, activeBuilding.rotatedTiles[rotationIndex]);

        // Delete
        if (Input.GetMouseButton(1))
            mainTilemap.SetTile(cell, null);
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
}