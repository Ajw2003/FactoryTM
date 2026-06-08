using System.Collections;
using Singleton;
using UnityEngine;

public class PlayerController : MonoBehaviour, IHealth
{
    private Vector2 targetPosition;
    private Vector2Int currentCell;
    [SerializeField] private float moveSpeed = 5f;
    private bool isMoving = false;
    private Coroutine currentCoroutine;
    
    public float width = 1.0f;
    public float height = 1.0f;
    public int maxHealth = 10;
    public int Health { get; set; }

    
    public static PlayerController Instance { get; private set; }

   
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        Health = maxHealth;
        UiManager.Instance.UpdateHp(Health, maxHealth);
        
        Vector2Int spawnCell = Vector2Int.zero;
        if (ZoneManager.Instance != null)
        {
            Vector2Int zoneSize = ZoneManager.Instance.zoneSizeInTiles;
            spawnCell = new Vector2Int(zoneSize.x / 2, zoneSize.y / 2);
        }
        else if (GridManager.Instance != null)
        {
            spawnCell = GridManager.Instance.center;
        }

        currentCell = spawnCell;
        if (GridManager.Instance != null)
        {
            transform.position = GridManager.Instance.CellToWorldConversion(currentCell);
        }
        targetPosition = transform.position;
    }

    private void Update()
    {
        if (isMoving) return;

        Vector2Int inputDirection = Vector2Int.zero;//set input to zero
        
        //get input and convert to direction in grid space +-1 in x or y coords

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) inputDirection.y += 1;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) inputDirection.y -= 1;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) inputDirection.x -= 1;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) inputDirection.x += 1;

        if (inputDirection != Vector2Int.zero)
        {
            Vector2Int nextCell = currentCell + inputDirection;
            Vector3Int nextCellV3 = new Vector3Int(nextCell.x, nextCell.y, 0);
            
            Vector2Int nextZone = ZoneManager.Instance.GetZoneCoordsFromTile(nextCellV3);
            Vector2Int currentZone = ZoneManager.Instance.GetCurrentZone();

            if (nextZone != currentZone)
            {
                // ONLY allow movement if the zone is already unlocked
                if (ZoneManager.Instance.IsZoneUnlocked(nextZone))
                {
                    ZoneManager.Instance.SetCurrentZone(nextZone);
                    currentCoroutine = StartCoroutine(MoveRoutine(nextCell));
                }
                else
                {
                    //add code here for visual when trying to access locked zone
                }
            }
            else
            {
                // Moving within the current zone is allowed if it is within the camera's viewport
                bool isVisible = true;
                Camera mainCam = ZoneManager.Instance != null && ZoneManager.Instance.mainCamera != null ? ZoneManager.Instance.mainCamera : Camera.main;
                if (mainCam != null && GridManager.Instance != null)
                {
                    Vector2 targetWorldPos = GridManager.Instance.CellToWorldConversion(nextCell);
                    Vector3 viewportPos = mainCam.WorldToViewportPoint(targetWorldPos);
                    // Check if target is inside the viewport with a small buffer
                    if (viewportPos.x < 0.02f || viewportPos.x > 0.98f || viewportPos.y < 0.04f || viewportPos.y > 0.96f)
                    {
                        isVisible = false;
                    }
                }

                if (isVisible)
                {
                    currentCoroutine = StartCoroutine(MoveRoutine(nextCell));
                }
            }
        }
    }

    private IEnumerator MoveRoutine(Vector2Int targetCell)
    {
        isMoving = true;
        currentCell = targetCell;//change current cell to target
        
        if (GameManager.Instance != null && GameManager.Instance.buildingTilemap != null)
        {
            targetPosition = GridManager.Instance.CellToWorldConversion(currentCell);
        }

        while (Vector2.Distance(transform.position, targetPosition) > 0.001f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);// move towards the target cell
            yield return null;
        }

        transform.position = targetPosition;// finalize and directly set position.
        isMoving = false;
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
    }
    
    public Rectangle2D GetBoundingBox()
    {
        float angleRadians = transform.eulerAngles.z * Mathf.Deg2Rad;
        return TwoDCollision.CreateFromRotated(
            transform.position.x, transform.position.y, width, height, angleRadians);
    }


    public void TakeDamage(int amount)
    {
        if (Health <= 0) return;

        Health -= amount;
        UiManager.Instance.UpdateHp(Health, maxHealth);
        if (Health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log("die");
        UiManager.Instance.ShowGameOver();
    }
}
