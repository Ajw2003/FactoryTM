using System.Collections.Generic;
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

        // Check buildings using current cell
        if (!hit && PlacementManager.HasInstance && GridManager.Instance != null)
        {
            Vector2Int cell = GridManager.Instance.WorldToCellConversion(transform.position);
            var activeBuildings = PlacementManager.Instance.GetActiveBuildings();
            if (activeBuildings.TryGetValue(cell, out GameObject buildingObj))
            {
                if (buildingObj != null)
                {
                    BuildingLogic building = buildingObj.GetComponent<BuildingLogic>();
                    if (building != null && building.Health > 0 && !building.isEnemyOwned && building.data.type != Buildings.BuildingType.Conveyor)
                    {
                        hit = true;
                    }
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

        // Hit buildings using grid check (O(Radius^2))
        if (PlacementManager.HasInstance && GridManager.Instance != null)
        {
            Vector2 tileSize = GridManager.Instance.tileSize;
            int minX = Mathf.FloorToInt((transform.position.x - ExplosionRadius) / tileSize.x);
            int maxX = Mathf.FloorToInt((transform.position.x + ExplosionRadius) / tileSize.x);
            int minY = Mathf.FloorToInt((transform.position.y - ExplosionRadius) / tileSize.y);
            int maxY = Mathf.FloorToInt((transform.position.y + ExplosionRadius) / tileSize.y);

            HashSet<BuildingLogic> damagedBuildings = new HashSet<BuildingLogic>();
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
                                damagedBuildings.Add(building);
                            }
                        }
                    }
                }
            }

            foreach (var building in damagedBuildings)
            {
                float dist = Vector2.Distance(transform.position, building.transform.position);
                if (dist <= ExplosionRadius)
                {
                    building.TakeDamage(Damage);
                }
            }
        }

        // Spawn Visual Effect
        GameObject visualObj = new GameObject("ExplosionVisual");
        visualObj.transform.position = transform.position;
        ExplosionVisual visual = visualObj.AddComponent<ExplosionVisual>();
        visual.Initialize(ExplosionRadius, Color.red); // Red for enemy

        Destroy(gameObject);
    }
}
