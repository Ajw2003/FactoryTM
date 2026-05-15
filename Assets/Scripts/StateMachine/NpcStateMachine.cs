using System.Collections.Generic;
using StateMachine;
using UnityEngine;

public class NpcStateMachine : BaseStateMachine
{
    public float CurrentHealth => _health;
    public float MaxHealth => _maxHealth;

    public Transform Target { get; set; }
    public float MoveSpeed = 3f;
    public List<Vector2> AssignedBuildings = new List<Vector2>();
    public int CurrentBuildingIndex = 0;
    public float BuildingReachedThreshold = 1.0f; // Increased for NavMesh precision

    [Header("Patrol Settings")]
    public float PatrolRadius = 10f;
    public int PatrolPointCount = 3;
    
    private float _health;
    private float _maxHealth;

}
