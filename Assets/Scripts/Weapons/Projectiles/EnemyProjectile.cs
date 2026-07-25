using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
namespace Weapons
{
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
                return;
            }
    
            // 4. Check for building collision using dictionary
            if (PlacementManager.HasInstance && GridManager.Instance != null)
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
                                if (building != null && building.IsAlive && !building.IsEnemyOwned && building.data.type != Buildings.BuildingType.Conveyor)
                                {
                                    Vector3 cellWorldPos = GridManager.Instance.CellToWorldConversion(cell);
                                    Rectangle2D cellBox = TwoDCollision.CreateFromRotated(cellWorldPos.x, cellWorldPos.y, 1f, 1f, 0f);
                                    if (Rectangle2D.CheckCollision(bulletBox, cellBox))
                                    {
                                        building.TakeDamage(Damage);
                                        Destroy(gameObject);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    
}


