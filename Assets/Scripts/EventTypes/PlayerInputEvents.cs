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
}
