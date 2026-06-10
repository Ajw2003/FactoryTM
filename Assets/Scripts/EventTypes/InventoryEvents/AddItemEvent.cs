using EventSystems;

namespace EventTypes.InventoryEvents
{
    public class AddItemEvent : IEvent
    {
        public InventoryManager.InventoryItem Item { get; set; }
    }
}