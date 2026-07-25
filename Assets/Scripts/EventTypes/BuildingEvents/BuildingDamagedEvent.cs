using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
namespace EventTypes.BuildingEvents
{
    using EventSystems;

    /// <summary>Published whenever a building takes damage, after its health has been reduced.</summary>
    public class BuildingDamagedEvent : IEvent
    {
        public BuildingLogic Building { get; }
        public int CurrentHealth { get; }
        public int MaxHealth { get; }

        public BuildingDamagedEvent(BuildingLogic building, int currentHealth, int maxHealth)
        {
            Building = building;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
        }
    }

}
