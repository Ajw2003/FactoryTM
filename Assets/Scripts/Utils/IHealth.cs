using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
using UnityEngine;

public interface IHealth 
{
    int Health { get; set; }
    void TakeDamage(int amount);
    
    void ChangeHealth(int amount, int previous);
    void Die();
}

