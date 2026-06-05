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
        Vector2 spawnPos = Vector2.zero;
        if (ZoneManager.Instance != null && ZoneManager.Instance.mainCamera != null)
        {
            spawnPos = ZoneManager.Instance.mainCamera.transform.position;
        }
        else if (Camera.main != null)
        {
            spawnPos = Camera.main.transform.position;
        }
        else if (GridManager.Instance != null)
        {
            spawnPos = GridManager.Instance.center;
        }

        currentCell = GridManager.Instance.WorldToCellConversion(spawnPos);
        transform.position = GridManager.Instance.CellToWorldConversion(currentCell);
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
                    Debug.Log("Zone is locked! Unlock it via the menu first.");
                }
            }
            else
            {
                // Moving within the current zone is always allowed
                currentCoroutine = StartCoroutine(MoveRoutine(nextCell));
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
    }
}
