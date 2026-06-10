using System.Collections;
using StateMachine.Monsters;
using UnityEngine;

public class MonsterAgroState : MonsterBaseState
{
    public MonsterAgroState(MonsterStateMachine stateMachine) : base(stateMachine)
    {
        
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public override void FixedUpdate()
    {
        StateMachine.DistanceCheck();
    }

    public override void Enter()
    {
        StateMachine.PreviousState = this;
        StateMachine.SetTarget();
        //monster chase animation go here
    }
}
