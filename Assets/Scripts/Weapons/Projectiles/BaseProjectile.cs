using System;
using UnityEngine;

public class BaseProjectile : BaseEntity
{
    private Vector2 direction;
    private float speed;
    public float despawnTime = 2f;

    public void Start()
    {
        Invoke(nameof(Despawn), despawnTime); 
    }

    public int damage;
    private GameObject owner;

    public void Initialize(Vector2 target, float bulletSpeed, int bulletDamage, GameObject shooter)
    {
        speed = bulletSpeed;
        damage = bulletDamage;
        owner = shooter;
        direction = (target - (Vector2)transform.position).normalized;
        
        // Optional: Rotate the bullet to face the target
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void Update()
    {
        transform.Translate(direction * (speed * Time.deltaTime), Space.World);
    }
    
    public void Despawn()
    {
        OnDisable();
        Destroy(gameObject);
    }
    public bool IsCollidingWith(BaseEntity other)
    {
        // Get the vector between the two objects
        Vector2 difference = transform.position - other.transform.position;
        
        // Get the squared distance
        float distanceSquared = difference.sqrMagnitude;
        
        // Get the squared sum of the radii
        float radiiSum = hitRadius + other.hitRadius;
        float radiiSumSquared = radiiSum * radiiSum;

        return distanceSquared <= radiiSumSquared;
    }
    
    private void OnEnable()
    {
        if (CollisionManager.Instance == null) return;
            CollisionManager.Instance.RegisterBullet(this);
    }

    // Automatically remove from the manager when deactivated
    private void OnDisable()
    {
        if (CollisionManager.Instance == null) return;
        else
            CollisionManager.Instance.DeregisterBullet(this);
    }
}

