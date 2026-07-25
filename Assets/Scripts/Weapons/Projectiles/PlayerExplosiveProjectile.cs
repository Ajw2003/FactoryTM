using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
namespace Weapons
{
    using System.Collections.Generic;
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
            
            // Check active enemies (Cartel members)
            for (int i = GameManager.Instance.ActiveEnemies.Count - 1; i >= 0; i--)
            {
                CartelMember currentEnemy = GameManager.Instance.ActiveEnemies[i];
                if (currentEnemy == null) continue;
                Rectangle2D enemyBox = currentEnemy.GetBoundingBox();
    
                if (Rectangle2D.CheckCollision(bulletBox, enemyBox))
                {
                    hit = true;
                    break; 
                }
            }
    
            // Check enemy-owned buildings
            if (!hit && PlacementManager.HasInstance && GridManager.Instance != null)
            {
                Vector2 tileSize = GridManager.Instance.TileSize;
                int minX = Mathf.FloorToInt((transform.position.x - width / 2f) / tileSize.x);
                int maxX = Mathf.FloorToInt((transform.position.x + width / 2f) / tileSize.x);
                int minY = Mathf.FloorToInt((transform.position.y - height / 2f) / tileSize.y);
                int maxY = Mathf.FloorToInt((transform.position.y + height / 2f) / tileSize.y);
    
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
                                if (building != null && building.IsAlive && building.IsEnemyOwned)
                                {
                                    Vector3 cellWorldPos = GridManager.Instance.CellToWorldConversion(cell);
                                    Rectangle2D cellBox = TwoDCollision.CreateFromRotated(cellWorldPos.x, cellWorldPos.y, 1f, 1f, 0f);
                                    if (Rectangle2D.CheckCollision(bulletBox, cellBox))
                                    {
                                        hit = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (hit) break;
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
    
            // Deal damage to all enemy-owned buildings within ExplosionRadius
            if (PlacementManager.HasInstance && GridManager.Instance != null)
            {
                Vector2 tileSize = GridManager.Instance.TileSize;
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
                                if (building != null && building.IsAlive && building.IsEnemyOwned)
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
            visual.Initialize(ExplosionRadius, new Color(1f, 0.5f, 0f)); // Orange for player
    
            Destroy(gameObject);
        }
    }
    
}


