using System;
using UnityEngine;

public class BaseWeapon : MonoBehaviour
{
    
    private float fireRate;
    private float bulletSize;
    private float bulletSpread;
    private float bulletSpeed;
    
    private int bulletDamage;
    private int magazineSize;
    private int bulletsFired;
    private int bulletRange;
    private int burstSize;
    
    private GameObject bulletPrefab;

    private WeaponType weaponType;
    
    private AudioClip fireSound;

    private Camera cam;
    
    public WeaponStats Stats;
    
    public Transform firePoint;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = GameManager.Instance.mainCamera;
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
            //fireSound = Stats.bulletSound;
            burstSize = Stats.burstSize;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(2))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        switch (weaponType)
        {
            case WeaponType.Automatic :
                for (int i = 0; i < magazineSize; i++)
                {
                    SpawnBullet();
                }
                break;
            case WeaponType.Burst :
                for (int i = 0; i < burstSize; i++)
                {
                    SpawnBullet();
                }
                break;
            case WeaponType.Explosive :
                //explosive code with sphere cast for aoe damage
                {
                    SpawnBullet();
                }
                break;
            case WeaponType.SemiAutomatic:
                {
                    SpawnBullet();
                }
                break;
        }
    }

    private void SpawnBullet()
    {
        Vector2 target = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        
        if (bullet.TryGetComponent<BaseProjectile>(out var projectile))
        {
            projectile.Initialize(target, bulletSpeed);
        }
    }
}
