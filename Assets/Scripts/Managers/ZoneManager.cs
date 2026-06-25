using UnityEngine;
using System.Collections.Generic;
using Singleton;

public class ZoneManager : SingletonBase<ZoneManager>
{
    [Header("Settings")]
    public Vector2Int zoneSizeInTiles = new Vector2Int(30, 15);
    public float initialUnlockCost = 20000f;
    public float costIncreasePerZone = 10000f;

    [Header("Initial Setup")]
    public List<Vector2Int> defaultUnlockedZones = new List<Vector2Int> { Vector2Int.zero };

    [Header("References")]
    public Camera mainCamera;

    private HashSet<Vector2Int> unlockedZones = new HashSet<Vector2Int>();
    private Vector2Int currentZone = Vector2Int.zero;

    private int currentZoomFactor = 1;
    private float defaultOrthographicSize;
    private UnityEngine.Rendering.Universal.PixelPerfectCamera pixelPerfectCam;

    public delegate void OnZoneUnlock();
    public event OnZoneUnlock onZoneUnlock;

    public int UnlockedZonesCount => unlockedZones.Count;

    protected override void Awake()
    {
        persistBetweenScenes = false;
        base.Awake();
        
        if (defaultUnlockedZones == null || defaultUnlockedZones.Count == 0)
        {
            unlockedZones.Add(Vector2Int.zero);
        }
        else
        {
            foreach (var zone in defaultUnlockedZones)
            {
                unlockedZones.Add(zone);
            }
        }
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

        defaultOrthographicSize = mainCamera != null ? mainCamera.orthographicSize : (zoneSizeInTiles.y / 2f);
        if (mainCamera != null)
        {
            pixelPerfectCam = mainCamera.GetComponent<UnityEngine.Rendering.Universal.PixelPerfectCamera>();
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

    public void ForceUnlockZone(Vector2Int zoneCoords)
    {
        if (!unlockedZones.Contains(zoneCoords))
        {
            unlockedZones.Add(zoneCoords);
            onZoneUnlock?.Invoke();
            Debug.Log($"ZoneManager: Force unlocked zone {zoneCoords}");
        }
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

    private void Update()
    {
        HandleZoomInput();
    }

    private void HandleZoomInput()
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            float scroll = Input.mouseScrollDelta.y;
            if (scroll > 0f)
            {
                ZoomIn();
            }
            else if (scroll < 0f)
            {
                ZoomOut();
            }
        }
    }

    private void ZoomIn()
    {
        if (currentZoomFactor > 1)
        {
            currentZoomFactor /= 2;
            UpdateCameraPosition(true);
        }
    }

    private void ZoomOut()
    {
        int nextZoomFactor = currentZoomFactor * 2;
        if (IsBlockUnlocked(currentZone, nextZoomFactor))
        {
            currentZoomFactor = nextZoomFactor;
            UpdateCameraPosition(true);
        }
    }

    public bool IsBlockUnlocked(Vector2Int zoneCoords, int zoomFactor)
    {
        int ax = Mathf.FloorToInt((float)zoneCoords.x / zoomFactor) * zoomFactor;
        int ay = Mathf.FloorToInt((float)zoneCoords.y / zoomFactor) * zoomFactor;

        for (int x = ax; x < ax + zoomFactor; x++)
        {
            for (int y = ay; y < ay + zoomFactor; y++)
            {
                if (!IsZoneUnlocked(new Vector2Int(x, y)))
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void UpdateCameraPosition(bool smooth)
    {
        if (mainCamera == null) return;

        // Calculate the world position of the center of the aligned block of zones
        int ax = Mathf.FloorToInt((float)currentZone.x / currentZoomFactor) * currentZoomFactor;
        int ay = Mathf.FloorToInt((float)currentZone.y / currentZoomFactor) * currentZoomFactor;

        float centerX = (ax + currentZoomFactor / 2f) * zoneSizeInTiles.x;
        float centerY = (ay + currentZoomFactor / 2f) * zoneSizeInTiles.y;
        
        Vector3 targetPos = new Vector3(centerX, centerY, mainCamera.transform.position.z);
        float targetOrthographicSize = defaultOrthographicSize * currentZoomFactor;

        if (smooth)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothPanAndZoom(targetPos, targetOrthographicSize));
        }
        else
        {
            if (pixelPerfectCam != null)
            {
                pixelPerfectCam.enabled = (currentZoomFactor == 1);
            }
            mainCamera.transform.position = targetPos;
            mainCamera.orthographicSize = targetOrthographicSize;
        }
    }

    private System.Collections.IEnumerator SmoothPanAndZoom(Vector3 targetPos, float targetSize)
    {
        if (pixelPerfectCam != null && currentZoomFactor > 1)
        {
            pixelPerfectCam.enabled = false;
        }

        float elapsed = 0;
        Vector3 startPos = mainCamera.transform.position;
        float startSize = mainCamera.orthographicSize;

        while (elapsed < panDuration)
        {
            float t = elapsed / panDuration;
            // Use the curve for better acceleration/deceleration
            float easedT = panCurve.Evaluate(t);
            
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, easedT);
            mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, easedT);
            elapsed += Time.deltaTime;
            yield return null;
        }
        mainCamera.transform.position = targetPos;
        mainCamera.orthographicSize = targetSize;

        if (pixelPerfectCam != null && currentZoomFactor == 1)
        {
            pixelPerfectCam.enabled = true;
        }
    }
}
