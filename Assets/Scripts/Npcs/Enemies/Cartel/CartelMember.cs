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

    [SerializeField] private float speed;

    [SerializeField] private float turnSpeed;
    
    [SerializeField] private float sightRange;

    [SerializeField] private EnemyWeapon weapon;
    
    [SerializeField] private GameObject target;
    
    [SerializeField] private float attackSpeed;

    [SerializeField] private int profitFromKill;

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
        Health = MaxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        FindClosestTarget();

        if (target == null) return;

        if (Vector3.Distance(transform.position, target.transform.position) <= sightRange)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
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
        while (Vector3.Distance(transform.position, target.transform.position) <= sightRange)
        {
            yield return new WaitForSeconds(attackSpeed);
            weapon.Shoot();
        }

        attacking = false;
        yield return null;
    }

    private float targetSearchTimer = 0f;
    private void FindClosestTarget()
    {
        targetSearchTimer -= Time.deltaTime;
        if (targetSearchTimer > 0f) return;
        targetSearchTimer = 0.5f; // Re-evaluate target every 0.5 seconds

        float minDistance = float.MaxValue;
        GameObject closest = null;

        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            float playerDist = Vector3.Distance(transform.position, GameManager.Instance.playerController.transform.position);
            minDistance = playerDist;
            closest = GameManager.Instance.playerController.gameObject;
        }

        if (BuildingManager.HasInstance)
        {
            foreach (var building in BuildingManager.Instance.Buildings)
            {
                if (building != null && building.Health > 0 && building.data.type != Buildings.BuildingType.Conveyor) // ignore conveyors maybe? Actually, all buildings are targetable except maybe conveyors if they are indestructible. But let's just target all for now.
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
        SpawnDamageNumber(amount, new Color(1f, 0.6f, 0f, 1f), transform.position);

        // Visual flash feedback
        StartCoroutine(FlashRed());

        if (Health <= 0)
        {
            Die();
        }
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
        CurrencyManager.Instance.AddCurrency(profitFromKill);
        SpawnDamageNumber(profitFromKill, Color.forestGreen, transform.position);
        Destroy(this.gameObject);
    }
}
