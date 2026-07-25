using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using System.Collections.Generic;
using Buildings;
using Singleton;
using UnityEngine;

namespace Managers
{
    public class UpgradeManager : SingletonBase<UpgradeManager>
    {
        [Header("Upgrade Database")]
        public List<UpgradeDefinition> allUpgrades = new List<UpgradeDefinition>();
    
        [Header("Runtime Status")]
        public List<UpgradeDefinition> researchedUpgrades = new List<UpgradeDefinition>();
        public List<UpgradeDefinition> activeUpgradesInShop = new List<UpgradeDefinition>();
        
        public Dictionary<BuildingType, int> buildingTiers = new Dictionary<BuildingType, int>();

        // Event triggered when a new upgrade is unlocked in the shop
        public delegate void OnUpgradesChanged();
        public event OnUpgradesChanged onUpgradesChanged;

        protected override void Awake()
        {
            persistBetweenScenes = false;
            base.Awake();
            LoadUpgradesFromResources();
        }

        private void LoadUpgradesFromResources()
        {
            // Reset runtime state so a fresh play session starts clean
            researchedUpgrades.Clear();
            activeUpgradesInShop.Clear();
            buildingTiers.Clear();

            // Load any custom UpgradeDefinition assets from "Assets/Resources/Upgrades/"
            UpgradeDefinition[] loaded = Resources.LoadAll<UpgradeDefinition>("Upgrades");
            allUpgrades.Clear();
            foreach (var def in loaded)
            {
                def.isResearched = false; // Reset runtime state on each load
                allUpgrades.Add(def);
            }
            Debug.Log($"UpgradeManager: Loaded {allUpgrades.Count} upgrade asset(s) from Resources.");

            // Auto-unlock any upgrades flagged to start unlocked at game start
            ApplyStartingUnlocks();
        }


        /// <summary>
        /// Marks upgrades with startsUnlocked=true as researched and adds them to the shop
        /// without triggering day progression or the upgrade-selection UI.
        /// </summary>
        private void ApplyStartingUnlocks()
        {
            foreach (var def in allUpgrades)
            {
                if (def.startsUnlocked && !def.isResearched)
                {
                    def.isResearched = true;
                    researchedUpgrades.Add(def);
                    if (def.addToShopAfterUnlock)
                    {
                        activeUpgradesInShop.Add(def);
                    }
                    ApplyUpgradeEffects(def);
                    Debug.Log($"UpgradeManager: Auto-unlocked '{def.upgradeName}' (startsUnlocked=true).");
                }
            }
            // Notify the shop UI once after all starting unlocks are applied
            if (researchedUpgrades.Count > 0)
            {
                onUpgradesChanged?.Invoke();
            }
        }


        public List<UpgradeDefinition> GetRandomUpgradeChoices(int count = 3)
        {
            List<UpgradeDefinition> pool = new List<UpgradeDefinition>();
            foreach (var def in allUpgrades)
            {
                if (!def.isResearched)
                {
                    pool.Add(def);
                }
            }

            List<UpgradeDefinition> choices = new List<UpgradeDefinition>();
            int choicesCount = Mathf.Min(count, pool.Count);
        
            for (int i = 0; i < choicesCount; i++)
            {
                int randomIndex = Random.Range(0, pool.Count);
                choices.Add(pool[randomIndex]);
                pool.RemoveAt(randomIndex);
            }

            return choices;
        }

        public void ShowUpgradeSelection()
        {
            // Populate and open UI
            var choices = GetRandomUpgradeChoices(3);
            if (choices.Count > 0)
            {
                UpgradeUi.Instance.OpenUpgradePanel(choices);
            }
            else
            {
                // No upgrades left to research, skip directly to next day
                Debug.Log("No upgrades left to research. Moving to next day.");
                DayNightManager.Instance.StartNextDay();
            }
        }

        public void UnlockUpgrade(UpgradeDefinition upgrade)
        {
            upgrade.isResearched = true;
            researchedUpgrades.Add(upgrade);
        
            // Add to active shop pool so it is purchasable
            if (upgrade.addToShopAfterUnlock)
            {
                activeUpgradesInShop.Add(upgrade);
            }

            ApplyUpgradeEffects(upgrade);

            onUpgradesChanged?.Invoke();
            if (upgrade.addToShopAfterUnlock)
            {
                Debug.Log($"Upgrade Researched: {upgrade.upgradeName}. Now available in the Shop!");
            }
            else
            {
                Debug.Log($"Upgrade Researched: {upgrade.upgradeName}.");
            }

            // Resume and start next day
            DayNightManager.Instance.StartNextDay();
        }

        /// <summary>
        /// Applies the side-effects of an upgrade (stat boosts, weapon swap, building unlock, etc.)
        /// without touching researched lists, shop lists, events, or day progression.
        /// Called by both UnlockUpgrade (player researches) and ApplyStartingUnlocks (game start).
        /// </summary>
        private void ApplyUpgradeEffects(UpgradeDefinition upgrade)
        {
            switch (upgrade.type)
            {
                case UpgradeType.Armor:
                    // Apply permanent armor / health boost immediately on research
                    if (PlayerController.Instance != null)
                    {
                        PlayerController.Instance.ApplyArmorUpgrade(upgrade.maxHealthBoost, upgrade.armorPercentBoost);


                        Debug.Log($"Applied armor boost: +{upgrade.armorPercentBoost*100}% reduction, +{upgrade.maxHealthBoost} max HP");
                    }
                    break;

                case UpgradeType.HealthPack:
                    // Grant 2 health packs immediately; more can be purchased from the shop
                    if (PlayerController.Instance != null)
                    {
                        PlayerController.Instance.GrantHealthPacks(2);
                        Debug.Log($"Granted 2 health packs. Total: {PlayerController.Instance.HealthPacksCount}");
                    }
                    break;

                case UpgradeType.Ammo:
                    // Grant 90 ammo immediately; more can be purchased from the shop
                    if (PlayerController.Instance != null)
                    {
                        PlayerController.Instance.AddAmmoReserve(90);
                        Debug.Log($"Granted 90 ammo. Total: {PlayerController.Instance.AmmoReserve}");
                    }
                    break;

                case UpgradeType.Weapon:
                    // Swap the player's current weapon stats
                    if (upgrade.weaponToUnlock != null)
                    {
                        PlayerWeapon playerWeapon = FindFirstObjectByType<PlayerWeapon>();
                        if (playerWeapon != null)
                        {
                            playerWeapon.Stats = upgrade.weaponToUnlock;
                            playerWeapon.ApplyStats();
                            Debug.Log($"Swapped player weapon to: {upgrade.weaponToUnlock.name}");
                        }
                    }
                    break;

                case UpgradeType.Building:
                    // Force the BuildingData's unlock cache so it appears in the store immediately
                    if (upgrade.buildingToUnlock != null)
                    {
                        upgrade.buildingToUnlock.ForceUnlock();
                        Debug.Log($"Unlocked building: {upgrade.buildingToUnlock.buildingName}");
                    }
                    break;

                case UpgradeType.IncomeBoost:
                    if (CurrencyManager.Instance != null)
                    {
                        CurrencyManager.Instance.incomeMultiplier += 0.25f; // +25% per upgrade
                        Debug.Log($"Applied Income Boost: New multiplier = {CurrencyManager.Instance.incomeMultiplier}");
                    }
                    break;

                case UpgradeType.RaidReduction:
                    if (DayNightManager.Instance != null)
                    {
                        DayNightManager.Instance.raidEnemyReduction += 2; // -2 enemies per wave per upgrade
                        Debug.Log($"Applied Raid Reduction: Total reduction = {DayNightManager.Instance.raidEnemyReduction}");
                    }
                    break;

                case UpgradeType.DodgeRoll:
                    if (PlayerController.Instance != null)
                    {
                        PlayerController.Instance.UnlockDodgeRoll();
                        Debug.Log("Dodge Roll capability unlocked!");
                    }
                    break;

                case UpgradeType.StaminaBoost:
                    if (PlayerController.Instance != null)
                    {
                        PlayerController.Instance.ApplyStaminaBoost(50f);
                        Debug.Log($"Applied Stamina Boost: New Max Stamina = {PlayerController.Instance.maxStamina}");
                    }
                    break;

                case UpgradeType.BuildingTierUp:
                    if (upgrade.buildingToUnlock != null)
                    {
                        IncreaseBuildingTier(upgrade.buildingToUnlock.type);
                        Debug.Log($"Upgraded tier for building type: {upgrade.buildingToUnlock.type}");
                    }
                    else
                    {
                        // Upgrade all buildings if none specified
                        foreach (BuildingType type in System.Enum.GetValues(typeof(BuildingType)))
                        {
                            IncreaseBuildingTier(type);
                        }
                        Debug.Log("Upgraded tier for ALL buildings.");
                    }
                    break;
            }
        }

        private void IncreaseBuildingTier(BuildingType type)
        {
            if (!buildingTiers.ContainsKey(type))
            {
                buildingTiers[type] = 1;
            }
            if (buildingTiers[type] < 4)
            {
                buildingTiers[type]++;
            }
        }

        public int GetBuildingTier(BuildingType type)
        {
            if (buildingTiers.ContainsKey(type))
            {
                return buildingTiers[type];
            }
            return 1; // Default tier
        }

        public void TriggerUpgradesChanged()
        {
            onUpgradesChanged?.Invoke();
        }
    }
}

