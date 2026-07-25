using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
namespace Placeables
{
    using Code.Scripts.EventSystems;
    using EventTypes.BuildingEvents;
    using UnityEngine;
    
    public abstract class BuildingLogic : MonoBehaviour, IHealth
    {
        public Buildings.BuildingData data;
        protected Vector2Int myCell;
        protected int rotationIndex = 0;
        public System.Collections.Generic.List<Vector2Int> occupiedCells = new System.Collections.Generic.List<Vector2Int>();
        [SerializeField] private bool isEnemyOwned = false;
        public EnemyOutpost outpost;

        /// <summary>Whether this building currently belongs to an enemy outpost. Change it through <see cref="SetEnemyOwned"/>.</summary>
        public bool IsEnemyOwned => isEnemyOwned;

        // Subclasses set Health from their Setup overrides; external code must go through TakeDamage.
        public int Health { get; protected set; }

        public bool IsAlive => Health > 0;

        /// <summary>Transfers ownership of this building, optionally attaching it to the outpost that now owns it.</summary>
        public virtual void SetEnemyOwned(bool value, EnemyOutpost owningOutpost = null)
        {
            isEnemyOwned = value;
            outpost = owningOutpost;
        }

        private Sprite crackSprite;
        private SpriteRenderer crackRenderer;
        private static Sprite portArrowSprite;

        public virtual void Setup(Buildings.BuildingData buildingData, Vector2Int cell)
        {
            data = buildingData;
            myCell = cell;
            Health = data.maxHealth;
        }

        public virtual void SetOccupiedCells(System.Collections.Generic.List<Vector2Int> cells)
        {
            occupiedCells = cells;
        }

        public Vector2Int GetMyCell()
        {
            return myCell;
        }

        /// <summary>Whether this building can accept an item arriving from a conveyor moving in the given direction. Default: no input side.</summary>
        public virtual bool CanAcceptInputFrom(Vector2Int incomingDirection)
        {
            return false;
        }

        public Vector2Int GetFacingDirection()
        {
            return GameManager.Instance.GetDirectionFromRotationIndex(rotationIndex);
        }

        /// <summary>Footprint cells on this building's single input side (opposite its facing/output direction). Empty if this building has no input side.</summary>
        public System.Collections.Generic.List<Vector2Int> GetInputCells()
        {
            return GetEdgeCells(-GetFacingDirection());
        }

        /// <summary>Footprint cells on this building's single output side (its facing direction).</summary>
        public System.Collections.Generic.List<Vector2Int> GetOutputCells()
        {
            return GetEdgeCells(GetFacingDirection());
        }

        /// <summary>Building footprint size with width/height swapped for a 90/270 degree rotation.</summary>
        protected Vector2Int GetActualSize()
        {
            if (data == null) return Vector2Int.one;
            return rotationIndex % 2 != 0 ? new Vector2Int(data.size.y, data.size.x) : data.size;
        }

        /// <summary>All cells in this building's footprint that lie on the given outward-facing edge.</summary>
        protected System.Collections.Generic.List<Vector2Int> GetEdgeCells(Vector2Int outwardDir)
        {
            Vector2Int actualSize = GetActualSize();
            var result = new System.Collections.Generic.List<Vector2Int>();
            foreach (var cell in occupiedCells)
            {
                Vector2Int local = cell - myCell;
                bool onEdge =
                    outwardDir.x > 0 ? local.x == actualSize.x - 1 :
                    outwardDir.x < 0 ? local.x == 0 :
                    outwardDir.y > 0 ? local.y == actualSize.y - 1 :
                    outwardDir.y < 0 ? local.y == 0 : false;
                if (onEdge) result.Add(cell);
            }
            return result;
        }

        /// <summary>The neighboring cell just outside the footprint in the given direction, from the origin cell.</summary>
        protected Vector2Int GetNeighborCellInDirection(Vector2Int direction)
        {
            Vector2Int actualSize = GetActualSize();
            Vector2Int offset = Vector2Int.zero;
            if (direction.x > 0) offset = new Vector2Int(actualSize.x, 0);
            else if (direction.x < 0) offset = new Vector2Int(-1, 0);
            else if (direction.y > 0) offset = new Vector2Int(0, actualSize.y);
            else if (direction.y < 0) offset = new Vector2Int(0, -1);
            return myCell + offset;
        }

        /// <summary>World position aligned to the entry boundary of targetCell for an item traveling in the given direction.</summary>
        protected Vector2 GetEdgeSpawnPosition(Vector2Int targetCell, Vector2Int direction)
        {
            Vector2 tileSize = GridManager.Instance.TileSize;
            if (direction.x > 0) return new Vector2(targetCell.x * tileSize.x, (targetCell.y + 0.5f) * tileSize.y);
            if (direction.x < 0) return new Vector2((targetCell.x + 1) * tileSize.x, (targetCell.y + 0.5f) * tileSize.y);
            if (direction.y > 0) return new Vector2((targetCell.x + 0.5f) * tileSize.x, targetCell.y * tileSize.y);
            return new Vector2((targetCell.x + 0.5f) * tileSize.x, (targetCell.y + 1) * tileSize.y);
        }

        /// <summary>Combines GetNeighborCellInDirection and GetEdgeSpawnPosition for the common "spawn an output item" case.</summary>
        protected Vector2 GetOutputSpawnPosition(Vector2Int direction, out Vector2Int targetCell)
        {
            targetCell = GetNeighborCellInDirection(direction);
            return GetEdgeSpawnPosition(targetCell, direction);
        }

        /// <summary>Computes a rectangular footprint of occupiedCells from myCell using the current rotation-adjusted size.</summary>
        protected System.Collections.Generic.List<Vector2Int> ComputeFootprintCells()
        {
            Vector2Int actualSize = GetActualSize();
            var cells = new System.Collections.Generic.List<Vector2Int>();
            for (int x = 0; x < actualSize.x; x++)
            {
                for (int y = 0; y < actualSize.y; y++)
                {
                    cells.Add(myCell + new Vector2Int(x, y));
                }
            }
            return cells;
        }

        private static Sprite GetPortArrowSprite()
        {
            if (portArrowSprite != null) return portArrowSprite;

            int size = 16;
            Texture2D tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Point;
            Color clear = new Color(0f, 0f, 0f, 0f);
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tex.SetPixel(x, y, clear);

            // Solid triangle, apex at the top, so the sprite reads as an upward-pointing arrow at rotation 0.
            for (int y = 0; y < size; y++)
            {
                float t = y / (float)(size - 1);
                int halfWidth = Mathf.RoundToInt((1f - t) * (size / 2f));
                int center = size / 2;
                for (int x = center - halfWidth; x <= center + halfWidth; x++)
                {
                    if (x >= 0 && x < size) tex.SetPixel(x, y, Color.white);
                }
            }
            tex.Apply();

            portArrowSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
            return portArrowSprite;
        }

        /// <summary>Spawns a small world-space arrow indicator at the outward edge of edgeCell, rotated to point in arrowPointDir.</summary>
        protected void CreatePortIndicator(Vector2Int edgeCell, Vector2Int edgeOutwardDir, Vector2Int arrowPointDir, Color tint, string label)
        {
            GameObject go = new GameObject("PortIndicator_" + label);
            go.transform.SetParent(transform, false);

            Vector2 cellCenter = GridManager.Instance.CellToWorldConversion(edgeCell);
            Vector2 tileSize = GridManager.Instance.TileSize;
            Vector2 edgeOffset = new Vector2(edgeOutwardDir.x * tileSize.x * 0.5f, edgeOutwardDir.y * tileSize.y * 0.5f);
            go.transform.position = new Vector3(cellCenter.x + edgeOffset.x, cellCenter.y + edgeOffset.y, -0.05f);

            float angle = 0f;
            if (arrowPointDir.x > 0) angle = -90f;
            else if (arrowPointDir.x < 0) angle = 90f;
            else if (arrowPointDir.y < 0) angle = 180f;
            go.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            go.transform.localScale = Vector3.one * 0.4f;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetPortArrowSprite();
            sr.color = tint;
            sr.sortingLayerName = "Buildings";
            sr.sortingOrder = 15;
        }
    
        protected virtual void OnEnable()
        {
            if (BuildingManager.Instance != null)
                BuildingManager.Instance.RegisterBuilding(this);
        }
    
        protected virtual void OnDisable()
        {
            if (BuildingManager.HasInstance)
                BuildingManager.Instance.UnregisterBuilding(this);
        }
    
        public abstract void PerformAction();
    
        public virtual void TakeDamage(int amount)
        {
            Health -= amount;
    
            // Spawn floating damage text!
            string settingsPath = isEnemyOwned ? "FloatingTextSettings/BuildingDamageEnemySettings" : "FloatingTextSettings/BuildingDamagePlayerSettings";
            FloatingTextSettings settings = Resources.Load<FloatingTextSettings>(settingsPath);
            FloatingTextManager.Instance.Spawn(amount.ToString(), transform.position, settings);
    
            // Update visual crack overlay
            UpdateCrackVisuals();
            
            // if (PlacementManager.HasInstance)
            // {
            //     StartCoroutine(FlashRedTile());
            // }
    
            if (BuildingManager.HasInstance && !isEnemyOwned)
            {
                BuildingManager.Instance.NotifyBuildingDamaged();
            }

            EventManager.Instance?.Publish(new BuildingDamagedEvent(this, Health, data?.maxHealth ?? Health));

            if (Health <= 0)
            {
                Die();
            }
        }
    
        // private System.Collections.IEnumerator FlashRedTile()
        // {
        //     Vector3Int pos3 = new Vector3Int(myCell.x, myCell.y, 0);
        //     UnityEngine.Tilemaps.Tilemap map = PlacementManager.Instance.mainTilemap;
        //     
        //     // Ensure the tile can be tinted
        //     map.SetTileFlags(pos3, UnityEngine.Tilemaps.TileFlags.None);
        //     
        //     Color originalColor = map.GetColor(pos3);
        //     map.SetColor(pos3, new Color(1f, 0.3f, 0.3f, 1f));
        //     yield return new WaitForSeconds(0.12f);
        //     map.SetColor(pos3, originalColor);
        // }
    
        public virtual void Die()
        {
            if (isEnemyOwned)
            {
                int reward = Mathf.RoundToInt(data != null ? data.cost * 1.5f : 50f);
                if (CurrencyManager.Instance != null)
                {
                    CurrencyManager.Instance.AddCurrency(reward);
                    FloatingTextSettings settings = Resources.Load<FloatingTextSettings>("FloatingTextSettings/BuildingDestructionRewardSettings");
                    FloatingTextManager.Instance.Spawn(reward.ToString(), transform.position, settings);
                }
                if (outpost != null)
                {
                    outpost.RemoveBuilding(this);
                }
            }
    
            if (PlacementManager.HasInstance)
            {
                PlacementManager.Instance.DestroyBuilding(myCell, false);
            }

            EventManager.Instance?.Publish(new BuildingDiedEvent(this));
        }
    
        public float GetTierMultiplier()
        {
            if (Managers.UpgradeManager.HasInstance)
            {
                int tier = Managers.UpgradeManager.Instance.GetBuildingTier(data.type);
                // Tier 1: 1.0f, Tier 2: 1.25f, Tier 3: 1.50f, Tier 4: 2.0f
                switch (tier)
                {
                    case 1: return 1.0f;
                    case 2: return 1.25f;
                    case 3: return 1.50f;
                    case 4: return 2.0f;
                    default: return 1.0f;
                }
            }
            return 1.0f;
        }
    
        private void UpdateCrackVisuals()
        {
            if (data == null || data.maxHealth <= 0) return;
            
            float healthPct = (float)Health / data.maxHealth;
            
            if (healthPct >= 0.99f)
            {
                if (crackRenderer != null)
                {
                    crackRenderer.gameObject.SetActive(false);
                }
                return;
            }
    
            if (crackRenderer == null)
            {
                GameObject crackObj = new GameObject("CrackOverlay");
                crackObj.transform.SetParent(transform);
                Vector2 sizeOffset = new Vector2(data.size.x - 1, data.size.y - 1) * 0.5f;
                crackObj.transform.localPosition = new Vector3(sizeOffset.x, sizeOffset.y, -0.1f);
                
                crackRenderer = crackObj.AddComponent<SpriteRenderer>();
                
                if (crackSprite == null)
                {
                    Texture2D crackTex = GenerateCrackTexture();
                    crackSprite = Sprite.Create(crackTex, new Rect(0, 0, crackTex.width, crackTex.height), new Vector2(0.5f, 0.5f), 64f);
                }
                crackRenderer.sprite = crackSprite;
                crackRenderer.sortingLayerName = "Buildings";
                crackRenderer.sortingOrder = 10;
            }
    
            crackRenderer.gameObject.SetActive(true);
            
            float damagePct = 1f - healthPct;
            Color c = crackRenderer.color;
            c.a = Mathf.Clamp(damagePct * 1.6f, 0.45f, 1.0f);
            crackRenderer.color = c;
    
            float scale = Mathf.Lerp(0.5f, 1.0f, damagePct);
            crackRenderer.transform.localScale = new Vector3(data.size.x * scale, data.size.y * scale, 1f);
        }
    
        private Texture2D GenerateCrackTexture()
        {
            int size = 64;
            Texture2D texture = new Texture2D(size, size);
            texture.filterMode = FilterMode.Point;
            
            Color transparent = new Color(0, 0, 0, 0);
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    texture.SetPixel(x, y, transparent);
                }
            }
            
            Color crackColor = new Color(0.02f, 0.02f, 0.02f, 0.95f);
            
            // Draw crack lines spanning further towards the edges
            DrawCrackLine(texture, size / 2, size / 2, size / 2 + Random.Range(-26, 26), size / 2 + Random.Range(-26, 26), crackColor);
            DrawCrackLine(texture, size / 2, size / 2, size / 2 + Random.Range(-26, 26), size / 2 - Random.Range(-26, 26), crackColor);
            DrawCrackLine(texture, size / 2, size / 2, size / 2 - Random.Range(-26, 26), size / 2 + Random.Range(-26, 26), crackColor);
            DrawCrackLine(texture, size / 2, size / 2, size / 2 - Random.Range(-26, 26), size / 2 - Random.Range(-26, 26), crackColor);
            
            texture.Apply();
            return texture;
        }
    
        private void DrawCrackLine(Texture2D tex, int x0, int y0, int x1, int y1, Color color)
        {
            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;
    
            while (true)
            {
                // Set thick pixel brush (cross shape) for high definition
                SetThickPixel(tex, x0, y0, color);
                
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }
    
        private void SetThickPixel(Texture2D tex, int x, int y, Color color)
        {
            tex.SetPixel(x, y, color);
            if (x > 0) tex.SetPixel(x - 1, y, color);
            if (x < tex.width - 1) tex.SetPixel(x + 1, y, color);
            if (y > 0) tex.SetPixel(x, y - 1, color);
            if (y < tex.height - 1) tex.SetPixel(x, y + 1, color);
        }
    
        protected virtual void OnDestroy()
        {
            if (crackSprite != null)
            {
                if (crackSprite.texture != null)
                {
                    Destroy(crackSprite.texture);
                }
                Destroy(crackSprite);
            }
        }
    }
    
}


