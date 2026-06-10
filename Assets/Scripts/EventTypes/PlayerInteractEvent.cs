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
