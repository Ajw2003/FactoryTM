using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
using EventSystems;

namespace EventTypes.InventoryEvents
{
    public class InventoryItemEvent : IEvent
    {
        public InventoryManager.InventoryItem Item { get; set; }
        public bool Exists { get; set; }
    }
}
