using System.Collections.Generic;
using UnityEngine;

public class TurretLogic : BuildingLogic
{
    public float targetRange = 10f;
    [SerializeField] private TurretWeapon turretWeapon;

    public override void Setup(Buildings.BuildingData buildingData, Vector2Int cell)
    {
        base.Setup(buildingData, cell);
        Health = Mathf.RoundToInt(data.maxHealth * GetTierMultiplier());
        
        if (turretWeapon == null)
        {
            turretWeapon = GetComponentInChildren<TurretWeapon>();
            if (turretWeapon == null)
            {
                turretWeapon = gameObject.AddComponent<TurretWeapon>();
                // Configure it using BuildingData
                float fRate = data.proccessingSpeed > 0 ? data.proccessingSpeed : 1f;
                turretWeapon.SetupWeapon(data.itemPrefab, fRate, 10, 15f);
            }
        }
        
        if (turretWeapon != null)
        {
            turretWeapon.ApplyTierMultiplier(GetTierMultiplier());
        }
    }

    public override void PerformAction()
    {
        // autonomous targeting logic
        if (turretWeapon != null)
        {
            CartelMember target = FindClosestEnemy();
            if (target != null)
            {
                turretWeapon.target = target.transform.position;
                turretWeapon.hasTarget = true;
            }
            else
            {
                // no target, stop shooting
                turretWeapon.hasTarget = false;
            }
        }
    }

    private void Update()
    {
        PerformAction(); // continuously scan and update target
    }

    private CartelMember FindClosestEnemy()
    {
        CartelMember closestEnemy = null;
        float minDistance = float.MaxValue;

        // Ensure GameManager and ActiveEnemies exist
        if (GameManager.HasInstance && GameManager.Instance.ActiveEnemies != null)
        {
            foreach (var enemy in GameManager.Instance.ActiveEnemies)
            {
                if (enemy == null) continue;

                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance <= targetRange && distance < minDistance)
                {
                    minDistance = distance;
                    closestEnemy = enemy;
                }
            }
        }

        return closestEnemy;
    }
}
