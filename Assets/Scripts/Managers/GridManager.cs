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
    using System;
    using Singleton;
    using UnityEngine;
    
    public class GridManager : SingletonBase<GridManager>
    {
        public Vector2Int center = new Vector2Int(0, 0);
    
        public Vector2 tileSize =new (1,1);
        public Vector2Int gridSize;
    
        protected override void Awake()
        {
            persistBetweenScenes = false;
            base.Awake();
        }
    
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
        
        
    
        [Header("Grid Configuration")]
        public Vector2Int baseChunkSize = new Vector2Int(30, 15);
        
        public void Start()
        {
            if (GameManager.Instance != null && GameManager.Instance.MainTileMap != null)
            {
                tileSize = GameManager.Instance.MainTileMap.cellSize;
                var sizex = GameManager.Instance.MainTileMap.size.x;
                var sizey = GameManager.Instance.MainTileMap.size.y;
                gridSize = new Vector2Int(sizex, sizey);
            }
            else
            {
                Debug.LogWarning("GridManager: GameManager or MainTileMap is null in Start.");
            }
    
            // Set center to the middle of the initial chunk area
            center = new Vector2Int(baseChunkSize.x / 2, baseChunkSize.y / 2);
        }
    }
    
}


