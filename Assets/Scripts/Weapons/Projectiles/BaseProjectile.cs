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
    public bool isEnemy = false;

    private void OnEnable()
    {
        CancelInvoke(nameof(Despawn));
        Invoke(nameof(Despawn), despawnTime);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(Despawn));
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

    public virtual void InitializeExplosive(float radius)
    {
        // Overridden by explosive projectiles
    }

    private void Update()
    {
        transform.Translate(direction * (speed * Time.deltaTime), Space.World);
        CheckForCollisions();
    }
    
    public void Despawn()
    {
        PooledObject pooled = GetComponent<PooledObject>();
        if (pooled != null)
        {
            pooled.ReturnToPool();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public virtual void CheckForCollisions()
    {
        float angleRadians = transform.eulerAngles.z * Mathf.Deg2Rad;
        Rectangle2D bulletBox = TwoDCollision.CreateFromRotated(
            transform.position.x, transform.position.y, width, height, angleRadians);

        if (GridManager.Instance == null) return;
        Vector2 tileSize = GridManager.Instance.tileSize;
        int minX = Mathf.FloorToInt((transform.position.x - width / 2f) / tileSize.x);
        int maxX = Mathf.FloorToInt((transform.position.x + width / 2f) / tileSize.x);
        int minY = Mathf.FloorToInt((transform.position.y - height / 2f) / tileSize.y);
        int maxY = Mathf.FloorToInt((transform.position.y + height / 2f) / tileSize.y);

        if (isEnemy)
        {
            // Collide with player
            if (PlayerController.Instance != null && PlayerController.Instance.gameObject.activeInHierarchy)
            {
                Rectangle2D playerBox = PlayerController.Instance.GetBoundingBox();
                if (Rectangle2D.CheckCollision(bulletBox, playerBox))
                {
                    PlayerController.Instance.TakeDamage(Damage);
                    Despawn();
                    return;
                }
            }

            // Collide with player-owned buildings (where !building.isEnemyOwned and not conveyor)
            if (PlacementManager.HasInstance)
            {
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
                                    Vector3 cellWorldPos = GridManager.Instance.CellToWorldConversion(cell);
                                    Rectangle2D cellBox = TwoDCollision.CreateFromRotated(cellWorldPos.x, cellWorldPos.y, 1f, 1f, 0f);
                                    if (Rectangle2D.CheckCollision(bulletBox, cellBox))
                                    {
                                        building.TakeDamage(Damage);
                                        Despawn();
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        else
        {
            // Collide with active enemies (Cartel members)
            for (int i = GameManager.Instance.ActiveEnemies.Count - 1; i >= 0; i--)
            {
                CartelMember currentEnemy = GameManager.Instance.ActiveEnemies[i];
                if (currentEnemy == null) continue;

                Rectangle2D enemyBox = currentEnemy.GetBoundingBox();

                if (Rectangle2D.CheckCollision(bulletBox, enemyBox))
                {
                    currentEnemy.TakeDamage(Damage);
                    Despawn();
                    return; 
                }
            }

            // Collide with enemy-owned buildings
            if (PlacementManager.HasInstance)
            {
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
                                if (building != null && building.Health > 0 && building.isEnemyOwned)
                                {
                                    Vector3 cellWorldPos = GridManager.Instance.CellToWorldConversion(cell);
                                    Rectangle2D cellBox = TwoDCollision.CreateFromRotated(cellWorldPos.x, cellWorldPos.y, 1f, 1f, 0f);
                                    if (Rectangle2D.CheckCollision(bulletBox, cellBox))
                                    {
                                        building.TakeDamage(Damage);
                                        Despawn();
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
