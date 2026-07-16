using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
using UnityEngine;

namespace StateMachine
{
    public class PlayerMoveState : IState
    {
    
        private PlayerController player;
   
        public PlayerMoveState(PlayerController player)
        {
            this.player = player;
        }
    
        public void Enter()
        {
        }

        public void Update()
        {
            if (player.IsMoving()) return;

            Vector2Int inputDirection = player.GetDiscreteInputDirection();

            if (inputDirection == Vector2Int.zero)
            {
                player.StateMachine.ChangeState(player.StateMachine.idleState);
                return;
            }

            player.SetLastMoveDirection(inputDirection);

            // Movement logic
            Vector2Int currentCell = GridManager.Instance.WorldToCellConversion(player.transform.position);
            Vector2Int nextCell = currentCell + inputDirection;
            if (PlacementManager.HasInstance && !PlacementManager.Instance.IsCellWalkable(nextCell))
            {
                return;
            }

            player.StartMove(nextCell);
        }

        public void Exit()
        {
        }

        public void FixedUpdate()
        {
        }
    }
}


