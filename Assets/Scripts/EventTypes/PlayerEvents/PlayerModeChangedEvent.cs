using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
namespace EventTypes.PlayerEvents
{
    using EventSystems;

    /// <summary>Published whenever the player toggles between combat and building mode.</summary>
    public class PlayerModeChangedEvent : IEvent
    {
        public PlayerController.PlayerMode Mode { get; }

        public PlayerModeChangedEvent(PlayerController.PlayerMode mode)
        {
            Mode = mode;
        }
    }

}
