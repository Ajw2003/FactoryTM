using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using UnityEngine;

public interface IHealth
{
    int Health { get; }
    void TakeDamage(int amount);
    void Die();
}

