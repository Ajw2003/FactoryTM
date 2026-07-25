using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
namespace EventTypes.PlayerEvents
{
    using EventSystems;

    /// <summary>Published whenever the player's reserve ammo count changes.</summary>
    public class PlayerAmmoChangedEvent : IEvent
    {
        public int AmmoReserve { get; }

        public PlayerAmmoChangedEvent(int ammoReserve)
        {
            AmmoReserve = ammoReserve;
        }
    }

}
