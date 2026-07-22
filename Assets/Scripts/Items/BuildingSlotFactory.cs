using UnityEngine;

namespace Items
{
    /// <summary>Creates an InventorySlot instance for a building's own owned input/output/storage
    /// state. Parented under the building itself (no Canvas ancestor) so it stays invisible/inert
    /// until BuildingUiManager reparents it into the shared panel while that building's panel is open.</summary>
    public static class BuildingSlotFactory
    {
        private const string PrefabPath = "prefabs/Items/InventorySlot";

        public static InventorySlot CreateSlot(Transform owner)
        {
            GameObject prefab = Resources.Load<GameObject>(PrefabPath);
            if (prefab == null) return null;

            GameObject go = Object.Instantiate(prefab, owner);
            go.SetActive(false);
            return go.GetComponent<InventorySlot>();
        }
    }
}
