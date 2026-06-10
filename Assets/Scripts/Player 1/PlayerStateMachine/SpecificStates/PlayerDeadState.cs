using Code.Scripts.EventSystems;
using Code.Scripts.Player.PlayerStateMachine;
using UnityEngine;

//"StateMachine" is referring to the "PlayerStateMachine" script

public class PlayerDeadState : PlayerState
{
    public PlayerDeadState(PlayerStateMachine stateMachine) : base(stateMachine)
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
        if (_stateMachine.animator != null)
        {
            _stateMachine.animator.speed = 0;
        }
        _stateMachine.PreviousState = this;
        //player death animation here
        //StateMachine.animationNameToPlay = " "; 
        //StateMachine.SetAnimationToPlay(StateMachine.animationNameToPlay);

        //respawn event call here
        EventManager.Instance?.Publish(new MovementInputEvent
        {
            IsEnabled = false
        });
        EventManager.Instance?.Publish(new InventoryInputEvent
        {
            IsEnabled = false
        });
        EventManager.Instance?.Publish(new DialogueInputEvent
        {
            IsEnabled = false
        });
    }
}
