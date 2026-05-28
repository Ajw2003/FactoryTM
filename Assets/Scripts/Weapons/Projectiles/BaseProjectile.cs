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

    public void Initialize(Vector2 target, float bulletSpeed)
    {
        speed = bulletSpeed;
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
        Destroy(gameObject);
    }
}
