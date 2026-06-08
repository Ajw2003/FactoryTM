using System.Collections.Generic;
using Buildings;
using UnityEngine;
using Singleton;

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
        // Dynamically load all upgrade definitions inside "Assets/Resources/Upgrades/"
        UpgradeDefinition[] loaded = Resources.LoadAll<UpgradeDefinition>("Upgrades");
        allUpgrades.Clear();
        foreach (var def in loaded)
        {
            def.isResearched = false; // Reset state on load
            allUpgrades.Add(def);
        }
        
        Debug.Log($"UpgradeManager: Loaded {allUpgrades.Count} upgrades from Resources.");

        if (allUpgrades.Count == 0)
        {
            GenerateDefaultUpgrades();
        }
    }

    private void GenerateDefaultUpgrades()
    {
        Debug.Log("UpgradeManager: No custom upgrade definitions found. Generating defaults...");

        // Load resources
        BuildingData[] buildings = Resources.LoadAll<BuildingData>("BuildingData");
        WeaponStats[] weapons = Resources.LoadAll<WeaponStats>("Weapons");

        // 1. Generate Building Upgrades
        foreach (var b in buildings)
        {
            if (b == null) continue;
            UpgradeDefinition def = ScriptableObject.CreateInstance<UpgradeDefinition>();
            def.upgradeId = "unlock_" + b.name.ToLower();
            def.upgradeName = "Unlock " + b.buildingName;
            def.description = $"Research technology to enable building {b.buildingName}. Available to build via docked store.";
            def.type = UpgradeType.Building;
            def.buildingToUnlock = b;
            def.costInShop = 100f;
            allUpgrades.Add(def);
        }

        // 2. Generate Weapon Upgrades
        foreach (var w in weapons)
        {
            if (w == null || w.name.Contains("Enemy")) continue; // Skip enemy weapons
            UpgradeDefinition def = ScriptableObject.CreateInstance<UpgradeDefinition>();
            def.upgradeId = "weapon_" + w.name.ToLower();
            def.upgradeName = w.name + " Weapon";
            def.description = $"Deploy the {w.name} weapon system. Swaps your primary weapon.";
            def.type = UpgradeType.Weapon;
            def.weaponToUnlock = w;
            def.costInShop = 150f;
            allUpgrades.Add(def);
        }

        // 3. Generate Armor Upgrades
        {
            UpgradeDefinition def1 = ScriptableObject.CreateInstance<UpgradeDefinition>();
            def1.upgradeId = "armor_mk1";
            def1.upgradeName = "Heavy Plating Mk1";
            def1.description = "Increases damage reduction by 15% and grants +2 Max HP.";
            def1.type = UpgradeType.Armor;
            def1.armorPercentBoost = 0.15f;
            def1.maxHealthBoost = 2;
            def1.costInShop = 80f;
            allUpgrades.Add(def1);

            UpgradeDefinition def2 = ScriptableObject.CreateInstance<UpgradeDefinition>();
            def2.upgradeId = "armor_mk2";
            def2.upgradeName = "Titanium Plating Mk2";
            def2.description = "Increases damage reduction by 25% and grants +4 Max HP.";
            def2.type = UpgradeType.Armor;
            def2.armorPercentBoost = 0.25f;
            def2.maxHealthBoost = 4;
            def2.costInShop = 160f;
            allUpgrades.Add(def2);
        }

        // 4. Generate Health Pack Upgrade
        {
            UpgradeDefinition def = ScriptableObject.CreateInstance<UpgradeDefinition>();
            def.upgradeId = "health_pack_upgrade";
            def.upgradeName = "Emergency Medkits";
            def.description = "Receive 2 emergency medkits immediately. Allows buying medkits in the shop. Press TAB to heal.";
            def.type = UpgradeType.HealthPack;
            def.costInShop = 30f;
            allUpgrades.Add(def);
        }

        Debug.Log($"UpgradeManager: Generated {allUpgrades.Count} default upgrades dynamically.");
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
        // Pause timescale
        Time.timeScale = 0f;

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

        switch (upgrade.type)
        {
            case UpgradeType.Armor:
                // Apply permanent armor / health boost immediately on research
                if (PlayerController.Instance != null)
                {
                    PlayerController.Instance.damageReductionFactor = Mathf.Clamp01(
                        PlayerController.Instance.damageReductionFactor + upgrade.armorPercentBoost);
                    PlayerController.Instance.maxHealth += upgrade.maxHealthBoost;
                    PlayerController.Instance.Health = Mathf.Min(
                        PlayerController.Instance.maxHealth,
                        PlayerController.Instance.Health + upgrade.maxHealthBoost);
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
        }

        onUpgradesChanged?.Invoke();
        Debug.Log($"Upgrade Researched: {upgrade.upgradeName}. Now available in the Shop!");

        // Resume and start next day
        DayNightManager.Instance.StartNextDay();
    }
}
