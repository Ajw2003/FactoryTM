using Code.Scripts.EventSystems;
using Code.Scripts.Player.PlayerStateMachine;
using UnityEngine;

//"StateMachine" is referring to the "PlayerStateMachine" script

public class PlayerWalkState : PlayerState
{
    public PlayerWalkState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    private Vector2 minimumVelocity = new Vector2(0.5f, 0.5f);

    public override void Enter()
    {
        if (_stateMachine.animator != null)
        {
            _stateMachine.animator.speed = 1;
        }
        _stateMachine.animationNameToPlay = "Walk";
        _stateMachine.SetAnimationToPlay(_stateMachine.animationNameToPlay);
        _stateMachine.PreviousState = this;
    }

    public override void FixedUpdate() //Normal movement and rotation
    {
        Vector2 movement = _stateMachine.currentMovement;
        if (_stateMachine.rb2D != null)
        {
            _stateMachine.rb2D.linearVelocity = movement * _stateMachine.walkSpeed;
        }

        if (movement.magnitude > minimumVelocity.magnitude)
        {
            string direction = GetDirectionFromVector(movement);
            _stateMachine.previousDirection = direction;
            _stateMachine.animationNameToPlay = $"Walk{direction}"; // i.e., WalkUp or WalkDown
            _stateMachine.SetAnimationToPlay(_stateMachine.animationNameToPlay);
        }
        else
        {
            ChangeToIdle();
        }
    }

    public void ChangeToIdle()
    {
        if (_stateMachine.PreviousState == this)
        {
            EventManager.Instance?.Publish(new PlayerStateOverrideToIdleEvent());
        }
    }

    private string GetDirectionFromVector(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            return dir.x > 0 ? "Right" : "Left";
        else
            return dir.y > 0 ?  "Up" : "Down";
    }
}
