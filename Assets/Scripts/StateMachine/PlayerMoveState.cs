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
                player.StateMachine.TransitionTo(player.StateMachine.idleState);
                return;
            }

            player.SetLastMoveDirection(inputDirection);

            // Movement logic
            Vector2Int currentCell = GridManager.Instance.WorldToCellConversion(player.transform.position);
            Vector2Int nextCell = currentCell + inputDirection;
            Vector3Int nextCellV3 = new Vector3Int(nextCell.x, nextCell.y, 0);
            
            Vector2Int nextZone = ZoneManager.Instance.GetZoneCoordsFromTile(nextCellV3);
            Vector2Int currentZone = ZoneManager.Instance.GetCurrentZone();

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
                    if (viewportPos.x < 0.02f || viewportPos.x > 0.98f || viewportPos.y < 0.04f || viewportPos.y > 0.96f)
                    {
                        isVisible = false;
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
