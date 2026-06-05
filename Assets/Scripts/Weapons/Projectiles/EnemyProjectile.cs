using System;
using UnityEngine;

public class EnemyProjectile : BaseProjectile
{
    public override void CheckForCollisions()
    {
        if (PlayerController.Instance == null || !PlayerController.Instance.gameObject.activeInHierarchy)
        {
            return; 
        }

        // 1. Generate the Bullet's bounding box
        float angleRadians = transform.eulerAngles.z * Mathf.Deg2Rad;
        Rectangle2D bulletBox = TwoDCollision.CreateFromRotated(
            transform.position.x, transform.position.y, width, height, angleRadians);

        // 2. Get the Player's bounding box via our new Singleton
        Rectangle2D playerBox = PlayerController.Instance.GetBoundingBox();

        // 3. Check for collision
        if (Rectangle2D.CheckCollision(bulletBox, playerBox))
        {
            // The player takes damage! Notice how clean this is because of IDamageable.
            PlayerController.Instance.TakeDamage(Damage);
            
            // Destroy the enemy bullet
            Destroy(gameObject);
        }
    }
}
