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
    
    protected GameObject bulletPrefab;
    protected WeaponType weaponType;
    protected AudioClip fireSound;
    
    public Vector2 target;

    protected bool canFire = true;
    
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
            roundsLeft = magazineSize;
            reloadSpeed = Stats.reloadSpeed;
            nextTimeToFire = fireRate;
        }
    }

    protected virtual void Update()
    {
        
    }

    public virtual void Shoot()
    {
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
            GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        
            roundsLeft--;
        
            if (bullet.TryGetComponent<BaseProjectile>(out var projectile))
            {
                projectile.Initialize(target, bulletSpeed, bulletDamage);
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
