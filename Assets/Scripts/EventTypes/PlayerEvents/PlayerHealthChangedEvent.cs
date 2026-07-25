using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
namespace EventTypes.PlayerEvents
{
    using EventSystems;

    /// <summary>Published whenever the player's current or maximum health changes.</summary>
    public class PlayerHealthChangedEvent : IEvent
    {
        public int Health { get; }
        public int MaxHealth { get; }

        public PlayerHealthChangedEvent(int health, int maxHealth)
        {
            Health = health;
            MaxHealth = maxHealth;
        }
    }

}
