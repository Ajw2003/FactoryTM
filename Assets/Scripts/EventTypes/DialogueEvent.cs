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
    
    public class DialogueEvent : IEvent
    {
        public bool enabled;
        public DialogueSO dialogue;
        public int index;
    }
    
}


