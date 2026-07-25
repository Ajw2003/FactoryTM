using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventSystems;

namespace EventTypes.InputEvents
{
    public class InputToggleEvent : IEvent
    {
        public string ActionName { get; set; }
        public bool Enable { get; set; }
    }
}

