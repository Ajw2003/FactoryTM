using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ConveyorItem : MonoBehaviour
{
    public Tilemap beltTilemap;
    public float speed = 2f;
    public int value;

    void Update()
    {
        Vector3Int currentCell = beltTilemap.WorldToCell(transform.position);
        TileBase tile = beltTilemap.GetTile(currentCell);

        // Ask the Manager for the direction
        Vector3Int moveDir = GameManager.Instance.GetDirectionFromTile(tile);

        if (moveDir != Vector3Int.zero)
        {
            Vector3 cellCenter = beltTilemap.GetCellCenterWorld(currentCell);// get center of cell placed on
            Vector3 targetPos = beltTilemap.GetCellCenterWorld(currentCell + moveDir);// set move direction

            // Move
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);// actually move

            // Auto-Centering logic
            if (moveDir.x != 0) // Nudge Y to center
                transform.position = new Vector3(transform.position.x, Mathf.MoveTowards(transform.position.y, cellCenter.y, speed * Time.deltaTime), 0);
            else if (moveDir.y != 0) // Nudge X to center
                transform.position = new Vector3(Mathf.MoveTowards(transform.position.x, cellCenter.x, speed * Time.deltaTime), transform.position.y, 0);
        }
    }

    private void Start()
    {
        beltTilemap = GameManager.Instance.buildingTilemap;
    }
}