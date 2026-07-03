using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
namespace EventTypes
{
    using EventSystems;
    
    public class EnemyOutpostClearedEvent : IEvent
    {
        public EnemyOutpost Outpost { get; }
    
        public EnemyOutpostClearedEvent(EnemyOutpost outpost)
        {
            Outpost = outpost;
        }
    }
    
}


