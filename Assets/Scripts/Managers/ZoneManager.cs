using UnityEngine;
using System.Collections.Generic;
using Singleton;

public class ZoneManager : SingletonBase<ZoneManager>
{
    [Header("Settings")]
    public Vector2Int zoneSizeInTiles = new Vector2Int(30, 15);
    public float initialUnlockCost = 20000f;
    public float costIncreasePerZone = 10000f;

    [Header("References")]
    public Camera mainCamera;

    private HashSet<Vector2Int> unlockedZones = new HashSet<Vector2Int>();
    private Vector2Int currentZone = Vector2Int.zero;

    public delegate void OnZoneUnlock();
    public event OnZoneUnlock onZoneUnlock;

    public int UnlockedZonesCount => unlockedZones.Count;

    protected override void Awake()
    {
        persistBetweenScenes = false;
        base.Awake();
        unlockedZones.Add(Vector2Int.zero);
    }

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        if (zoneSizeInTiles.x <= 0 || zoneSizeInTiles.y <= 0)
        {
            if (mainCamera.orthographic)
            {
                float height = 2f * mainCamera.orthographicSize;
                float width = height * mainCamera.aspect;
                zoneSizeInTiles = new Vector2Int(Mathf.RoundToInt(width), Mathf.RoundToInt(height));
            }
            else
            {
                // Fallback default
                zoneSizeInTiles = new Vector2Int(20, 10);
            }
        }
        
        UpdateCameraPosition(false);
    }

    public bool IsZoneUnlocked(Vector2Int zoneCoords)
    {
        return unlockedZones.Contains(zoneCoords);
    }

    public bool IsTileInsideUnlockedZone(Vector3Int cell)
    {
        Vector2Int zoneCoords = GetZoneCoordsFromTile(cell);
        return unlockedZones.Contains(zoneCoords);
    }

    public Vector2Int GetZoneCoordsFromTile(Vector3Int cell)
    {
        int x = Mathf.FloorToInt((float)cell.x / zoneSizeInTiles.x);
        int y = Mathf.FloorToInt((float)cell.y / zoneSizeInTiles.y);
        return new Vector2Int(x, y);
    }

    public void SetCurrentZone(Vector2Int targetZone)
    {
        if (targetZone == currentZone) return;

        // Only allow navigating to UNLOCKED zones via movement
        if (unlockedZones.Contains(targetZone))
        {
            currentZone = targetZone;
            UpdateCameraPosition(true);
        }
    }

    public bool TryUnlockZone(Vector2Int targetZone)
    {
        if (unlockedZones.Contains(targetZone)) return true;

        float cost = GetUnlockCost();
        if (CurrencyManager.Instance.currentCurrencyValue >= cost)
        {
            CurrencyManager.Instance.RemoveCurrency(cost);
            unlockedZones.Add(targetZone);
            Debug.Log("Zone Unlocked: " + targetZone);
            onZoneUnlock?.Invoke();
            return true;
        }
        
        Debug.Log("Not enough currency to unlock zone! Cost: " + cost);
        return false;
    }

    // Methods for UI Buttons to call directly
    public bool UnlockNorth() => TryUnlockZone(currentZone + Vector2Int.up);
    public bool UnlockSouth() => TryUnlockZone(currentZone + Vector2Int.down);
    public bool UnlockEast() => TryUnlockZone(currentZone + Vector2Int.right);
    public bool UnlockWest() => TryUnlockZone(currentZone + Vector2Int.left);

    public float GetUnlockCost()
    {
        // Simple cost scaling: base + (unlocked_count - 1) * increase
        return initialUnlockCost + (unlockedZones.Count - 1) * costIncreasePerZone;
    }

    public Vector2Int GetCurrentZone() => currentZone;

    [Header("Camera Control")]
    public AnimationCurve panCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float panDuration = 0.5f;

    private void UpdateCameraPosition(bool smooth)
    {
        // Calculate the world position of the center of the target zone
        float centerX = (currentZone.x * zoneSizeInTiles.x) + (zoneSizeInTiles.x / 2f);
        float centerY = (currentZone.y * zoneSizeInTiles.y) + (zoneSizeInTiles.y / 2f);
        
        Vector3 targetPos = new Vector3(centerX, centerY, mainCamera.transform.position.z);

        if (smooth)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothPan(targetPos));
        }
        else
        {
            mainCamera.transform.position = targetPos;
        }
    }

    private System.Collections.IEnumerator SmoothPan(Vector3 targetPos)
    {
        float elapsed = 0;
        Vector3 startPos = mainCamera.transform.position;

        while (elapsed < panDuration)
        {
            float t = elapsed / panDuration;
            // Use the curve for better acceleration/deceleration
            float easedT = panCurve.Evaluate(t);
            
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, easedT);
            elapsed += Time.deltaTime;
            yield return null;
        }
        mainCamera.transform.position = targetPos;
    }
}
