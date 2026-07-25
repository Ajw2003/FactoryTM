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
            Vector3Int nextCellV3 = new Vector3Int(nextCell.x, nextCell.y, 0);
            
            Vector2Int nextZone = ZoneManager.Instance.GetZoneCoordsFromTile(nextCellV3);
            Vector2Int currentZone = ZoneManager.Instance.GetCurrentZone();

            if (PlacementManager.HasInstance && !PlacementManager.Instance.IsCellWalkable(nextCell))
            {
                return;
            }

            if (nextZone != currentZone)
            {
                if (ZoneManager.Instance.IsZoneUnlocked(nextZone))
                {
                    ZoneManager.Instance.SetCurrentZone(nextZone);
                    player.StartMove(nextCell);
                }
            }
            else
            {
                bool isVisible = true;
                Camera mainCam = ZoneManager.Instance != null && ZoneManager.Instance.mainCamera != null ? ZoneManager.Instance.mainCamera : Camera.main;
                if (mainCam != null && GridManager.Instance != null)
                {
                    Vector2 targetWorldPos = GridManager.Instance.CellToWorldConversion(nextCell);
                    Vector3 viewportPos = mainCam.WorldToViewportPoint(targetWorldPos);
                    
                    bool outOfBoundsX = viewportPos.x < 0.02f || viewportPos.x > 0.98f;
                    bool outOfBoundsY = viewportPos.y < 0.04f || viewportPos.y > 0.96f;

                    if (outOfBoundsX || outOfBoundsY)
                    {
                        // Check if the adjacent zone in the direction we're moving is unlocked
                        Vector2Int adjacentZone = currentZone;
                        if (viewportPos.x < 0.02f) adjacentZone.x -= 1;
                        if (viewportPos.x > 0.98f) adjacentZone.x += 1;
                        if (viewportPos.y < 0.04f) adjacentZone.y -= 1;
                        if (viewportPos.y > 0.96f) adjacentZone.y += 1;

                        if (!ZoneManager.Instance.IsZoneUnlocked(adjacentZone))
                        {
                            isVisible = false;
                        }
                    }
                }

                if (isVisible)
                {
                    player.StartMove(nextCell);
                }
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


