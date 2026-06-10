using Code.Scripts.EventSystems;
using Code.Scripts.Player.PlayerStateMachine;
using UnityEngine;

//"StateMachine" is referring to the "PlayerStateMachine" script

public class PlayerInventoryState : PlayerState
{
    public PlayerInventoryState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        _stateMachine.PreviousState = this;
        _stateMachine.animationNameToPlay = "Idle";
        _stateMachine.animationNameToPlay = $"Idle{_stateMachine.previousDirection}"; // i.e., WalkUp or WalkDown
        _stateMachine.SetAnimationToPlay(_stateMachine.animationNameToPlay);
        EventManager.Instance?.Publish(new MovementInputEvent
        {
            IsEnabled = false
        });
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }
}
