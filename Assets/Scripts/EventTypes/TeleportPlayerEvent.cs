using EventSystems;
using UnityEngine;

public class TeleportPlayerEvent : IEvent
{
    public Transform Destination { get; set; }
}
