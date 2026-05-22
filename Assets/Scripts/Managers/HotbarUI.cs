using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    public GameObject slotPrefab;
    public Transform slotContainer;
    
    private HotbarSlotUI[] slots;

    private void Start()
    {
        InitializeHotbar();
        if (HotbarManager.Instance != null)
        {
            HotbarManager.Instance.onHotbarChange += UpdateUI;
        }
    }

    private void InitializeHotbar()
    {
        int count = HotbarManager.Instance.slotCount;
        slots = new HotbarSlotUI[count];
        
        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(slotPrefab, slotContainer);
            slots[i] = go.GetComponent<HotbarSlotUI>();
            slots[i].SetSlotIndex(i);
        }
        UpdateUI();
    }

    private void UpdateUI()
    {
        var hotbarSlots = HotbarManager.Instance.Slots;
        int selected = HotbarManager.Instance.SelectedSlot;
        
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].UpdateSlot(hotbarSlots[i], i == selected);
        }
    }
}
