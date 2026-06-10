using EventSystems;
using UnityEngine;

public class MonsterStateChangeEvent :IEvent
{
    public MonsterBaseState NextState { get; set; }
}
