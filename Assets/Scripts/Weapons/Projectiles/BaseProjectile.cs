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
        Destroy(gameObject);
    }
    
    public virtual void CheckForCollisions()
    {
        float angleRadians = transform.eulerAngles.z * Mathf.Deg2Rad;
        Rectangle2D bulletBox = TwoDCollision.CreateFromRotated(
            transform.position.x, transform.position.y, width, height, angleRadians);

        if (GridManager.Instance == null) return;
        Vector2Int cell = GridManager.Instance.WorldToCellConversion(transform.position);

        if (isEnemy)
        {
            // Collide with player
            if (PlayerController.Instance != null && PlayerController.Instance.gameObject.activeInHierarchy)
            {
                Rectangle2D playerBox = PlayerController.Instance.GetBoundingBox();
                if (Rectangle2D.CheckCollision(bulletBox, playerBox))
                {
                    PlayerController.Instance.TakeDamage(Damage);
                    Destroy(gameObject);
                    return;
                }
            }

            // Collide with player-owned buildings (where !building.isEnemyOwned and not conveyor)
            if (PlacementManager.HasInstance)
            {
                var activeBuildings = PlacementManager.Instance.GetActiveBuildings();
                if (activeBuildings.TryGetValue(cell, out GameObject buildingObj))
                {
                    if (buildingObj != null)
                    {
                        BuildingLogic building = buildingObj.GetComponent<BuildingLogic>();
                        if (building != null && building.Health > 0 && !building.isEnemyOwned && building.data.type != Buildings.BuildingType.Conveyor)
                        {
                            building.TakeDamage(Damage);
                            Destroy(gameObject);
                            return;
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
                    Destroy(gameObject);
                    return; 
                }
            }

            // Collide with enemy-owned buildings
            if (PlacementManager.HasInstance)
            {
                var activeBuildings = PlacementManager.Instance.GetActiveBuildings();
                if (activeBuildings.TryGetValue(cell, out GameObject buildingObj))
                {
                    if (buildingObj != null)
                    {
                        BuildingLogic building = buildingObj.GetComponent<BuildingLogic>();
                        if (building != null && building.Health > 0 && building.isEnemyOwned)
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
