using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
using UnityEngine;
using Buildings;

public enum UpgradeType
{
    Building,
    Weapon,
    Armor,
    HealthPack,
    Ammo,
    IncomeBoost,
    RaidReduction,
    DodgeRoll,
    StaminaBoost,
    ZoneExpansion,
    BuildingTierUp
}

[CreateAssetMenu(fileName = "UpgradeDefinition", menuName = "Scriptable Objects/UpgradeDefinition")]
public class UpgradeDefinition : ScriptableObject
{
    [Header("General Settings")]
    public string upgradeId;
    public string upgradeName;
    [TextArea(3, 10)]
    public string description;
    public Sprite icon;
    public UpgradeType type;
    public float costInShop = 50f;

    [Header("Building Upgrade Settings")]
    public BuildingData buildingToUnlock;

    [Header("Weapon Upgrade Settings")]
    public WeaponStats weaponToUnlock;

    [Header("Armor / Health Boost Settings")]
    public float armorPercentBoost = 0f; // E.g., 0.15f for 15% damage reduction
    public int maxHealthBoost = 0;

    [Header("Unlock Settings")]
    [Tooltip("If true, this upgrade will be available in the shop from the very start of the game, without needing to be researched.")]
    public bool startsUnlocked = false;
    [Tooltip("If true, this upgrade will be added to the shop for purchase after it is researched/unlocked.")]
    public bool addToShopAfterUnlock = true;

    [Header("Status (Runtime Only)")]
    public bool isResearched = false;
}

