using Code.Scripts.Player.PlayerStateMachine;
using EventSystems;

namespace EventTypes.StateEvents
{
    public class PlayerStateChangeEvent : IEvent
    {
        public PlayerState NextState { get; set; }
    }
}

