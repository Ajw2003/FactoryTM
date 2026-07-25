using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using Items;
using Singleton;

namespace Managers
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class HotbarUI : SingletonBase<HotbarUI>
    {
        public GameObject slotPrefab;
        public Transform slotContainer;
        
        public InventorySlot[] slots;
    
        private void Start()
        {
            InitializeHotbar();
        }
    
        private void InitializeHotbar()
        {
            int count = HotbarManager.Instance.slotCount;
            slots = new InventorySlot[count];
            
            for (int i = 0; i < count; i++)
            {
                GameObject go = Instantiate(slotPrefab, slotContainer);
                slots[i] = go.GetComponent<InventorySlot>();
            }
        }
    }
    
}


