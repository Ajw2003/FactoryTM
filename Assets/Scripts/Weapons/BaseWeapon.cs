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
    using System;
    using System.Collections;
    using UnityEngine;
    using Random = UnityEngine.Random;
    
    public class BaseWeapon : MonoBehaviour
    {
        
        protected float fireRate;
        protected float bulletSize;
        protected float bulletSpread;
        protected float bulletSpeed;
        
        protected int bulletDamage;
        protected int magazineSize;
        protected int bulletsFired;
        protected int bulletRange;
        protected int burstSize;
        protected float nextTimeToFire = 0f;
        protected bool isReloading = false;
        protected int roundsLeft;
        protected int reloadSpeed;
        protected float explosionRadius;
        
        protected GameObject bulletPrefab;
        protected WeaponType weaponType;
        protected AudioClip fireSound;
        
        public Vector2 target;
    
        public bool canFire = true;
        public bool isEnemyFired = false;
        
        public WeaponStats Stats;
        public Transform firePoint;
        
        protected virtual void Start()
        {
            ApplyStats();
        }
    
        public virtual void ApplyStats()
        {
            if (Stats != null)
            {
                fireRate = Stats.fireRate;
                bulletDamage = Stats.bulletDamage;
                bulletSize = Stats.bulletSize;
                bulletSpread = Stats.bulletSpread;
                bulletSpeed = Stats.bulletSpeed;
                magazineSize = Stats.magazineSize;
                bulletsFired = Stats.bulletsFired;
                bulletRange = Stats.bulletRange;
                bulletPrefab = Stats.bulletPrefab;
                weaponType = Stats.weaponType;
                burstSize = Stats.burstSize;
                explosionRadius = Stats.explosionRadius;
                roundsLeft = magazineSize;
                reloadSpeed = Stats.reloadSpeed;
                nextTimeToFire = fireRate;
            }
        }
    
        protected virtual void Update()
        {
            
        }
    
        protected virtual void OnDisable()
        {
            isReloading = false;
            canFire = true;
        }
    
        public virtual void Shoot()
        {
            if (Stats == null) return;
            if (isReloading) return;
            if (isReloading || Time.time < nextTimeToFire) return;
    
            if (roundsLeft <= 0)
            {
                canFire = false;
                StartCoroutine(Reload());
                return;
            }
            nextTimeToFire = Time.time + fireRate;
            switch (weaponType)
            {
                case WeaponType.Automatic :
                    if (canFire)
                    {
                        SpawnBullet();
                    }
                    break;
                case WeaponType.Burst:
                    if (canFire)
                    {
                        StartCoroutine(BurstFire());
                    }
                    break;
                case WeaponType.Explosive:
                case WeaponType.Shotgun:
                    if ( canFire)
                    {
                        SpawnBullet();
                    }
                    break;
                default:
                    if (canFire)
                    {
                        SpawnBullet();
                    }
                    break;
            }
        }
    
        protected virtual IEnumerator BurstFire()
        {
            for (int i = 0; i < burstSize; i++)
            {
                if (roundsLeft <= 0) break;
                SpawnBullet();
                yield return new WaitForSeconds(0.1f); // Small delay between burst rounds
            }
        }
    
        protected virtual void SpawnBullet()
        {
            if (roundsLeft <= 0) return;
            
            for (int i = 0; i < bulletsFired; i++)
            {
                Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
                GameObject bullet = ObjectPoolManager.Instance.GetPooledObject(bulletPrefab, spawnPos, Quaternion.identity);
            
                if (bullet.TryGetComponent<BaseProjectile>(out var projectile))
                { 
                    projectile.isEnemy = isEnemyFired;
                    roundsLeft--;
                    Vector2 actualTarget = target;
                    if (bulletSpread > 0)
                    {
                        float spreadAngle = Random.Range(-bulletSpread, bulletSpread);
                        Vector2 direction = (target - (Vector2)spawnPos).normalized;
                        float cos = Mathf.Cos(spreadAngle * Mathf.Deg2Rad);
                        float sin = Mathf.Sin(spreadAngle * Mathf.Deg2Rad);
                        Vector2 spreadDirection = new Vector2(
                            direction.x * cos - direction.y * sin,
                            direction.x * sin + direction.y * cos
                        );
                        actualTarget = (Vector2)spawnPos + spreadDirection * 10f; // multiply by arbitrary distance so it doesn't just target the unit circle
                    }
                    
                    projectile.Initialize(actualTarget, bulletSpeed, bulletDamage);
                    projectile.InitializeExplosive(explosionRadius);
                }
            }
        }
    
        protected virtual IEnumerator Reload()
        {
            Debug.Log("Reloading");
            isReloading = true;
            
            yield return new WaitForSeconds(reloadSpeed);
            
            roundsLeft = magazineSize;
            isReloading = false;
            canFire = true;
        }
    }
    
}


