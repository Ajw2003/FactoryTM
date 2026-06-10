using System.Collections;
using UnityEngine;
using System;
using Code.Scripts.EventSystems;
using Code.Scripts.Player.PlayerStateMachine;

public class PlayerCaughtState : PlayerState
{
    public PlayerCaughtState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }
    
    public override void FixedUpdate()
    {
        //In Non movement states such as this override update but don't call any logic, this just doesn't run the movement logic and as such removes the requirement for freeze logic
    }

    public override void Enter()
    {
        _stateMachine.animationNameToPlay = $"Idle{_stateMachine.previousDirection}"; // i.e., WalkUp or WalkDown
        _stateMachine.SetAnimationToPlay(_stateMachine.animationNameToPlay);
        _stateMachine.PreviousState = this;
        EventManager.Instance?.Publish(new MovementInputEvent
        {
            IsEnabled = false
        });
        EventManager.Instance?.Publish(new PlayerEscapeInputEvent
        {
            IsEnabled = true
        });
    }
}
