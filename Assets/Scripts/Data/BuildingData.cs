using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings
{
    [System.Serializable]
    public class BuildingUnlockCondition
    {
        public enum ConditionType
        {
            None,
            CurrencyReached,
            TotalZonesUnlocked,
            BuildingOwnedCount,
            BuildingPlacedCount
        }

        public ConditionType type;
        public float targetValue;
        public BuildingData requiredBuilding;
        [TextArea(2, 5)]
        public string conditionDescription;

        public bool IsMet()
        {
            switch (type)
            {
                case ConditionType.CurrencyReached:
                    if (CurrencyManager.Instance != null)
                    {
                        return CurrencyManager.Instance.currentCurrencyValue >= targetValue;
                    }
                    return false;

                case ConditionType.TotalZonesUnlocked:
                    if (ZoneManager.Instance != null)
                    {
                        return ZoneManager.Instance.UnlockedZonesCount >= targetValue;
                    }
                    return false;

                case ConditionType.BuildingOwnedCount:
                    if (InventoryManager.Instance != null && requiredBuilding != null)
                    {
                        InventoryManager.InventoryItem item = InventoryManager.Instance.items.Find(i => i.data == requiredBuilding);
                        int count = item != null ? item.count : 0;
                        return count >= targetValue;
                    }
                    return false;

                case ConditionType.BuildingPlacedCount:
                    if (BuildingManager.Instance != null && requiredBuilding != null)
                    {
                        return BuildingManager.Instance.GetBuildingCount(requiredBuilding) >= targetValue;
                    }
                    return false;

                default:
                    return true;
            }
        }
    }

    [CreateAssetMenu(fileName = "New Building", menuName = "Construction/Building")]
    public class BuildingData : ScriptableObject
    {
        public string buildingName;
        public Sprite icon; // New icon field
        [TextArea(3, 10)]
        public string description;
        public TileBase[] rotatedTiles; // 0:Right, 1:Down, 2:Left, 3:Up
        public float proccessingSpeed = 2.0f;
        public GameObject itemPrefab; // The "Resource" it creates
        public float cost;
        public BuildingType type;
        public int maxHealth = 100;
        public Vector2Int size = new Vector2Int(1, 1);

        [Header("Combat & Spawner Settings")]
        public int damage = 10;
        public float bulletSpeed = 15f;
        public float fireRate = 1.0f;
        public int bulletsFired = 1;
        public float bulletSpread = 0f;
        public WeaponType weaponType = WeaponType.Automatic;
        public int spawnLimit = 5;
        public float spawnCooldown = 8f;
        public int maxConcurrentEnemies = 3;
        public float activationRange = 9f;

        [Header("Unlock Requirements")]
        public List<BuildingUnlockCondition> unlockConditions = new List<BuildingUnlockCondition>();

        [System.NonSerialized]
        private bool _isUnlockedCached = false;

        private void OnEnable()
        {
            _isUnlockedCached = false;
        }

        public bool IsUnlocked()
        {
            if (_isUnlockedCached) return true;

            if (unlockConditions == null || unlockConditions.Count == 0)
            {
                _isUnlockedCached = true;
                return true;
            }

            foreach (var condition in unlockConditions)
            {
                if (!condition.IsMet())
                {
                    return false;
                }
            }

            _isUnlockedCached = true;
            return true;
        }

        /// <summary>Force-mark this building as unlocked (called when researched via an UpgradeDefinition).</summary>
        public void ForceUnlock()
        {
            _isUnlockedCached = true;
        }

        public string GetUnlockRequirementsText()
        {
            if (IsUnlocked() || unlockConditions == null || unlockConditions.Count == 0)
            {
                return "";
            }

            List<string> requirements = new List<string>();
            foreach (var condition in unlockConditions)
            {
                if (!condition.IsMet())
                {
                    if (!string.IsNullOrEmpty(condition.conditionDescription))
                    {
                        requirements.Add(condition.conditionDescription);
                    }
                    else
                    {
                        requirements.Add(GetDefaultDescription(condition));
                    }
                }
            }
            return string.Join("\n", requirements);
        }

        private string GetDefaultDescription(BuildingUnlockCondition condition)
        {
            switch (condition.type)
            {
                case BuildingUnlockCondition.ConditionType.CurrencyReached:
                    return $"Reach ${condition.targetValue}";
                case BuildingUnlockCondition.ConditionType.TotalZonesUnlocked:
                    return $"Unlock {condition.targetValue} zones";
                case BuildingUnlockCondition.ConditionType.BuildingOwnedCount:
                    return $"Own {condition.targetValue}x {condition.requiredBuilding.buildingName}";
                case BuildingUnlockCondition.ConditionType.BuildingPlacedCount:
                    return $"Place {condition.targetValue}x {condition.requiredBuilding.buildingName}";
                default:
                    return "Locked";
            }
        }
    }

    public enum BuildingType
    {
        Chest,
        Miner,
        Seller,
        Conveyor,
        Furnace,
        Wall,
        Turret
    }
}


