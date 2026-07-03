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
    using UnityEngine;
    
    public class StopAudioEvent: IEvent
    {
        public AudioChannel Channel;
    }
    
}


