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
    using UnityEngine.InputSystem.LowLevel;
    
    public class PlayerWeapon : BaseWeapon
    {
        
        private Camera cam;
        private AmmoUI ammoUI;
        
        private float lastNoAmmoWarningTime = -999f;
        private float lastNoReserveWarningTime = -999f;
    
        public static event System.Action OnPlayerShoot;
    
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
    
            if (PlayerController.Instance == null || PlayerController.Instance.currentMode != PlayerController.PlayerMode.Combat) return;
    
            if (PlayerController.Instance.StateMachine.CurrentState == PlayerController.Instance.StateMachine.storeState ||
                PlayerController.Instance.StateMachine.CurrentState == PlayerController.Instance.StateMachine.buildingUiState ||
                PlayerController.Instance.StateMachine.CurrentState == PlayerController.Instance.StateMachine.deadState)
            {
                return;
            }
    
            if (cam == null) cam = Camera.main;
            if (cam != null)
            {
                target = cam.ScreenToWorldPoint(Input.mousePosition);
            }
    
            if (Stats == null) return;
    
            // Manual reload
            if (Input.GetKeyDown(KeyCode.R) && !isReloading && roundsLeft < magazineSize)
            {
                int reserve = PlayerController.Instance != null ? PlayerController.Instance.ammoReserve : 0;
                if (reserve > 0)
                {
                    StartCoroutine(Reload());
                }
                else
                {
                    Debug.Log("No ammo reserve left to reload!");
                    if (Time.time - lastNoReserveWarningTime > 1.5f)
                    {
                        lastNoReserveWarningTime = Time.time;
                        if (UiManager.HasInstance) UiManager.Instance.ShowAmmoAlert("NO RESERVE AMMO!", true);
                    }
                }
            }
    
            if (weaponType == WeaponType.Automatic)
            {
                if (Input.GetMouseButton(0))
                {
                    CheckAndWarnAmmo();
                    Shoot();
                    UpdateAmmoUI();
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    CheckAndWarnAmmo();
                    Shoot();
                    UpdateAmmoUI();
                }
            }
        }
    
        protected override void OnDisable()
        {
            isReloading = false;
            canFire = true;
            if (ammoUI != null) ammoUI.SetReloading(false);
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
    
        public override void Shoot()
        {
            if (isReloading) return;
    
            if (roundsLeft <= 0)
            {
                int reserve = PlayerController.Instance != null ? PlayerController.Instance.ammoReserve : 0;
                if (reserve <= 0)
                {
                    canFire = false;
                    return;
                }
    
                canFire = false;
                StartCoroutine(Reload());
                return;
            }
    
            base.Shoot();
            OnPlayerShoot?.Invoke();
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
    
}


