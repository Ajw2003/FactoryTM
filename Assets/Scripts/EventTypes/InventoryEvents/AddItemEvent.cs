using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventSystems;

namespace EventTypes.InventoryEvents
{
    public class AddItemEvent : IEvent
    {
        public InventoryManager.InventoryItem Item { get; set; }
    }
}
