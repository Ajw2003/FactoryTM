using UnityEngine;
using UnityEngine.Tilemaps;

namespace TerrainBlending
{
    // The 5 blend tile assets actually in use on the "Blending" overlay tilemap, plus a per-piece
    // rotation offset used to calibrate each art asset's canonical orientation once verified in-editor.
    [System.Serializable]
    public class GrassDirtBlendPieces
    {
        [Header("1 grass neighbor (straight edge) - alternates for variety")]
        public TileBase edgeVariantA; // GrassTopOuter 1
        public TileBase edgeVariantB; // GrassRightInner
        public int edgeVariantARotationOffset;
        public int edgeVariantBRotationOffset;

        [Header("2 adjacent grass neighbors (concave corner)")]
        public TileBase cornerInner; // GrassCornerInner
        public int cornerInnerRotationOffset;

        [Header("2 opposite grass neighbors (corridor)")]
        public TileBase cornerHalf; // GrassCornerHalf
        public int cornerHalfRotationOffset;

        [Header("3 grass neighbors (peninsula tip)")]
        public TileBase cornerFull; // GrassCornerFull
        public int cornerFullRotationOffset;
    }

    // Cross-tilemap auto-tiler: reads terrain state from a base tilemap (tile present = grass,
    // empty = dirt) and paints the correct one of the 5 blend pieces - rotated to match - onto a
    // separate overlay tilemap. Unity's own RuleTile can't do this because it only ever inspects
    // neighbor cells on the tilemap it is painted on.
    public class GrassDirtBlender
    {
        readonly Tilemap baseTilemap;
        readonly Tilemap blendTilemap;
        readonly GrassDirtBlendPieces pieces;

        public GrassDirtBlender(Tilemap baseTilemap, Tilemap blendTilemap, GrassDirtBlendPieces pieces)
        {
            this.baseTilemap = baseTilemap;
            this.blendTilemap = blendTilemap;
            this.pieces = pieces;
        }

        public bool IsGrass(Vector3Int cell) => baseTilemap.HasTile(cell);

        public void RecomputeRegion(BoundsInt area)
        {
            foreach (Vector3Int cell in area.allPositionsWithin)
            {
                RecomputeCell(cell);
            }
        }

        public void RecomputeCell(Vector3Int cell)
        {
            if (IsGrass(cell))
            {
                blendTilemap.SetTile(cell, null);
                return;
            }

            bool north = IsGrass(cell + Vector3Int.up);
            bool south = IsGrass(cell + Vector3Int.down);
            bool east = IsGrass(cell + Vector3Int.right);
            bool west = IsGrass(cell + Vector3Int.left);
            int grassCount = (north ? 1 : 0) + (south ? 1 : 0) + (east ? 1 : 0) + (west ? 1 : 0);

            TileBase tile;
            int rotationDegrees;

            switch (grassCount)
            {
                case 1 when north:
                    (tile, rotationDegrees) = PickEdge(cell, 0);
                    break;
                case 1 when west:
                    (tile, rotationDegrees) = PickEdge(cell, 90);
                    break;
                case 1 when south:
                    (tile, rotationDegrees) = PickEdge(cell, 180);
                    break;
                case 1: // east
                    (tile, rotationDegrees) = PickEdge(cell, 270);
                    break;

                case 2 when east && west:
                    tile = pieces.cornerHalf;
                    rotationDegrees = 0 + pieces.cornerHalfRotationOffset; // horizontal corridor
                    break;
                case 2 when north && south:
                    tile = pieces.cornerHalf;
                    rotationDegrees = 90 + pieces.cornerHalfRotationOffset; // vertical corridor
                    break;
                case 2 when north && west:
                    tile = pieces.cornerInner;
                    rotationDegrees = 0 + pieces.cornerInnerRotationOffset;
                    break;
                case 2 when south && west:
                    tile = pieces.cornerInner;
                    rotationDegrees = 90 + pieces.cornerInnerRotationOffset;
                    break;
                case 2 when south && east:
                    tile = pieces.cornerInner;
                    rotationDegrees = 180 + pieces.cornerInnerRotationOffset;
                    break;
                case 2: // north && east
                    tile = pieces.cornerInner;
                    rotationDegrees = 270 + pieces.cornerInnerRotationOffset;
                    break;

                // Dirt continues on exactly one side - rotate so the peninsula opens that way.
                case 3 when !south:
                    tile = pieces.cornerFull;
                    rotationDegrees = 0 + pieces.cornerFullRotationOffset;
                    break;
                case 3 when !east:
                    tile = pieces.cornerFull;
                    rotationDegrees = 90 + pieces.cornerFullRotationOffset;
                    break;
                case 3 when !north:
                    tile = pieces.cornerFull;
                    rotationDegrees = 180 + pieces.cornerFullRotationOffset;
                    break;
                case 3: // !west
                    tile = pieces.cornerFull;
                    rotationDegrees = 270 + pieces.cornerFullRotationOffset;
                    break;

                // 0 (fully interior dirt) or 4 (isolated dirt speck, no art for this case today).
                default:
                    tile = null;
                    rotationDegrees = 0;
                    break;
            }

            blendTilemap.SetTile(cell, tile);
            if (tile != null)
            {
                blendTilemap.SetTransformMatrix(cell, Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, rotationDegrees), Vector3.one));
            }
        }

        // Alternates deterministically by cell position (not call order) so the same cell always
        // resolves to the same variant regardless of how/when it gets recomputed.
        (TileBase tile, int rotationDegrees) PickEdge(Vector3Int cell, int baseRotation)
        {
            bool useVariantA = ((cell.x + cell.y) & 1) == 0;
            return useVariantA
                ? (pieces.edgeVariantA, baseRotation + pieces.edgeVariantARotationOffset)
                : (pieces.edgeVariantB, baseRotation + pieces.edgeVariantBRotationOffset);
        }
    }
}
