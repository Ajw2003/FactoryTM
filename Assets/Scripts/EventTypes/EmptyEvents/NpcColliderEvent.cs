using EventSystems;

namespace Code.Scripts.EventSystems.EventTypes.EmptyEvents
{
    public class NpcColliderEvent : IEvent //if you press f2 on a class, and rename it, it will automatically rename the file. You can also do this for fixing any variable names.
    {
        public bool Entered { get; set; } //Name changed with f2 and then it automatically propagates to all files (Jetbrains <3)
    }
}
