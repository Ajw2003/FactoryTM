using System.Collections;
using Singleton;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : BaseEntity
{
    private Vector2 targetPosition;
    private Vector2Int currentCell;
    [SerializeField] private float moveSpeed = 5f;
    private bool isMoving = false;
    private Coroutine currentCoroutine;
    public Health health;

    protected  void Awake()
    {
        if (GameManager.Instance != null) GameManager.Instance.player = this;
        health = GetComponent<Health>();
        if (health == null) health = gameObject.AddComponent<Health>();
        
        health.onDeath.AddListener(OnDeath);
    }

    private void Start()
    {
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
    
    private void OnDeath()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
}
