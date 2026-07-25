using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
namespace Managers
{
    using System;
    using Singleton;
    using UnityEngine;
    
    public class GridManager : SingletonBase<GridManager>
    {
        [SerializeField] private Vector2Int center = new Vector2Int(0, 0);

        [SerializeField] private Vector2 tileSize = new(1, 1);
        [SerializeField] private Vector2Int gridSize;

        /// <summary>Cell coordinate the grid is centred on (the starting zone's middle).</summary>
        public Vector2Int Center => center;
        /// <summary>World-space size of a single grid cell. Read every frame by placement and conveyor math.</summary>
        public Vector2 TileSize => tileSize;
        public Vector2Int GridSize => gridSize;


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
    
            // Dynamically center on the center of the starting zone (0, 0) if ZoneManager is present
            if (ZoneManager.Instance != null)
            {
                Vector2Int zoneSize = ZoneManager.Instance.ZoneSizeInTiles;
                center = new Vector2Int(zoneSize.x / 2, zoneSize.y / 2);
            }
        }
    }
    
}


