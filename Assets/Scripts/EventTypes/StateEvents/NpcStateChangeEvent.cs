using EventSystems;
using UnityEngine;

public class NpcStateChangeEvent : IEvent
{
    public NpcBaseState NextState;
}
