using System.Collections.Generic;
using Buildings;
using Singleton;
using UnityEngine;

public class HotbarManager : SingletonBase<HotbarManager>
{
    public int slotCount = 9;
    private BuildingData[] hotbarSlots;
    private int selectedSlot = 0;

    protected override void Awake()
    {
        persistBetweenScenes = false;
        base.Awake();
        hotbarSlots = new BuildingData[slotCount];
    }

    private void Update()
    {
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

    private void Start()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onInventoryChange += RefreshUI;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (InventoryManager.HasInstance)
        {
            InventoryManager.Instance.onInventoryChange -= RefreshUI;
        }
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= slotCount) return;
        selectedSlot = index;
        
        // Update PlacementManager
        if (PlacementManager.Instance != null)
        {
            PlacementManager.Instance.ChangeSelection(hotbarSlots[selectedSlot]);
        }
        
        RefreshUI();
    }

    public void AssignToSlot(int slotIndex, BuildingData data)
    {
        if (slotIndex < 0 || slotIndex >= slotCount) return;
        hotbarSlots[slotIndex] = data;
        
        RefreshUI();
    }

    public BuildingData GetSelectedBuilding()
    {
        return hotbarSlots[selectedSlot];
    }

    public int SelectedSlot => selectedSlot;
    public BuildingData[] Slots => hotbarSlots;

    public delegate void OnHotbarChange();
    public event OnHotbarChange onHotbarChange;

    private void RefreshUI()
    {
        onHotbarChange?.Invoke();
    }
}
