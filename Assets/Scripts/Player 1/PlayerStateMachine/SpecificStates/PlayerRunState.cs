using Code.Scripts.Player.PlayerStateMachine;
using UnityEngine;

//"StateMachine" is referring to the "PlayerStateMachine" script

public class PlayerRunState : PlayerState
{
    public PlayerRunState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        _stateMachine.PreviousState = this;
        HandleMovement();
    }

    public override void FixedUpdate() //Normal movement and rotation
    {
        _stateMachine.rb2D.linearVelocity = _stateMachine.currentMovement * _stateMachine.sprintSpeed;
    }
}
