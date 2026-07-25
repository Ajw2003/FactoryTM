using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
using Code.Scripts.EventSystems;

namespace StateMachine
{
    public class PlayerStoreState : IState
    {
    
        private PlayerController player;
   
        public PlayerStoreState(PlayerController player)
        {
            this.player = player;
        }
        public void Enter()
        {
            // Disable movement but allow UI/Inventory input
            EventManager.Instance?.Publish(new MovementInputEvent { IsEnabled = false });
            EventManager.Instance?.Publish(new InventoryInputEvent { IsEnabled = true });

            UiManager.Instance?.OpenStore();
        }

        public void Update()
        {
        }

        public void Exit()
        {
            UiManager.Instance?.CloseStore();
        }

        public void FixedUpdate()
        {
        }
    }
}

