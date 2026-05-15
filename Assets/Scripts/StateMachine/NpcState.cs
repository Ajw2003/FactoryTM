using StateMachine;
using UnityEngine;

public class NpcState : IState
{
    protected NpcStateMachine _stateMachine; //Some confusing parts if you have protected variables as a Public value, as it also is a name you have used for a class.

    public NpcState(NpcStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
    }

    public virtual void Enter()
    {
            
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

    public virtual void HandleMovement()
    {

    }
}
