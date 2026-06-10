using System.Collections;
using Singleton;
using UnityEngine;

public class PlayerController : MonoBehaviour, IHealth
{
    private Vector2 targetPosition;
    private Vector2Int currentCell;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dodgeSpeed = 15f;
    [SerializeField] private float dodgeDistance = 2f;
    private bool isMoving = false;
    private bool isDodging = false;
    public bool canDodgeRoll = false;
    private Vector2Int lastMoveDirection = Vector2Int.up;
    private Coroutine currentCoroutine;
    
    public float width = 1.0f;
    public float height = 1.0f;
    public int maxHealth = 10;
    public int Health { get; set; }

    public float damageReductionFactor = 0f;
    public int healthPacksCount = 0;
    public int ammoReserve = 90;

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegenRate = 20f;
    public float staminaCostPerDodge = 30f;
    
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
        
        currentStamina = maxStamina;
        UiManager.Instance.UpdateStamina(currentStamina, maxStamina);

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
        // Use health pack with Tab key
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            UseHealthPack();
        }

        // Regenerate stamina
        if (currentStamina < maxStamina)
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * Time.deltaTime);
            UiManager.Instance.UpdateStamina(currentStamina, maxStamina);
        }

        Vector2Int inputDirection = Vector2Int.zero;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) inputDirection.y += 1;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) inputDirection.y -= 1;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) inputDirection.x -= 1;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) inputDirection.x += 1;

        if (inputDirection != Vector2Int.zero)
        {
            lastMoveDirection = inputDirection;
        }

        // Dodge Roll with Space - can interrupt normal movement
        if (canDodgeRoll && !isDodging && Input.GetKeyDown(KeyCode.Space) && currentStamina >= staminaCostPerDodge)
        {
            if (currentCoroutine != null) StopCoroutine(currentCoroutine);
            Vector2Int dodgeDir = inputDirection != Vector2Int.zero ? inputDirection : lastMoveDirection;
            currentCoroutine = StartCoroutine(DodgeRoutine(dodgeDir));
            return;
        }

        if (isMoving) return;

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
        
        if (GameManager.Instance != null && GameManager.Instance.MainTileMap != null)
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

    private IEnumerator DodgeRoutine(Vector2Int direction)
    {
        isMoving = true;
        isDodging = true;
        
        currentStamina -= staminaCostPerDodge;
        UiManager.Instance.UpdateStamina(currentStamina, maxStamina);
        
        // Dodge 2 tiles
        Vector2Int targetCell = currentCell + (direction * (int)dodgeDistance);
        
        // Basic boundary check - don't dodge into locked zones or off-screen
        // For simplicity, we'll just check if the final destination is valid
        Vector3Int targetCellV3 = new Vector3Int(targetCell.x, targetCell.y, 0);
        Vector2Int targetZone = ZoneManager.Instance.GetZoneCoordsFromTile(targetCellV3);
        
        if (!ZoneManager.Instance.IsZoneUnlocked(targetZone))
        {
            // If 2 tiles is too far, try 1 tile
            targetCell = currentCell + direction;
            targetCellV3 = new Vector3Int(targetCell.x, targetCell.y, 0);
            targetZone = ZoneManager.Instance.GetZoneCoordsFromTile(targetCellV3);
            if (!ZoneManager.Instance.IsZoneUnlocked(targetZone))
            {
                isMoving = false;
                isDodging = false;
                yield break;
            }
        }

        currentCell = targetCell;
        targetPosition = GridManager.Instance.CellToWorldConversion(currentCell);

        while (Vector2.Distance(transform.position, targetPosition) > 0.001f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, dodgeSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
        isDodging = false;
    }
    
    public Rectangle2D GetBoundingBox()
    {
        float angleRadians = transform.eulerAngles.z * Mathf.Deg2Rad;
        return TwoDCollision.CreateFromRotated(
            transform.position.x, transform.position.y, width, height, angleRadians);
    }


    public void TakeDamage(int amount)
    {
        if (Health <= 0 || isDodging) return;

        // Apply armor damage reduction (e.g. 0.2f = 20% less damage)
        int reduced = Mathf.Max(1, Mathf.RoundToInt(amount * (1f - damageReductionFactor)));

        Health -= reduced;
        UiManager.Instance.UpdateHp(Health, maxHealth);

        // Spawn floating damage text in red
        SpawnDamageNumber(reduced, Color.red, transform.position);

        // Visual flash feedback
        StartCoroutine(FlashRed());

        if (Health <= 0)
        {
            Die();
        }
    }

    /// <summary>Consume one health pack to restore half of max health.</summary>
    public void UseHealthPack()
    {
        if (healthPacksCount <= 0) return;
        if (Health >= maxHealth) return;

        healthPacksCount--;
        int healAmount = Mathf.CeilToInt(maxHealth * 0.5f);
        Health = Mathf.Min(maxHealth, Health + healAmount);
        UiManager.Instance.UpdateHp(Health, maxHealth);

        // Green heal number
        SpawnDamageNumber(healAmount, new Color(0.2f, 1f, 0.2f, 1f), transform.position);
        Debug.Log($"Used health pack. Restored {healAmount} HP. Packs remaining: {healthPacksCount}");
    }

    private void SpawnDamageNumber(int amount, Color color, Vector3 position)
    {
        GameObject textObj = new GameObject("DamageNumber");
        FloatingDamageText floatText = textObj.AddComponent<FloatingDamageText>();
        floatText.Initialize(amount.ToString(), color, position + new Vector3(0, 0.5f, 0));
    }

    private IEnumerator FlashRed()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();

        if (sprite != null)
        {
            Color originalColor = sprite.color;
            sprite.color = new Color(1f, 0.3f, 0.3f, 1f);
            yield return new WaitForSeconds(0.12f);
            sprite.color = originalColor;
        }
    }

    public void Die()
    {
        Debug.Log("die");
        UiManager.Instance.ShowGameOver();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
