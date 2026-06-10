using Code.Scripts.EventSystems;
using Code.Scripts.Player.PlayerStateMachine;
using Unity.VisualScripting;
using UnityEngine;

//"StateMachine" is referring to the "PlayerStateMachine" script

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        _stateMachine.animationNameToPlay = "Idle";
        _stateMachine.animationNameToPlay = $"Idle{_stateMachine.previousDirection}"; // i.e., WalkUp or WalkDown
        _stateMachine.SetAnimationToPlay(_stateMachine.animationNameToPlay);
        _stateMachine.PreviousState = this;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        EventManager.Instance?.Publish(new MovementInputEvent
        {
            IsEnabled = true
        });
        EventManager.Instance?.Publish(new InventoryInputEvent
        {
            IsEnabled = true
        });
    }
}
