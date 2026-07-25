using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
namespace Managers
{
    using System.Collections.Generic;
    using Singleton;
    using UnityEngine;
    
    public class BuildingManager : SingletonBase<BuildingManager>
    {
        private List<BuildingLogic> buildings = new List<BuildingLogic>();
        public List<BuildingLogic> Buildings => buildings;
    
        private float lastDamageNotificationTime = -999f;
        [SerializeField] private float damageNotificationCooldown = 10f;
    
    
        protected override void Awake()
        {
            persistBetweenScenes = false;
            base.Awake();
        }
    
        public void NotifyBuildingDamaged()
        {
            if (Time.time - lastDamageNotificationTime >= damageNotificationCooldown)
            {
                lastDamageNotificationTime = Time.time;
                if (UiManager.HasInstance)
                {
                    UiManager.Instance.ShowBuildingDamageAlert();
                }
            }
        }
    
        public void RegisterBuilding(BuildingLogic building)
        {
            buildings.Add(building);
        }
    
        public void UnregisterBuilding(BuildingLogic building)
        {
            buildings.Remove(building);
        }
    
        public int GetBuildingCount(Buildings.BuildingData data)
        {
            int count = 0;
            for (int i = 0; i < buildings.Count; i++)
            {
                if (buildings[i] != null && buildings[i].data == data)
                {
                    count++;
                }
            }
            return count;
        }
    
        private void Update()
        {
            for (int i = 0; i < buildings.Count; i++)
            {
                buildings[i].PerformAction();
            }
        }
    }
    
}


