using UnityEngine;
using UnityEditor;
using Buildings;

public static class UpgradeAssetGenerator
{
    [MenuItem("Tools/Generate Upgrade Assets")]
    public static void Generate()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Upgrades"))
            AssetDatabase.CreateFolder("Assets/Resources", "Upgrades");

        // Base upgrades
        CreateUpgrade("armor_mk1", "Heavy Plating Mk1", "Increases damage reduction by 15% and grants +2 Max HP.", UpgradeType.Armor, 80f, true, false, 0.15f, 2);
        CreateUpgrade("armor_mk2", "Titanium Plating Mk2", "Increases damage reduction by 25% and grants +4 Max HP.", UpgradeType.Armor, 160f, true, false, 0.25f, 4);
        CreateUpgrade("health_pack_upgrade", "Emergency Medkits", "Receive 2 emergency medkits immediately. Allows buying medkits in the shop. Press H to heal.", UpgradeType.HealthPack, 30f, true, false, 0, 0);
        CreateUpgrade("ammo_upgrade", "Ammo Reserves", "Receive 90 spare rounds immediately. Allows purchasing ammo packs (30 rounds) in the shop.", UpgradeType.Ammo, 15f, true, false, 0, 0);
        CreateUpgrade("tax_loophole", "Taxloophole", "Exploit financial technicalities to increase every dollar earned by +25%. (Stackable)", UpgradeType.IncomeBoost, 120f, false, false, 0, 0);
        CreateUpgrade("border_patrol", "BorderPatrol", "Bolster off-screen security to reduce the number of enemies spawned per wave by 2.", UpgradeType.RaidReduction, 150f, false, false, 0, 0);
        CreateUpgrade("souls_like", "SoulsLike?", "Unlock a rapid tile-based dodge roll. Press SPACE while moving or holding a direction to evade.", UpgradeType.DodgeRoll, 200f, false, false, 0, 0);
        CreateUpgrade("stamina_boost", "Last Longer", "Condition your body for intense combat. Increases maximum stamina by +50. (Stackable)", UpgradeType.StaminaBoost, 140f, false, false, 0, 0);

        // Building upgrades
        BuildingData[] buildings = Resources.LoadAll<BuildingData>("BuildingData");
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
            def.addToShopAfterUnlock = true;

            SaveAsset(def, def.upgradeId);
        }

        // Weapon upgrades
        WeaponStats[] weapons = Resources.LoadAll<WeaponStats>("Weapons");
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
            def.addToShopAfterUnlock = true;

            SaveAsset(def, def.upgradeId);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Upgrades generated successfully!");
    }

    private static void CreateUpgrade(string id, string name, string desc, UpgradeType type, float cost, bool addToShop, bool startsUnlocked, float armorBoost, int hpBoost)
    {
        UpgradeDefinition def = ScriptableObject.CreateInstance<UpgradeDefinition>();
        def.upgradeId = id;
        def.upgradeName = name;
        def.description = desc;
        def.type = type;
        def.costInShop = cost;
        def.addToShopAfterUnlock = addToShop;
        def.startsUnlocked = startsUnlocked;
        def.armorPercentBoost = armorBoost;
        def.maxHealthBoost = hpBoost;
        
        SaveAsset(def, id);
    }

    private static void SaveAsset(UpgradeDefinition def, string id)
    {
        // check if any existing upgrade has this ID
        string[] guids = AssetDatabase.FindAssets("t:UpgradeDefinition", new[] { "Assets/Resources/Upgrades" });
        foreach (string guid in guids)
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            UpgradeDefinition existing = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(p);
            if (existing != null && existing.upgradeId == id)
            {
                return; // already exists
            }
        }
        
        string path = $"Assets/Resources/Upgrades/{id}.asset";
        AssetDatabase.CreateAsset(def, path);
    }
}
