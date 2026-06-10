using EventSystems;
using UnityEngine;

public class OtherInventoryEvent : IEvent
{
    public PlayerActions PlayerAction { get; set; }
}
