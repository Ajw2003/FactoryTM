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
        if (target == null)
        {
            if (GameManager.Instance != null && GameManager.Instance.playerController != null)
            {
                target = GameManager.Instance.playerController.gameObject;
            }
        }

        if (target == null) return;

        if (Vector3.Distance(transform.position, target.transform.position) <= sightRange)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
            weapon.target = target.transform.position;
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
        GameManager.Instance.ActiveEnemies.Remove(this);
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
        if (Health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Destroy(this.gameObject);
    }
}
