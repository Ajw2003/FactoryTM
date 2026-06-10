using Code.Scripts.EventSystems;
using Code.Scripts.Player.PlayerStateMachine;
using UnityEngine;

public class PlayerPuzzleState : PlayerState
{
    public PlayerPuzzleState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void FixedUpdate()
    {
        //In Non movement states such as this override update but don't call any logic, this just doesn't run the movement logic and as such removes the requirement for freeze logic
    }

    public override void Enter()
    {
        _stateMachine.PreviousState = this;
        _stateMachine.animationNameToPlay = "Idle";
        _stateMachine.animationNameToPlay = $"Idle{_stateMachine.previousDirection}"; // i.e., WalkUp or WalkDown
        _stateMachine.SetAnimationToPlay(_stateMachine.animationNameToPlay);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        EventManager.Instance?.Publish(new MovementInputEvent
        {
            IsEnabled = false
        });
        EventManager.Instance?.Publish(new InventoryInputEvent
        {
            IsEnabled = false
        });
    }
}
