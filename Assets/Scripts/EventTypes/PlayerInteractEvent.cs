using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
using EventSystems;

namespace Code.Scripts.Interfaces.EventTypes
{
    public class PlayerInteractEvent : IEvent
    {
        
    }

    public class PlayerInteractInputEvent : IEvent
    {
        public bool IsEnabled;
    }
}

