using System;
using System.Collections;
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
    private float nextTimeToFire = 0f;
    private bool isReloading = false;
    private int roundsLeft;
    private int reloadSpeed;
    
    private GameObject bulletPrefab;
    private WeaponType weaponType;
    private AudioClip fireSound;
    private Camera cam;
    private AmmoUI ammoUI;
    
    public WeaponStats Stats;
    public Transform firePoint;
    
    public bool isPlayerControlled = true;
    private GameObject owner;

    void Start()
    {
        owner = transform.root.gameObject;
        cam = GameManager.Instance.mainCamera;
        ammoUI = FindFirstObjectByType<AmmoUI>();

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
        }

        UpdateAmmoUI();
    }

    private void UpdateAmmoUI()
    {
        if (ammoUI != null)
        {
            ammoUI.UpdateAmmo(roundsLeft, magazineSize);
        }
    }

    private void Update()
    {
        if (!isPlayerControlled || isReloading) return;

        // Manual reload
        if (Input.GetKeyDown(KeyCode.R) && roundsLeft < magazineSize)
        {
            StartCoroutine(Reload());
            return;
        }

        if (roundsLeft <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        bool canFire = Time.time >= nextTimeToFire;

        if (canFire)
        {
            Vector2 target = cam.ScreenToWorldPoint(Input.mousePosition);
            switch (weaponType)
            {
                case WeaponType.Automatic:
                    if (Input.GetMouseButton(0)) Shoot(target);
                    break;
                case WeaponType.Burst:
                    if (Input.GetMouseButton(0)) Shoot(target);
                    break;
                case WeaponType.Explosive:
                    if (Input.GetMouseButton(0)) Shoot(target);
                    break;
                default:
                    if (Input.GetMouseButtonDown(0)) Shoot(target);
                    break;
            }
        }
    }

    public void Shoot(Vector2 target)
    {
        if (isReloading || roundsLeft <= 0 || Time.time < nextTimeToFire) return;
        
        nextTimeToFire = Time.time + 1f / fireRate;

        if (weaponType == WeaponType.Burst)
        {
            StartCoroutine(BurstFire(target));
        }
        else
        {
            SpawnBullet(target);
        }
    }

    private IEnumerator BurstFire(Vector2 target)
    {
        for (int i = 0; i < burstSize; i++)
        {
            if (roundsLeft <= 0) break;
            SpawnBullet(target);
            yield return new WaitForSeconds(0.1f); // Small delay between burst rounds
        }
    }

    private void SpawnBullet(Vector2 target)
    {
        if (roundsLeft <= 0) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        
        roundsLeft--;
        UpdateAmmoUI();
        
        if (bullet.TryGetComponent<BaseProjectile>(out var projectile))
        {
            projectile.Initialize(target, bulletSpeed, bulletDamage, owner);
        }
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        if (ammoUI != null) ammoUI.SetReloading(true);
        Debug.Log("Reloading...");
        
        yield return new WaitForSeconds(reloadSpeed);
        
        roundsLeft = magazineSize;
        isReloading = false;
        if (ammoUI != null) ammoUI.SetReloading(false);
        UpdateAmmoUI();
        Debug.Log("Reload Complete");
    }
}
