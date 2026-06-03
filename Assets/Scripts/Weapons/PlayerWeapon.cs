using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class PlayerWeapon : BaseWeapon
{
    
    private Camera cam;
    private AmmoUI ammoUI;

    private void UpdateAmmoUI()
    {
        if (ammoUI != null)
        {
            ammoUI.UpdateAmmo(roundsLeft, magazineSize);
        }
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
        var enumerator = base.Reload();
        if (ammoUI != null) ammoUI.SetReloading(true);
        yield return base.Reload();
        if (ammoUI != null) ammoUI.SetReloading(false);
        UpdateAmmoUI();
        StopCoroutine(enumerator);
        yield return null;
    }
}
