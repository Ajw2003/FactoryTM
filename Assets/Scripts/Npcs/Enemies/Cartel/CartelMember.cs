using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum MemberType
{
    lackie,
    enforcer,
    bossman,
    legend,
    kingpin
}

public class CartelMember : MonoBehaviour, IHealth
{

    public MemberType Rank { get; private set; }
    public bool isRaidEnemy = false;

    [SerializeField] private float speed;

    [SerializeField] private float turnSpeed;
    
    [SerializeField] private float sightRange = 12f;

    [SerializeField] private EnemyWeapon weapon;
    
    [SerializeField] private GameObject target;
    
    [SerializeField] private float attackSpeed;

    [SerializeField] private int profitFromKill;

    [SerializeField] private float engagementDistance = 5f;
    [SerializeField] private float attackDistance = 2f;
    private float forceTargetTimer = 0f;

    private bool attacking;
    
    public int Health { get; set; }
    public int MaxHealth = 10;
    
        
    

    public float width = 1.0f;
    public float height = 1.0f;

    private Coroutine attackRoutine;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Setup();
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            target = GameManager.Instance.playerController.gameObject;
        }
        weapon = GetComponent<EnemyWeapon>();
        if (weapon != null)
        {
            weapon.isEnemyFired = true;
        }
        Health = MaxHealth;

        if (isRaidEnemy) sightRange = 100f;
    }

    // Update is called once per frame
    void Update()
    {
        FindClosestTarget();

        if (target == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

        if (distanceToTarget <= sightRange)
        {
            if (distanceToTarget > attackDistance)
            {
                Vector3 nextPos = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);

                bool collision = false;
                float angleRadians = transform.eulerAngles.z * Mathf.Deg2Rad;
                Rectangle2D enemyBox = TwoDCollision.CreateFromRotated(nextPos.x, nextPos.y, width, height, angleRadians);

                if (PlacementManager.HasInstance && GridManager.Instance != null)
                {
                    Vector2 tileSize = GridManager.Instance.tileSize;
                    int minX = Mathf.FloorToInt((nextPos.x - width / 2f) / tileSize.x);
                    int maxX = Mathf.FloorToInt((nextPos.x + width / 2f) / tileSize.x);
                    int minY = Mathf.FloorToInt((nextPos.y - height / 2f) / tileSize.y);
                    int maxY = Mathf.FloorToInt((nextPos.y + height / 2f) / tileSize.y);

                    var activeBuildings = PlacementManager.Instance.GetActiveBuildings();
                    for (int x = minX; x <= maxX; x++)
                    {
                        for (int y = minY; y <= maxY; y++)
                        {
                            Vector2Int cell = new Vector2Int(x, y);
                            if (activeBuildings.TryGetValue(cell, out GameObject buildingObj))
                            {
                                if (buildingObj != null)
                                {
                                    BuildingLogic building = buildingObj.GetComponent<BuildingLogic>();
                                     if (building != null && building.Health > 0 && !building.isEnemyOwned && building.data.type != Buildings.BuildingType.Conveyor)
                                    {
                                        Vector3 cellWorldPos = GridManager.Instance.CellToWorldConversion(cell);
                                        Rectangle2D cellBox = TwoDCollision.CreateFromRotated(cellWorldPos.x, cellWorldPos.y, 1f, 1f, 0f);
                                        if (Rectangle2D.CheckCollision(enemyBox, cellBox))
                                        {
                                            collision = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        if (collision) break;
                    }
                }

                if (!collision)
                {
                    transform.position = nextPos;
                }
            }

            weapon.target = target.transform.position;

            // Rotate towards target we are aiming at
            Vector2 direction = (Vector2)target.transform.position - (Vector2)transform.position;
            if (direction.sqrMagnitude > 0.01f)
            {
                float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) + 90f;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }

            if (attacking) return;
            attackRoutine = StartCoroutine(Attack());
            attacking = true;
        }
    }

    private IEnumerator Attack()
    {
        while (target != null && Vector3.Distance(transform.position, target.transform.position) <= sightRange)
        {
            yield return new WaitForSeconds(attackSpeed);
            if (target != null)
            {
                weapon.Shoot();
            }
        }

        attacking = false;
        yield return null;
    }

    private float targetSearchTimer = 0f;
    private void FindClosestTarget()
    {
        if (forceTargetTimer > 0f)
        {
            forceTargetTimer -= Time.deltaTime;
            if (GameManager.Instance != null && GameManager.Instance.playerController != null)
            {
                target = GameManager.Instance.playerController.gameObject;
                return;
            }
        }

        targetSearchTimer -= Time.deltaTime;
        if (targetSearchTimer > 0f) return;
        targetSearchTimer = 0.5f; // Re-evaluate target every 0.5 seconds

        float minDistance = float.MaxValue;
        GameObject closest = null;

        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            float playerDist = Vector3.Distance(transform.position, GameManager.Instance.playerController.transform.position);
            
            if (playerDist <= engagementDistance)
            {
                target = GameManager.Instance.playerController.gameObject;
                return;
            }

            minDistance = playerDist;
            closest = GameManager.Instance.playerController.gameObject;
        }

        if (BuildingManager.HasInstance)
        {
            foreach (var building in BuildingManager.Instance.Buildings)
            {
                if (building != null && building.Health > 0 && !building.isEnemyOwned && building.data.type != Buildings.BuildingType.Conveyor)
                {
                    float dist = Vector3.Distance(transform.position, building.transform.position);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closest = building.gameObject;
                    }
                }
            }
        }

        target = closest;
    }

    public void Setup()
    {
        switch (Rank)
        {
            case MemberType.lackie:
                Rank = MemberType.lackie;
                break;
            case MemberType.enforcer:
                Rank = MemberType.enforcer;
                break;
            case MemberType.bossman:
                Rank = MemberType.bossman;
                break;
            case MemberType.legend:
                Rank = MemberType.legend;
                break;
            case MemberType.kingpin:
                Rank = MemberType.kingpin;
                break;
            default:
                break;
        }
    }


    // Add to the list when spawned
    void OnEnable()
    {
        GameManager.Instance.ActiveEnemies.Add(this);
    }

    // Remove from the list when destroyed
    void OnDisable()
    {
        if (GameManager.HasInstance) 
        {
            GameManager.Instance.ActiveEnemies.Remove(this);
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

        // Spawn floating damage text in bright yellow-orange
        FloatingTextSettings settings = Resources.Load<FloatingTextSettings>("FloatingTextSettings/CartelDamageSettings");
        FloatingTextManager.Instance.Spawn(amount.ToString(), transform.position, settings);

        // Visual flash feedback
        StartCoroutine(FlashRed());

        // Force target the player when shot
        forceTargetTimer = 5f;

        if (Health <= 0)
        {
            Die();
        }
    }

    public void ChangeHealth(int amount, int previous)
    {
        
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
        if (Managers.GameStatsManager.HasInstance)
        {
            Managers.GameStatsManager.Instance.IncrementKills();
        }
        CurrencyManager.Instance.AddCurrency(profitFromKill);
        
        FloatingTextSettings settings = Resources.Load<FloatingTextSettings>("FloatingTextSettings/CartelKillRewardSettings");
        FloatingTextManager.Instance.Spawn(profitFromKill.ToString(), transform.position, settings);

        Destroy(this.gameObject);
    }
}

