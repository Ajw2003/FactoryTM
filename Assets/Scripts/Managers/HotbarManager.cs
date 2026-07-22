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
    using Items;
    using Singleton;
    using UnityEngine;

    public class HotbarManager : SingletonBase<HotbarManager>
    {
        public int slotCount = 9;
        private int selectedSlot = 0;

        // Reverse lookup from a hotbar slot's ItemData back to the BuildingData it represents -
        // built once from every BuildingData asset that has an itemData reference (Phase 1).
        private Dictionary<specificItemType, BuildingData> buildingByItemType = new Dictionary<specificItemType, BuildingData>();

        protected override void Awake()
        {
            persistBetweenScenes = false;
            base.Awake();
        }

        private void Start()
        {
            foreach (var data in Resources.LoadAll<BuildingData>("BuildingData"))
            {
                if (data != null && data.itemData != null)
                {
                    buildingByItemType[data.itemData.specificItemType] = data;
                }
            }
        }

        private void Update()
        {
            if (PauseManager.IsPaused) return;

            if (PlayerController.Instance == null || PlayerController.Instance.currentMode == PlayerController.PlayerMode.Combat) return;

            // Handle slot selection via number keys
            for (int i = 0; i < slotCount; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SelectSlot(i);
                }
            }

            // Scroll wheel selection
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0f) SelectSlot((selectedSlot + 1) % slotCount);
            else if (scroll < 0f) SelectSlot((selectedSlot - 1 + slotCount) % slotCount);
        }

        public void SelectSlot(int index)
        {
            if (index < 0 || index >= slotCount) return;
            selectedSlot = index;

            if (PlacementManager.Instance != null)
            {
                PlacementManager.Instance.ChangeSelection(GetSelectedBuilding());
            }
        }

        /// <summary>Resolves the BuildingData a hotbar slot represents via its ItemData - null if
        /// that slot is empty or currently holds a resource rather than a building.</summary>
        public BuildingData GetBuildingInSlot(int index)
        {
            if (!HotbarUI.HasInstance || HotbarUI.Instance.slots == null || index < 0 || index >= HotbarUI.Instance.slots.Length)
            {
                return null;
            }

            InventorySlot slot = HotbarUI.Instance.slots[index];
            if (slot == null || !slot.slotFilled) return null;

            return buildingByItemType.TryGetValue(slot._itemData.specificItemType, out BuildingData data) ? data : null;
        }

        public BuildingData GetSelectedBuilding()
        {
            return GetBuildingInSlot(selectedSlot);
        }

        /// <summary>Removes 1 unit of `data` from whichever hotbar slot currently holds it, so the
        /// visible count tracks what's actually left to place (called on placement).</summary>
        public void RemoveFromHotbar(BuildingData data)
        {
            if (data == null || data.itemData == null || !HotbarUI.HasInstance || HotbarUI.Instance.slots == null) return;

            foreach (var slot in HotbarUI.Instance.slots)
            {
                if (slot != null && slot.slotFilled && slot._itemData.specificItemType == data.itemData.specificItemType)
                {
                    slot.TryRemove(1);
                    return;
                }
            }
        }

        public int SelectedSlot => selectedSlot;
    }

}
