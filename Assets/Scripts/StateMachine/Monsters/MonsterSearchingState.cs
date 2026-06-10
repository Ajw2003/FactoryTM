using StateMachine.Monsters;
using UnityEngine;

public class MonsterPatrollingState : MonsterBaseState
{
    public MonsterPatrollingState(MonsterStateMachine stateMachine) : base(stateMachine)
    {
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    public override void Update()
    {
        
    }

    public override void Enter()
    {
        StateMachine.PreviousState = this;
    }
}
