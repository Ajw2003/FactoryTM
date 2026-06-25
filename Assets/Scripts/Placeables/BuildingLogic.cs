using UnityEngine;

public abstract class BuildingLogic : MonoBehaviour, IHealth
{
    public Buildings.BuildingData data;
    protected Vector2Int myCell;
    public System.Collections.Generic.List<Vector2Int> occupiedCells = new System.Collections.Generic.List<Vector2Int>();
    public bool isEnemyOwned = false;
    public EnemyOutpost outpost;
    
    public int Health { get; set; }

    private Sprite crackSprite;
    private SpriteRenderer crackRenderer;

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
        GameObject textObj = new GameObject("DamageNumber");
        FloatingDamageText floatText = textObj.AddComponent<FloatingDamageText>();
        Color textColor = isEnemyOwned ? new Color(1f, 0.7f, 0.2f) : Color.red; // Orange/yellow for enemy, red for player
        floatText.Initialize(amount.ToString(), textColor, transform.position + new Vector3(0, 0.5f, 0));

        // Update visual crack overlay
        UpdateCrackVisuals();
        
        if (PlacementManager.HasInstance)
        {
            StartCoroutine(FlashRedTile());
        }

        if (BuildingManager.HasInstance && !isEnemyOwned)
        {
            BuildingManager.Instance.NotifyBuildingDamaged();
        }

        if (Health <= 0)
        {
            Die();
        }
    }

    public void ChangeHealth(int amount, int previous)
    {
        
    }

    private System.Collections.IEnumerator FlashRedTile()
    {
        Vector3Int pos3 = new Vector3Int(myCell.x, myCell.y, 0);
        UnityEngine.Tilemaps.Tilemap map = PlacementManager.Instance.mainTilemap;
        
        // Ensure the tile can be tinted
        map.SetTileFlags(pos3, UnityEngine.Tilemaps.TileFlags.None);
        
        Color originalColor = map.GetColor(pos3);
        map.SetColor(pos3, new Color(1f, 0.3f, 0.3f, 1f));
        yield return new WaitForSeconds(0.12f);
        map.SetColor(pos3, originalColor);
    }

    public virtual void Die()
    {
        if (isEnemyOwned)
        {
            int reward = Mathf.RoundToInt(data != null ? data.cost * 1.5f : 50f);
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddCurrency(reward);
                GameObject textObj = new GameObject("DamageNumber");
                FloatingDamageText floatText = textObj.AddComponent<FloatingDamageText>();
                floatText.Initialize(reward.ToString(), Color.forestGreen, transform.position + new Vector3(0, 0.5f, 0));
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
        c.a = Mathf.Clamp(damagePct * 1.2f, 0.2f, 1f);
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
        
        Color crackColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        // Draw crack lines stemming from center
        DrawCrackLine(texture, size / 2, size / 2, size / 2 + Random.Range(-15, 15), size / 2 + Random.Range(-15, 15), crackColor);
        DrawCrackLine(texture, size / 2, size / 2, size / 2 + Random.Range(-15, 15), size / 2 - Random.Range(-15, 15), crackColor);
        DrawCrackLine(texture, size / 2, size / 2, size / 2 - Random.Range(-15, 15), size / 2 + Random.Range(-15, 15), crackColor);
        DrawCrackLine(texture, size / 2, size / 2, size / 2 - Random.Range(-15, 15), size / 2 - Random.Range(-15, 15), crackColor);
        
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
            tex.SetPixel(x0, y0, color);
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
