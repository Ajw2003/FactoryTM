using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
namespace Weapons
{
    using UnityEngine;
    
    public class TurretWeapon : BaseWeapon
    {
        [SerializeField] private float rotationSpeed = 5f;
    
        public bool hasTarget = false;
    
        protected override void Update()
        {
            base.Update();
    
            TurretLogic turret = GetComponentInParent<TurretLogic>();
            bool hasAmmo = turret == null || turret.isEnemyOwned || turret.ammoRemaining > 0;
            
            if (hasTarget && canFire && roundsLeft > 0 && hasAmmo)
            {
                // Rotate towards target
                Vector2 direction = target - (Vector2)transform.position;
                if (direction.sqrMagnitude > 0.01f)
                {
                    float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) - 90f;
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), rotationSpeed * Time.deltaTime);
                }
    
                Shoot();
            }
        }
    
        protected override void SpawnBullet()
        {
            TurretLogic turret = GetComponentInParent<TurretLogic>();
            if (turret != null && !turret.isEnemyOwned)
            {
                if (turret.ammoRemaining <= 0) return;
                turret.ammoRemaining = Mathf.Max(0, turret.ammoRemaining - 1);
            }
    
            base.SpawnBullet();
        }
    
        public void SetupWeapon(GameObject projectilePrefab, float defaultFireRate = 1f, int defaultDamage = 10, float defaultSpeed = 10f, int defaultBulletsFired = 1, float defaultSpread = 0f, WeaponType defaultWeaponType = WeaponType.Automatic)
        {
            bulletPrefab = projectilePrefab;
            fireRate = defaultFireRate;
            bulletDamage = defaultDamage;
            bulletSpeed = defaultSpeed;
            magazineSize = 99999;
            roundsLeft = magazineSize;
            bulletsFired = defaultBulletsFired;
            bulletSpread = defaultSpread;
            weaponType = defaultWeaponType;
            nextTimeToFire = Time.time + fireRate;
        }
    
        // Apply tier multipliers to weapon stats
        public void ApplyTierMultiplier(float multiplier)
        {
            // Adjust current stats based on multiplier directly
            fireRate /= multiplier; // Fire faster
            bulletDamage = Mathf.RoundToInt(bulletDamage * multiplier); // Do more damage
            nextTimeToFire = Time.time + fireRate;
        }
    }
    
}


