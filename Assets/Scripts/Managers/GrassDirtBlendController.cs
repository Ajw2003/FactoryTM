namespace Managers
{
    using Singleton;
    using TerrainBlending;
    using UnityEngine;
    using UnityEngine.Tilemaps;

    // Scene-side wiring for GrassDirtBlender: holds the base/blend tilemap references and the 5
    // blend tile assets, and exposes a regenerate-everything entry point (used by the Editor tool
    // in Assets/Scripts/Editor/GrassDirtBlendEditor.cs) plus an incremental entry point for future
    // runtime terrain edits (e.g. grass disappearing as resources are stripped).
    public class GrassDirtBlendController : SingletonBase<GrassDirtBlendController>
    {
        [Header("Tilemaps")]
        [Tooltip("Solid ground layer. A cell with a tile here counts as grass; an empty cell counts as dirt.")]
        public Tilemap baseTilemap;
        [Tooltip("Overlay layer the blend fringe pieces get painted onto.")]
        public Tilemap blendTilemap;

        [Header("Blend Pieces")]
        public GrassDirtBlendPieces pieces = new GrassDirtBlendPieces();

        GrassDirtBlender blender;

        protected override void Awake()
        {
            persistBetweenScenes = false;
            base.Awake();
        }

        bool EnsureBlender()
        {
            if (blender != null) return true;
            if (baseTilemap == null || blendTilemap == null) return false;
            blender = new GrassDirtBlender(baseTilemap, blendTilemap, pieces);
            return true;
        }

        public void RegenerateAll()
        {
            if (!EnsureBlender())
            {
                Debug.LogWarning("GrassDirtBlendController: baseTilemap/blendTilemap not assigned.");
                return;
            }
            blender.RecomputeRegion(baseTilemap.cellBounds);
        }

        // Recomputes the changed cell plus its neighbors, since a change can also affect the
        // blend piece shown on adjacent dirt cells.
        public void NotifyCellChanged(Vector3Int cell)
        {
            if (!EnsureBlender())
            {
                Debug.LogWarning("GrassDirtBlendController: baseTilemap/blendTilemap not assigned.");
                return;
            }
            BoundsInt area = new BoundsInt(cell.x - 1, cell.y - 1, cell.z, 3, 3, 1);
            blender.RecomputeRegion(area);
        }
    }
}
