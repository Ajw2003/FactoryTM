using System.Collections;
using Code.Scripts.EventSystems;
using Code.Scripts.Interfaces.EventTypes;
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
    private Vector2 rawInputDirection;
    private Coroutine currentCoroutine;
    private PlayerWeapon weapon;
    
    public float width = 1.0f;
    public float height = 1.0f;
    public int maxHealth = 10;
    public int Health { get; set; }

    public float damageReductionFactor = 0f;
    public int healthPacksCount = 0;
    public int ammoReserve = 90;

    public enum PlayerMode { Combat, Building }
    [Header("Game Mode")]
    public PlayerMode currentMode = PlayerMode.Combat;
    private HotbarUI hotbarUIInstance;
    public delegate void ModeChangedAction(PlayerMode mode);
    public event ModeChangedAction OnModeChanged;
    public event System.Action OnPlayerDodge;

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegenRate = 20f;
    public float staminaCostPerDodge = 30f;
    
    public static PlayerController Instance { get; private set; }
    public StateMachine.PlayerStateMachine StateMachine { get; private set; }
    public Vector2 RawInputDirection => rawInputDirection;

   
    
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

        StateMachine = new StateMachine.PlayerStateMachine(this);
    }

    private void Start()
    {
        Health = maxHealth;
        
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

        // Shift player 2 cells north of IDT center to prevent overlap
        spawnCell += new Vector2Int(0, 2);

        currentCell = spawnCell;
        if (GridManager.Instance != null)
        {
            transform.position = GridManager.Instance.CellToWorldConversion(currentCell);
        }
        targetPosition = transform.position;

        // Subscribe to input events
        EventManager.Instance?.Subscribe(this, (PlayerMoveEvent e) => OnMoveInput(e.MoveInput));
        EventManager.Instance?.Subscribe(this, (PlayerDodgeEvent e) => OnDodgeInput());
        EventManager.Instance?.Subscribe(this, (PlayerHealEvent e) => OnHealInput());
        EventManager.Instance?.Subscribe(this, (PlayerOpenStoreEvent e) => OnOpenStoreInput());

        weapon = GetComponentInChildren<PlayerWeapon>();

        // Cache HotbarUI and set to initial mode (Combat Mode = hidden hotbar)
        hotbarUIInstance = FindFirstObjectByType<HotbarUI>();
        if (hotbarUIInstance != null)
        {
            hotbarUIInstance.gameObject.SetActive(false);
        }

        StateMachine.Initialize(StateMachine.idleState);
    }

    private void OnMoveInput(Vector2 input)
    {
        rawInputDirection = input;
    }

    private void OnDodgeInput()
    {
        if (canDodgeRoll && !isDodging && currentStamina >= staminaCostPerDodge)
        {
            OnPlayerDodge?.Invoke();
            StateMachine.TransitionTo(StateMachine.dodgeState);
        }
    }

    private void OnHealInput()
    {
        UseHealthPack();
    }

    private void OnOpenStoreInput()
    {
        if (TutorialManager.HasInstance && TutorialManager.Instance.IsStoreLocked())
        {
            if (UiManager.HasInstance)
            {
                UiManager.Instance.ShowGeneralAlert("STORE OFFLINE - REBOOT IDT FIRST", new Color(1f, 0.3f, 0.3f));
            }
            return;
        }

        if (StateMachine.CurrentState == StateMachine.idleState || StateMachine.CurrentState == StateMachine.walkState)
        {
            StateMachine.TransitionTo(StateMachine.storeState);
        }
        else if (StateMachine.CurrentState == StateMachine.storeState)
        {
            StateMachine.TransitionTo(StateMachine.idleState);
        }
    }

    public Vector2Int GetDiscreteInputDirection()
    {
        Vector2Int inputDirection = Vector2Int.zero;
        if (rawInputDirection.y > 0.5f) inputDirection.y = 1;
        else if (rawInputDirection.y < -0.5f) inputDirection.y = -1;
        
        if (rawInputDirection.x > 0.5f) inputDirection.x = 1;
        else if (rawInputDirection.x < -0.5f) inputDirection.x = -1;
        
        return inputDirection;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && StateMachine.CurrentState != StateMachine.storeState && StateMachine.CurrentState != StateMachine.deadState)
        {
            ToggleGameMode();
        }

        // Regenerate stamina
        if (currentStamina < maxStamina)
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * Time.deltaTime);
            UiManager.Instance.UpdateStamina(currentStamina, maxStamina);
        }

        // Rotate towards aim target
        if (StateMachine.CurrentState != StateMachine.deadState && StateMachine.CurrentState != StateMachine.storeState)
        {
            if (weapon == null) weapon = GetComponentInChildren<PlayerWeapon>();
            if (weapon != null)
            {
                Vector2 targetPos = weapon.target;
                Vector2 direction = targetPos - (Vector2)transform.position;
                if (direction.sqrMagnitude > 0.01f)
                {
                    float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg)+ 90f;
                    transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                }
            }
        }

        StateMachine.Update();
    }

    public void ToggleGameMode()
    {
        currentMode = (currentMode == PlayerMode.Combat) ? PlayerMode.Building : PlayerMode.Combat;

        if (currentMode == PlayerMode.Combat)
        {
            if (PlacementManager.Instance != null)
            {
                PlacementManager.Instance.ChangeSelection(null);
            }
            if (hotbarUIInstance == null)
            {
                hotbarUIInstance = FindFirstObjectByType<HotbarUI>();
            }
            if (hotbarUIInstance != null)
            {
                hotbarUIInstance.gameObject.SetActive(false);
            }

            if (UiManager.HasInstance)
            {
                UiManager.Instance.ShowGeneralAlert("COMBAT MODE ACTIVE", new Color(1f, 0.3f, 0.3f));
            }
        }
        else
        {
            if (hotbarUIInstance == null)
            {
                hotbarUIInstance = FindFirstObjectByType<HotbarUI>();
            }
            if (hotbarUIInstance != null)
            {
                hotbarUIInstance.gameObject.SetActive(true);
            }

            if (PlacementManager.Instance != null && HotbarManager.Instance != null)
            {
                PlacementManager.Instance.ChangeSelection(HotbarManager.Instance.GetSelectedBuilding());
            }

            if (UiManager.HasInstance)
            {
                UiManager.Instance.ShowGeneralAlert("BUILDING MODE ACTIVE", new Color(0.3f, 0.9f, 0.3f));
            }
        }

        OnModeChanged?.Invoke(currentMode);
    }

    public void SetLastMoveDirection(Vector2Int direction)
    {
        if (direction != Vector2Int.zero)
            lastMoveDirection = direction;
    }

    public Vector2Int GetLastMoveDirection() => lastMoveDirection;

    public bool IsMoving() => isMoving;
    public bool IsDodging() => isDodging;

    public void StartMove(Vector2Int nextCell)
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(MoveRoutine(nextCell));
    }

    public void StartDodge(Vector2Int direction)
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(DodgeRoutine(direction));
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
        if (Health <= 0 || isDodging || StateMachine.CurrentState == StateMachine.deadState) return;

        // Apply armor damage reduction (e.g. 0.2f = 20% less damage)
        int reduced = Mathf.Max(1, Mathf.RoundToInt(amount * (1f - damageReductionFactor)));
        Health = Mathf.Max(0, Health - reduced);
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

    public void ChangeHealth(int amount, int previous)
    {
        // Satisfies IHealth interface, but unused since we modify Health directly.
    }

    /// <summary>Consume one health pack to restore half of max health.</summary>
    public void UseHealthPack()
    {
        if (healthPacksCount <= 0 || StateMachine.CurrentState == StateMachine.deadState) return;
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
        StateMachine.TransitionTo(StateMachine.deadState);
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
