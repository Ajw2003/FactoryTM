using Buildings;
using UnityEngine;

public class InventoryUIItemButton : MonoBehaviour
{
    public PlacementManager placementManager;

    // Attach this to a Button's OnClick event in the Inspector
    // Drag the specific BuildingData asset into the parameter slot
    public void SelectBuilding(BuildingData data)
    {
        placementManager.ChangeSelection(data);
    }
}