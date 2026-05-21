using Buildings;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryUIItemButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public BuildingData buildingData;
    public TMP_Text priceText;
    public PlacementManager placementManager;

    public TMP_Text countText;

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onInventoryChange += RefreshUI;
        }
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onInventoryChange -= RefreshUI;
        }
    }

    private void Start()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (buildingData != null)
        {
            if (priceText != null)
            {
                var adjustedPrice = buildingData.cost * CurrencyManager.Instance.exchangeRate;
                priceText.text = "$" + adjustedPrice;
            }

            if (countText != null)
            {
                InventoryManager.InventoryItem item = InventoryManager.Instance.items.Find(i => i.data == buildingData);
                countText.text = item != null ? item.count.ToString() : "0";
            }
        }
    }

    // This is now purely for the STORE
    public void PurchaseBuilding()
    {
        if (buildingData == null) return;

        float cost = buildingData.cost * CurrencyManager.Instance.exchangeRate;
        if (CurrencyManager.Instance.currentCurrencyValue >= cost)
        {
            CurrencyManager.Instance.RemoveCurrency(buildingData.cost);
            InventoryManager.Instance.AddBuilding(buildingData, 1);
            
            // Auto-assign to first empty hotbar slot if it's the first time buying
            bool alreadyInHotbar = false;
            int emptySlot = -1;
            for (int i = 0; i < HotbarManager.Instance.slotCount; i++)
            {
                if (HotbarManager.Instance.Slots[i] == buildingData) alreadyInHotbar = true;
                if (emptySlot == -1 && HotbarManager.Instance.Slots[i] == null) emptySlot = i;
            }

            if (!alreadyInHotbar && emptySlot != -1)
            {
                HotbarManager.Instance.AssignToSlot(emptySlot, buildingData);
            }

            RefreshUI();
        }
        else
        {
            Debug.Log("Not enough currency to buy " + buildingData.buildingName);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buildingData != null && !string.IsNullOrEmpty(buildingData.description))
        {
            UiManager.Instance.ShowTooltip(buildingData.description);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UiManager.Instance.HideTooltip();
    }
}