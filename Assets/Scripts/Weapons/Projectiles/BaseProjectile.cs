using System;
using UnityEngine;

public class BaseProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    public float despawnTime = 2f;

    public void Start()
    {
        Invoke(nameof(Despawn), despawnTime); 
    }

    private int damage;
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == owner) return;

        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage);
            Despawn();
        }
        else if (!other.isTrigger)
        {
            // Hit a wall or something else solid
            Despawn();
        }
    }
    
    public void Despawn()
    {
        Destroy(gameObject);
    }
}
