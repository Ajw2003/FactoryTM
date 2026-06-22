using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class PlayerWeapon : BaseWeapon
{
    
    private Camera cam;
    private AmmoUI ammoUI;
    
    private float lastNoAmmoWarningTime = -999f;
    private float lastNoReserveWarningTime = -999f;

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

    public override void ApplyStats()
    {
        base.ApplyStats();
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
       
        if (PauseManager.IsPaused) return;

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
                CheckAndWarnAmmo();
                Shoot();
                UpdateAmmoUI();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(2))
            {
                CheckAndWarnAmmo();
                Shoot();
                UpdateAmmoUI();
            }
        }
    }

    private void CheckAndWarnAmmo()
    {
        if (isReloading) return;
        if (roundsLeft <= 0)
        {
            int reserve = PlayerController.Instance != null ? PlayerController.Instance.ammoReserve : 0;
            if (reserve <= 0)
            {
                if (Time.time - lastNoReserveWarningTime > 1.5f)
                {
                    lastNoReserveWarningTime = Time.time;
                    if (UiManager.HasInstance) UiManager.Instance.ShowAmmoAlert("NO RESERVE AMMO!", true);
                }
            }
            else
            {
                if (Time.time - lastNoAmmoWarningTime > 1.5f)
                {
                    lastNoAmmoWarningTime = Time.time;
                    if (UiManager.HasInstance) UiManager.Instance.ShowAmmoAlert("RELOADING...", false);
                }
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
            if (Time.time - lastNoReserveWarningTime > 1.5f)
            {
                lastNoReserveWarningTime = Time.time;
                if (UiManager.HasInstance) UiManager.Instance.ShowAmmoAlert("NO RESERVE AMMO!", true);
            }
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
