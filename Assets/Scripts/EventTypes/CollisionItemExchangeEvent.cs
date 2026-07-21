using EventSystems;
using Items;
using Managers;
using UnityEngine;

public class CollisionItemExchangeEvent : IEvent
{
    public Rectangle2D rectangle;
    
    public Item item;
}
