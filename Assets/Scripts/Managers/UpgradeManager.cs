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

            // Load any custom UpgradeDefinition assets from "Assets/Resources/Upgrades/"
            UpgradeDefinition[] loaded = Resources.LoadAll<UpgradeDefinition>("Upgrades");
            allUpgrades.Clear();
            foreach (var def in loaded)
            {
                def.isResearched = false; // Reset runtime state on each load
                allUpgrades.Add(def);
            }
            Debug.Log($"UpgradeManager: Loaded {allUpgrades.Count} upgrade asset(s) from Resources.");

            // If no custom assets exist at all, generate building/weapon defaults too
            if (allUpgrades.Count == 0)
            {
                GenerateBuildingAndWeaponUpgrades();
            }

            // ALWAYS ensure the core researchable upgrades exist in the pool.
            // These are checked by upgradeId so custom assets with matching IDs aren't duplicated.
            EnsureBaseUpgrades();

            // Auto-unlock any upgrades flagged to start unlocked at game start
            ApplyStartingUnlocks();
        }

        /// <summary>
        /// Generates building and weapon upgrade definitions from Resources as a fallback
        /// when no custom UpgradeDefinition assets exist.
        /// </summary>
        private void GenerateBuildingAndWeaponUpgrades()
        {
            Debug.Log("UpgradeManager: No custom assets found. Generating building/weapon upgrade defaults...");

            BuildingData[] buildings = Resources.LoadAll<BuildingData>("BuildingData");
            WeaponStats[] weapons = Resources.LoadAll<WeaponStats>("Weapons");

            foreach (var b in buildings)
            {
                if (b == null) continue;
                UpgradeDefinition def = ScriptableObject.CreateInstance<UpgradeDefinition>();
                def.upgradeId = "unlock_" + b.name.ToLower();
                def.upgradeName = "Unlock " + b.buildingName;
                def.description = $"Research technology to enable building {b.buildingName}. Available to build via docked store.";
                def.type = UpgradeType.Building;
                def.buildingToUnlock = b;
                def.costInShop = b.cost;
                allUpgrades.Add(def);
            }

            foreach (var w in weapons)
            {
                if (w == null || w.name.Contains("Enemy")) continue;
                UpgradeDefinition def = ScriptableObject.CreateInstance<UpgradeDefinition>();
                def.upgradeId = "weapon_" + w.name.ToLower();
                def.upgradeName = w.name + " Weapon";
                def.description = $"Deploy the {w.name} weapon system. Swaps your primary weapon.";
                def.type = UpgradeType.Weapon;
                def.weaponToUnlock = w;
                def.costInShop = w.cost;
                allUpgrades.Add(def);
            }

            Debug.Log($"UpgradeManager: Generated {allUpgrades.Count} building/weapon upgrades.");
        }

        /// <summary>
        /// Guarantees that armor, health pack, and ammo upgrade entries always exist in the pool,
        /// regardless of what custom assets are loaded. Checks by upgradeId to avoid duplicates.
        /// </summary>
        private void EnsureBaseUpgrades()
        {
            if (!HasUpgradeWithId("armor_mk1"))
            {
                UpgradeDefinition def = ScriptableObject.CreateInstance<UpgradeDefinition>();
                def.upgradeId = "armor_mk1";
                def.upgradeName = "Heavy Plating Mk1";
                def.description = "Increases damage reduction by 15% and grants +2 Max HP.";
                def.type = UpgradeType.Armor;
                def.armorPercentBoost = 0.15f;
                def.maxHealthBoost = 2;
                def.costInShop = 80f;
                allUpgrades.Add(def);
            }

            if (!HasUpgradeWithId("armor_mk2"))
            {
                UpgradeDefinition def = ScriptableObject.CreateInstance<UpgradeDefinition>();
                def.upgradeId = "armor_mk2";
                def.upgradeName = "Titanium Plating Mk2";
                def.description = "Increases damage reduction by 25% and grants +4 Max HP.";
                def.type = UpgradeType.Armor;
                def.armorPercentBoost = 0.25f;
                def.maxHealthBoost = 4;
                def.costInShop = 160f;
                allUpgrades.Add(def);
            }

            if (!HasUpgradeWithId("health_pack_upgrade"))
            {
                UpgradeDefinition def = ScriptableObject.CreateInstance<UpgradeDefinition>();
                def.upgradeId = "health_pack_upgrade";
                def.upgradeName = "Emergency Medkits";
                def.description = "Receive 2 emergency medkits immediately. Allows buying medkits in the shop. Press TAB to heal.";
                def.type = UpgradeType.HealthPack;
                def.costInShop = 30f;
                allUpgrades.Add(def);
            }

            if (!HasUpgradeWithId("ammo_upgrade"))
            {
                UpgradeDefinition def = ScriptableObject.CreateInstance<UpgradeDefinition>();
                def.upgradeId = "ammo_upgrade";
                def.upgradeName = "Ammo Reserves";
                def.description = "Receive 90 spare rounds immediately. Allows purchasing ammo packs (30 rounds) in the shop.";
                def.type = UpgradeType.Ammo;
                def.costInShop = 15f;
                allUpgrades.Add(def);
            }

            if (!HasUpgradeWithId("tax_loophole"))
            {
                UpgradeDefinition def = ScriptableObject.CreateInstance<UpgradeDefinition>();
                def.upgradeId = "tax_loophole";
                def.upgradeName = "Taxloophole";
                def.description = "Exploit financial technicalities to increase every dollar earned by +25%. (Stackable)";
                def.type = UpgradeType.IncomeBoost;
                def.costInShop = 120f;
                allUpgrades.Add(def);
            }

            if (!HasUpgradeWithId("border_patrol"))
            {
                UpgradeDefinition def = ScriptableObject.CreateInstance<UpgradeDefinition>();
                def.upgradeId = "border_patrol";
                def.upgradeName = "BorderPatrol";
                def.description = "Bolster off-screen security to reduce the number of enemies spawned per wave by 2.";
                def.type = UpgradeType.RaidReduction;
                def.costInShop = 150f;
                allUpgrades.Add(def);
            }

            if (!HasUpgradeWithId("souls_like"))
            {
                UpgradeDefinition def = ScriptableObject.CreateInstance<UpgradeDefinition>();
                def.upgradeId = "souls_like";
                def.upgradeName = "SoulsLike?";
                def.description = "Unlock a rapid tile-based dodge roll. Press SPACE while moving or holding a direction to evade.";
                def.type = UpgradeType.DodgeRoll;
                def.costInShop = 200f;
                allUpgrades.Add(def);
            }

            if (!HasUpgradeWithId("stamina_boost"))
            {
                UpgradeDefinition def = ScriptableObject.CreateInstance<UpgradeDefinition>();
                def.upgradeId = "stamina_boost";
                def.upgradeName = "Last Longer";
                def.description = "Condition your body for intense combat. Increases maximum stamina by +50. (Stackable)";
                def.type = UpgradeType.StaminaBoost;
                def.costInShop = 140f;
                allUpgrades.Add(def);
            }

            Debug.Log($"UpgradeManager: Pool has {allUpgrades.Count} total upgrades after base-upgrade check.");
        }

        /// <summary>Returns true if allUpgrades already contains an entry with this id.</summary>
        private bool HasUpgradeWithId(string id)
        {
            foreach (var def in allUpgrades)
                if (def != null && def.upgradeId == id) return true;
            return false;
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
                    activeUpgradesInShop.Add(def);
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
            activeUpgradesInShop.Add(upgrade);

            ApplyUpgradeEffects(upgrade);

            onUpgradesChanged?.Invoke();
            Debug.Log($"Upgrade Researched: {upgrade.upgradeName}. Now available in the Shop!");

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
                        PlayerController.Instance.damageReductionFactor = Mathf.Clamp01(PlayerController.Instance.damageReductionFactor + upgrade.armorPercentBoost);
                        PlayerController.Instance.maxHealth += upgrade.maxHealthBoost; 
                        PlayerController.Instance.Health = Mathf.Min(PlayerController.Instance.maxHealth, PlayerController.Instance.Health + upgrade.maxHealthBoost);
                        UiManager.Instance.UpdateHp(PlayerController.Instance.Health, PlayerController.Instance.maxHealth);
                        Debug.Log($"Applied armor boost: +{upgrade.armorPercentBoost*100}% reduction, +{upgrade.maxHealthBoost} max HP");
                    }
                    break;

                case UpgradeType.HealthPack:
                    // Grant 2 health packs immediately; more can be purchased from the shop
                    if (PlayerController.Instance != null)
                    {
                        PlayerController.Instance.healthPacksCount += 2;
                        Debug.Log($"Granted 2 health packs. Total: {PlayerController.Instance.healthPacksCount}");
                    }
                    break;

                case UpgradeType.Ammo:
                    // Grant 90 ammo immediately; more can be purchased from the shop
                    if (PlayerController.Instance != null)
                    {
                        PlayerController.Instance.ammoReserve += 90;
                        Debug.Log($"Granted 90 ammo. Total: {PlayerController.Instance.ammoReserve}");
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
                            // Force re-initialise weapon with new stats via Start()-equivalent
                            playerWeapon.SendMessage("Start", SendMessageOptions.DontRequireReceiver);
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
                        PlayerController.Instance.canDodgeRoll = true;
                        Debug.Log("Dodge Roll capability unlocked!");
                    }
                    break;

                case UpgradeType.StaminaBoost:
                    if (PlayerController.Instance != null)
                    {
                        PlayerController.Instance.maxStamina += 50f;
                        PlayerController.Instance.currentStamina += 50f;
                        UiManager.Instance.UpdateStamina(PlayerController.Instance.currentStamina, PlayerController.Instance.maxStamina);
                        Debug.Log($"Applied Stamina Boost: New Max Stamina = {PlayerController.Instance.maxStamina}");
                    }
                    break;
            }
        }
    }
}
