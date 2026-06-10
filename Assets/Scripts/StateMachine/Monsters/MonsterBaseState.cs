using StateMachine;
using StateMachine.Monsters;
using UnityEngine;

public class MonsterBaseState : IState
{
    protected MonsterStateMachine StateMachine;
    public MonsterBaseState(MonsterStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }
    public virtual void Enter()
    {
       StateMachine.ChangeState(this);
    }

    public virtual void Update()
    {
        
    }

    public virtual void Exit()
    {
        
    }

    public virtual void FixedUpdate()
    {
        
    }
}
