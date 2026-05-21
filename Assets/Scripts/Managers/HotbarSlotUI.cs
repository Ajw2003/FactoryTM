using Buildings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotbarSlotUI : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text countText;
    public Image highlightFrame;

    private int slotIndex;
    private BuildingData currentData;
    

    public void UpdateSlot(BuildingData data, bool isSelected)
    {
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
                var item = InventoryManager.Instance.items.Find(i => i.data == data);
                countText.text = item != null ? item.count.ToString() : "0";
            }
        }
        else
        {
            if (iconImage != null) iconImage.enabled = false;
            if (countText != null) countText.text = "";
        }
    }
}
