using System;
using UnityEngine;

public class BaseProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    public float despawnTime = 2f;
    public float width = 0.2f;
    public float height = 0.5f;
    public int Damage = 1;

    public void Start()
    {
        Invoke(nameof(Despawn), despawnTime); 
    }

    public void Initialize(Vector2 target, float bulletSpeed, int damage)
    {
        speed = bulletSpeed;
        direction = (target - (Vector2)transform.position).normalized;
        Damage = damage;
        
        // Optional: Rotate the bullet to face the target
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void Update()
    {
        transform.Translate(direction * (speed * Time.deltaTime), Space.World);
        CheckForCollisions();
    }
    
    public void Despawn()
    {
        Destroy(gameObject);
    }
    
    public virtual void CheckForCollisions()
    {
        float angleRadians = transform.eulerAngles.z * Mathf.Deg2Rad;
        Rectangle2D bulletBox = TwoDCollision.CreateFromRotated(
            transform.position.x, transform.position.y, width, height, angleRadians);

        // Loop backwards through the list! 
        // This is a neat trick: if you destroy an enemy, it gets removed from the list.
        // Looping backwards ensures the list indices don't break when an item is removed.
        for (int i = GameManager.Instance.ActiveEnemies.Count - 1; i >= 0; i--)
        {
            CartelMember currentEnemy = GameManager.Instance.ActiveEnemies[i];
            Rectangle2D enemyBox = currentEnemy.GetBoundingBox();

            if (Rectangle2D.CheckCollision(bulletBox, enemyBox))
            {
                // Destroy the enemy
                currentEnemy.TakeDamage(Damage);
                
                // Destroy the bullet and exit the loop so we don't hit two things at once
                Destroy(gameObject);
                break; 
            }
        }
    }
}
