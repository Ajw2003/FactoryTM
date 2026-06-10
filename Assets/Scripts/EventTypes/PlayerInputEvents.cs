using EventSystems;
using UnityEngine;

namespace Code.Scripts.Interfaces.EventTypes
{
    public class PlayerMoveEvent : IEvent
    {
        public Vector2 MoveInput;

        public PlayerMoveEvent(Vector2 moveInput)
        {
            MoveInput = moveInput;
        }
    }

    public class PlayerDodgeEvent : IEvent
    {
    }

    public class PlayerHealEvent : IEvent
    {
    }

    public class PlayerOpenStoreEvent : IEvent
    {
    }

    public class PlayerPlaceEvent : IEvent
    {
    }

    public class PlayerRemoveEvent : IEvent
    {
    }

    public class PlayerRotateEvent : IEvent
    {
    }

    public class PlayerNextItemEvent : IEvent
    {
    }

    public class PlayerPreviousItemEvent : IEvent
    {
    }
}
