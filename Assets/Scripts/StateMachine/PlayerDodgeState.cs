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
    public class PlayerDodgeState : IState
    {
    
        private PlayerController player;
   
        public PlayerDodgeState(PlayerController player)
        {
            this.player = player;
        }
        public void Enter()
        {
            // Disable inputs during dodge
            EventManager.Instance?.Publish(new MovementInputEvent { IsEnabled = false });
            EventManager.Instance?.Publish(new InventoryInputEvent { IsEnabled = false });

            Vector2Int inputDirection = player.GetDiscreteInputDirection();
            Vector2Int dodgeDir = inputDirection != Vector2Int.zero ? inputDirection : player.GetLastMoveDirection();
            
            player.StartDodge(dodgeDir);
        }

        public void Update()
        {
            if (!player.IsDodging())
            {
                player.StateMachine.ChangeState(player.StateMachine.idleState);
            }
        }

        public void Exit()
        {
        }

        public void FixedUpdate()
        {
        }
    }
}


