using Buildings;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryUIItemButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public BuildingData buildingData;
    public TMP_Text priceText;
    public PlacementManager placementManager;

    private void Start()
    {
        if (buildingData != null && priceText != null)
        {
            var ajustedPrice = buildingData.cost * CurrencyManager.Instance.exchangeRate;
            priceText.text = "$" + ajustedPrice;
        }
    }

    // Attach this to a Button's OnClick event in the Inspector
    // Drag the specific BuildingData asset into the parameter slot
    public void SelectBuilding(BuildingData data)
    {
        if (data != null)
        {
            placementManager.ChangeSelection(data);
        }
        else if (buildingData != null)
        {
            placementManager.ChangeSelection(buildingData);
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