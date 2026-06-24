using UnityEngine;

public abstract class BuildingLogic : MonoBehaviour, IHealth
{
    public Buildings.BuildingData data;
    protected Vector2Int myCell;
    public System.Collections.Generic.List<Vector2Int> occupiedCells = new System.Collections.Generic.List<Vector2Int>();
    public bool isEnemyOwned = false;
    public EnemyOutpost outpost;
    
    public int Health { get; set; }

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
        
        if (PlacementManager.HasInstance)
        {
            StartCoroutine(FlashRedTile());
        }

        if (BuildingManager.HasInstance)
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
}
