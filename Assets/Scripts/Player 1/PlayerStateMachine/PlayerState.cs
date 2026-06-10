using StateMachine;
using UnityEngine;

//This script is to be inherited by ALL STATE SPECIFIC functionality for the player's state machine.
//This includes a state for the player running, player walking, player within the inventory UI, etc.

namespace Code.Scripts.Player.PlayerStateMachine
{
    public class PlayerState : IState
    {
        protected PlayerStateMachine _stateMachine; //Some confusing parts if you have protected variables as a Public value, as it also is a name you have used for a class.

        public PlayerState(PlayerStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public virtual void Enter()
        {
            _stateMachine.ChangeState(this);
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

        public virtual void OnTriggerEnter2D(Collider2D other)
        {
        
        }

        public virtual void HandleMovement()
        {

        }
    }
}
