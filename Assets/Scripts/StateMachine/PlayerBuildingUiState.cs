using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using Code.Scripts.EventSystems;

namespace StateMachine
{
    public class PlayerBuildingUiState : IState
    {
        private PlayerController player;

        public PlayerBuildingUiState(PlayerController player)
        {
            this.player = player;
        }

        public void Enter()
        {
            // Disable movement but allow UI/Inventory input
            EventManager.Instance?.Publish(new MovementInputEvent { IsEnabled = false });
            EventManager.Instance?.Publish(new InventoryInputEvent { IsEnabled = true });
        }

        public void Update()
        {
        }

        public void Exit()
        {
            // Re-enable movement when exiting building UI state
            EventManager.Instance?.Publish(new MovementInputEvent { IsEnabled = true });
        }

        public void FixedUpdate()
        {
        }
    }
}

