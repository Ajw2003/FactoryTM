using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
namespace Placeables
{
    using System.Collections.Generic;
    using UnityEngine;
    
    public class TurretLogic : BuildingLogic
    {
        [Header("Ammo Settings")]
        public int ammoRemaining = 30;
        public int maxAmmo = 120;
    
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
                }
            }
            
            if (turretWeapon != null)
            {
                float fRate = data.fireRate > 0 ? data.fireRate : 1f;
                int bullets = data.bulletsFired > 0 ? data.bulletsFired : 1;
                turretWeapon.SetupWeapon(data.itemPrefab, fRate, data.damage, data.bulletSpeed, bullets, data.bulletSpread, data.weaponType);
                turretWeapon.ApplyTierMultiplier(GetTierMultiplier());
                turretWeapon.isEnemyFired = IsEnemyOwned;
            }

            if (IsEnemyOwned)
            {
                targetRange = 8f; // Reduce range of enemy turrets so player can out-range them
            }
        }

        public override void SetEnemyOwned(bool value, EnemyOutpost owningOutpost = null)
        {
            base.SetEnemyOwned(value, owningOutpost);

            // May be called before Setup() has resolved the weapon (enemy spawn path sets
            // ownership first); Setup() applies isEnemyFired itself in that case.
            if (turretWeapon == null) turretWeapon = GetComponentInChildren<TurretWeapon>();
            if (turretWeapon != null) turretWeapon.isEnemyFired = value;
        }
    
        private float targetScanTimer = 0f;
        private const float TARGET_SCAN_INTERVAL = 0.25f;
        private Vector3? currentTargetPos = null;
    
        public override void PerformAction()
        {
            // autonomous targeting logic
            if (turretWeapon != null)
            {
                targetScanTimer -= Time.deltaTime;
                if (targetScanTimer <= 0f)
                {
                    targetScanTimer = TARGET_SCAN_INTERVAL + Random.Range(-0.05f, 0.05f); // Jitter to space out frames
                    currentTargetPos = FindClosestTarget();
                }
    
                if (currentTargetPos.HasValue)
                {
                    turretWeapon.target = currentTargetPos.Value;
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
    
        private Vector3? FindClosestTarget()
        {
            float minDistance = targetRange;
            Vector3? closestTarget = null;
    
            if (IsEnemyOwned)
            {
                // Target player
                if (PlayerController.Instance != null && PlayerController.Instance.IsAlive && PlayerController.Instance.gameObject.activeInHierarchy)
                {
                    float distance = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
                    if (distance <= minDistance)
                    {
                        minDistance = distance;
                        closestTarget = PlayerController.Instance.transform.position;
                    }
                }
    
                // Target player-owned buildings (where !building.IsEnemyOwned and not conveyor)
                if (BuildingManager.HasInstance)
                {
                    foreach (var building in BuildingManager.Instance.Buildings)
                    {
                        if (building != null && building.IsAlive && !building.IsEnemyOwned && building.data.type != Buildings.BuildingType.Conveyor)
                        {
                            float distance = Vector2.Distance(transform.position, building.transform.position);
                            if (distance <= minDistance)
                            {
                                minDistance = distance;
                                closestTarget = building.transform.position;
                            }
                        }
                    }
                }
            }
            else
            {
                // Target closest enemy (CartelMember)
                if (GameManager.HasInstance && GameManager.Instance.ActiveEnemies != null)
                {
                    foreach (var enemy in GameManager.Instance.ActiveEnemies)
                    {
                        if (enemy == null) continue;
    
                        float distance = Vector2.Distance(transform.position, enemy.transform.position);
                        if (distance <= minDistance)
                        {
                            minDistance = distance;
                            closestTarget = enemy.transform.position;
                        }
                    }
                }
            }
    
            return closestTarget;
        }
    }
    
}


