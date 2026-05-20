using System;
using Singleton;
using UnityEngine;

public class GridManager : SingletonBase<GridManager>
{
    public Vector2Int center = new Vector2Int(0, 0);

    public Vector2 tileSize =new (1,1);
    public Vector2Int gridSize;

    public Vector2 CellToWorldConversion(Vector2Int cellPosition)
    {
        return new Vector2(
            (cellPosition.x * tileSize.x) + (tileSize.x / 2f),
            (cellPosition.y * tileSize.y) + (tileSize.y / 2f)
        );// multiply by tile size then add to tile size/2 to get corner of Tile
    }
    
    public Vector2Int WorldToCellConversion(Vector2 worldPosition)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPosition.x / tileSize.x), 
            Mathf.FloorToInt(worldPosition.y / tileSize.y)
        );//convert vector2 to vector2Int and use floortoInt to prevent negative numbers erroring
    }
    
    

    public void Start()
    {
        tileSize = GameManager.Instance.buildingTilemap.cellSize;
        var sizex = GameManager.Instance.buildingTilemap.size.x;
        var sizey = GameManager.Instance.buildingTilemap.size.y;
        gridSize = new Vector2Int(sizex, sizey);
    }
}
