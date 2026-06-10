using EventSystems;

namespace Code.Scripts.EventSystems.EventTypes.InputEvents
{
    public class InputToggleEvent : IEvent
    {
        public string ActionName { get; set; }
        public bool Enable { get; set; }
    }
}