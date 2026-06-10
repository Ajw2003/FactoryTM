using StateMachine;
using StateMachine.Npcs;
using UnityEngine;

public class NpcBaseState : IState
{
    
    protected NpcStateMachine StateMachine;
    public NpcBaseState(NpcStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }
    public virtual void Enter()
    {
        StateMachine.ChangeState(this);
    }

    void IState.Update()
    {
        Update();
    }

    public void Exit()
    {
        throw new System.NotImplementedException();
    }

    public void FixedUpdate()
    {
        throw new System.NotImplementedException();
    }

    public virtual void OnTriggerEnter2D(Collider2D other)
    {
        
    }

    void Update()
    {
        
    }
}
