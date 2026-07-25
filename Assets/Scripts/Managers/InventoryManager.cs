using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
namespace Managers
{
    using System.Collections.Generic;
    using Buildings;
    using Singleton;
    using UnityEngine;
    
    public class InventoryManager : SingletonBase<InventoryManager>
    {
        [System.Serializable]
        public class InventoryItem
        {
            public BuildingData data;
            public int count;
        }
    
        public List<InventoryItem> items = new List<InventoryItem>();
    
        protected override void Awake()
        {
            persistBetweenScenes = false;
            base.Awake();
        }
    
        public void AddBuilding(BuildingData data, int count = 1)
        {
            InventoryItem item = items.Find(i => i.data == data);
            if (item != null)
            {
                item.count += count;
            }
            else
            {
                items.Add(new InventoryItem { data = data, count = count });
            }
            RefreshUI();
        }
    
        public bool HasBuilding(BuildingData data)
        {
            InventoryItem item = items.Find(i => i.data == data);
            return item != null && item.count > 0;
        }
    
        public void RemoveBuilding(BuildingData data, int count = 1)
        {
            InventoryItem item = items.Find(i => i.data == data);
            if (item != null)
            {
                item.count -= count;
                if (item.count <= 0)
                {
                    // items.Remove(item); // Keep it in the list but at 0?
                }
            }
            RefreshUI();
        }
    
        public delegate void OnInventoryChange();
        public event OnInventoryChange onInventoryChange;
    
        private void RefreshUI()
        {
            onInventoryChange?.Invoke();
            if (HotbarManager.Instance != null)
            {
                // Trigger hotbar refresh since counts might have changed
                // We can just invoke the same event or a different one
                // Let's assume HotbarUI listens to its own manager
            }
        }
    }
    
}


