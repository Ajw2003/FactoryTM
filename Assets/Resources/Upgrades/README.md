# Upgrades Folder

Place `UpgradeDefinition` ScriptableObject assets here for the roguelike research system.

## How to create an upgrade:
1. Right-click in the Project window → **Create → Scriptable Objects → UpgradeDefinition**
2. Save it in this folder (`Assets/Resources/Upgrades/`)
3. Fill out the Inspector fields:
   - **upgradeId**: Unique string ID (e.g. "heavy_armor_mk1")
   - **upgradeName**: Display name shown on the card
   - **description**: Shown on the card in the upgrade selection screen
   - **icon**: Optional sprite icon (shown on the card)
   - **type**: One of `Building`, `Weapon`, `Armor`, `HealthPack`
   - **costInShop**: How much it costs when bought from the store
   - Type-specific fields (see below)

## Upgrade Types:

| Type | Effect on Research | Effect when bought from shop |
|------|-------------------|------------------------------|
| **Building** | Calls `ForceUnlock()` on the `BuildingData` — it now appears enabled in the normal store | Building remains unlocked (no further effect) |
| **Weapon** | Swaps the player's `PlayerWeapon.Stats` to the new `WeaponStats` SO | Swaps weapon again (useful if player wants to change back) |
| **Armor** | Immediately boosts `damageReductionFactor` and `maxHealth` | Smaller incremental boost on re-buy |
| **HealthPack** | Grants 2 health packs immediately. Press **Tab** to use one mid-fight | Grants 1 additional health pack |

## Notes:
- `UpgradeManager.LoadUpgradesFromResources()` uses `Resources.LoadAll<UpgradeDefinition>("Upgrades")` — so every `.asset` file in this folder is automatically included.
- `isResearched` is reset to `false` on each scene load.
