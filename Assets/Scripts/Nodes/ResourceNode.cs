using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
namespace Nodes
{
    using UnityEngine;
    using UnityEngine.Tilemaps;

    public class ResourceNode : MonoBehaviour
    {
        public GameObject minedItemPrefab;
        public float miningSpeed = 1f; // Items per second
        public int oreCount;
        public Vector2Int myCell { get; private set; }
        bool isSetup;

        // Visually the node is tile-painted (ground tile + blend fringe), not the sprite - the
        // sprite stays only for whatever still reads it (e.g. mining raycasts), made fully transparent.
        public void Setup(Vector2Int cell, TileBase groundTile)
        {
            myCell = cell;
            isSetup = true;
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.RegisterNode(myCell, this);
            }

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 0f;
                sr.color = c;
            }

            Vector3Int cell3 = new Vector3Int(cell.x, cell.y, 0);
            GameManager gm = GameManager.Instance;
            if (gm != null && gm.OreTileMap != null && groundTile != null)
            {
                gm.OreTileMap.SetTile(cell3, groundTile);
            }

            GrassDirtBlendController blend = GrassDirtBlendController.Instance;
            if (blend != null && blend.baseTilemap != null)
            {
                // Ore always reads as dirt for blending purposes, even if it spawned on a grass cell.
                blend.baseTilemap.SetTile(cell3, null);
                blend.NotifyCellChanged(cell3);
            }
        }

        private void OnDestroy()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.DeregisterNode(myCell);
            }

            if (!isSetup) return;

            Vector3Int cell3 = new Vector3Int(myCell.x, myCell.y, 0);
            GameManager gm = GameManager.Instance;
            if (gm != null && gm.OreTileMap != null)
            {
                gm.OreTileMap.SetTile(cell3, null);
            }

            GrassDirtBlendController.Instance?.NotifyCellChanged(cell3);
        }
    }

}


