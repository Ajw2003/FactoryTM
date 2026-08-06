using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
namespace EventTypes.BuildingEvents
{
    using EventSystems;

    /// <summary>Published when a building is destroyed, after rewards and outpost bookkeeping have run.</summary>
    public class BuildingDiedEvent : IEvent
    {
        public BuildingLogic Building { get; }  

        public BuildingDiedEvent(BuildingLogic building)
        {
            Building = building;
        }
    }

}
