using UnityEngine;

public class EnemyExplosiveProjectile : EnemyProjectile
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

        // Check player
        if (PlayerController.Instance != null && PlayerController.Instance.gameObject.activeInHierarchy)
        {
            Rectangle2D playerBox = PlayerController.Instance.GetBoundingBox();
            if (Rectangle2D.CheckCollision(bulletBox, playerBox))
            {
                hit = true;
            }
        }

        // Check buildings
        if (!hit && BuildingManager.HasInstance)
        {
            foreach (var building in BuildingManager.Instance.Buildings)
            {
                if (building != null && building.Health > 0 && building.data.type != Buildings.BuildingType.Conveyor)
                {
                    foreach (var cell in building.occupiedCells)
                    {
                        Vector3 cellWorldPos = GridManager.Instance.CellToWorldConversion(cell);
                        Rectangle2D cellBox = TwoDCollision.CreateFromRotated(cellWorldPos.x, cellWorldPos.y, 1f, 1f, 0f);
                        if (Rectangle2D.CheckCollision(bulletBox, cellBox))
                        {
                            hit = true;
                            break;
                        }
                    }
                    if (hit) break;
                }
            }
        }

        if (hit)
        {
            Explode();
        }
    }

    private void Explode()
    {
        // Hit player
        if (PlayerController.Instance != null && PlayerController.Instance.gameObject.activeInHierarchy)
        {
            float dist = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
            if (dist <= ExplosionRadius)
            {
                PlayerController.Instance.TakeDamage(Damage);
            }
        }

        // Hit buildings
        if (BuildingManager.HasInstance)
        {
            foreach (var building in BuildingManager.Instance.Buildings)
            {
                if (building != null && building.Health > 0 && building.data.type != Buildings.BuildingType.Conveyor)
                {
                    float dist = Vector2.Distance(transform.position, building.transform.position);
                    if (dist <= ExplosionRadius)
                    {
                        building.TakeDamage(Damage);
                    }
                }
            }
        }

        Destroy(gameObject);
    }
}
