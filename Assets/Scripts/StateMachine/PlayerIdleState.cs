using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using Code.Scripts.EventSystems;
using UnityEngine;

namespace StateMachine
{
    public class PlayerIdleState : IState
    {
        private PlayerController player;
   
        public PlayerIdleState(PlayerController player)
        {
            this.player = player;
        }
        public void Enter()
        {
            // Enable movement and inventory inputs
            EventManager.Instance?.Publish(new MovementInputEvent { IsEnabled = true });
            EventManager.Instance?.Publish(new InventoryInputEvent { IsEnabled = true });
        }
        
        public void Exit()
        {
        }

        public void FixedUpdate()
        {
        }

        public void Update()
        {
            Vector2Int inputDirection = player.GetDiscreteInputDirection();

            if (inputDirection != Vector2Int.zero)
            {
                player.SetLastMoveDirection(inputDirection);
                player.StateMachine.ChangeState(player.StateMachine.walkState);
            }
        }
    }
}


