using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class PlayerWeapon : BaseWeapon
{
    
    private Camera cam;
    private AmmoUI ammoUI;

    public void UpdateAmmoUI()
    {
        if (ammoUI != null)
        {
            int reserve = PlayerController.Instance != null ? PlayerController.Instance.ammoReserve : 0;
            ammoUI.UpdateAmmo(roundsLeft, magazineSize, reserve);
        }
    }

    protected override void SpawnBullet()
    {
        base.SpawnBullet();
        UpdateAmmoUI();
    }

    protected override void Start()
    {
        base.Start();
        cam = GameManager.Instance.mainCamera;
        ammoUI = FindFirstObjectByType<AmmoUI>();
        UpdateAmmoUI();
    }

    protected override void Update()
    {
        base.Update();
       
        if (cam == null) cam = Camera.main;
        if (cam == null) return;
            
        target = cam.ScreenToWorldPoint(Input.mousePosition);
        // Manual reload
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Reload());
        }

        if (weaponType == WeaponType.Automatic)
        {
            if (Input.GetMouseButton(2))
            {
                Shoot();
                UpdateAmmoUI();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(2))
            {
                Shoot();
                UpdateAmmoUI();
            }
        }
    }

    protected override IEnumerator Reload()
    {
        if (isReloading) yield break;

        int needed = magazineSize - roundsLeft;
        if (needed <= 0) yield break;

        int reserve = PlayerController.Instance != null ? PlayerController.Instance.ammoReserve : 0;
        if (reserve <= 0)
        {
            Debug.Log("No ammo reserve left to reload!");
            yield break;
        }

        Debug.Log("Reloading player weapon from reserve");
        isReloading = true;
        if (ammoUI != null) ammoUI.SetReloading(true);

        yield return new WaitForSeconds(reloadSpeed);

        int toLoad = Mathf.Min(needed, reserve);
        roundsLeft += toLoad;
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.ammoReserve -= toLoad;
        }

        isReloading = false;
        canFire = true;
        if (ammoUI != null) ammoUI.SetReloading(false);
        UpdateAmmoUI();
    }
}
