using Buildings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HotbarSlotUI : MonoBehaviour, IPointerClickHandler
{
    public Image iconImage;
    public TMP_Text countText;
    public Image highlightFrame;

    private int slotIndex;
    private BuildingData currentData;
    
    private BuildingData lastData;
    private int lastCount = -1;

    public void SetSlotIndex(int index)
    {
        slotIndex = index;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (HotbarManager.Instance != null)
        {
            HotbarManager.Instance.SelectSlot(slotIndex);
        }
    }

    public void UpdateSlot(BuildingData data, bool isSelected)
    {
        // Calculate new count
        int newCount = 0;
        if (data != null)
        {
            var item = InventoryManager.Instance.items.Find(i => i.data == data);
            newCount = item != null ? item.count : 0;
        }

        // Detect if item is newly added or count increased
        bool newlyAdded = (data != null && lastData == null);
        bool countIncreased = (data != null && lastData == data && newCount > lastCount);

        currentData = data;
        if (highlightFrame != null) highlightFrame.enabled = isSelected;

        if (data != null)
        {
            if (iconImage != null)
            {
                iconImage.sprite = data.icon;
                iconImage.enabled = data.icon != null;
            }
            
            if (countText != null)
            {
                countText.text = newCount.ToString();
            }


        }
        else
        {
            if (iconImage != null) iconImage.enabled = false;
            if (countText != null) countText.text = "";
        }

        // Cache for next comparison
        lastData = data;
        lastCount = newCount;
    }

}
