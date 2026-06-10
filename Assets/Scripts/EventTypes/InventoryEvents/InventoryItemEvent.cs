using EventSystems;

namespace EventTypes.InventoryEvents
{
    public class InventoryItemEvent : IEvent
    {
        public InventoryManager.InventoryItem Item { get; set; }
        public bool Exists { get; set; }
    }
}