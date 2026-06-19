using UnityEngine;

public class PlayerExplosiveProjectile : BaseProjectile
{
    public float ExplosionRadius = 2f;

    public override void InitializeExplosive(float radius)
    {
        ExplosionRadius = radius;
    }

    public override void CheckForCollisions()
    {
        float angleRadians = transform.eulerAngles.z * Mathf.Deg2Rad;
        Rectangle2D bulletBox = TwoDCollision.CreateFromRotated(
            transform.position.x, transform.position.y, width, height, angleRadians);

        bool hit = false;
        
        for (int i = GameManager.Instance.ActiveEnemies.Count - 1; i >= 0; i--)
        {
            CartelMember currentEnemy = GameManager.Instance.ActiveEnemies[i];
            Rectangle2D enemyBox = currentEnemy.GetBoundingBox();

            if (Rectangle2D.CheckCollision(bulletBox, enemyBox))
            {
                hit = true;
                break; 
            }
        }
        
        if (hit)
        {
            Explode();
        }
    }

    private void Explode()
    {
        // Deal damage to all enemies within ExplosionRadius
        for (int i = GameManager.Instance.ActiveEnemies.Count - 1; i >= 0; i--)
        {
            CartelMember enemy = GameManager.Instance.ActiveEnemies[i];
            if (enemy == null) continue;
            
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist <= ExplosionRadius)
            {
                enemy.TakeDamage(Damage);
            }
        }
        
        // Spawn Visual Effect
        GameObject visualObj = new GameObject("ExplosionVisual");
        visualObj.transform.position = transform.position;
        ExplosionVisual visual = visualObj.AddComponent<ExplosionVisual>();
        visual.Initialize(ExplosionRadius, new Color(1f, 0.5f, 0f)); // Orange for player

        Destroy(gameObject);
    }
}
