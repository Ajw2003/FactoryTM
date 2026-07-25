using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using Code.Scripts.EventSystems;

namespace StateMachine
{
    public class PlayerDeadState : IState
    {
        private PlayerController player;
   
        public PlayerDeadState(PlayerController player)
        {
            this.player = player;
        }
        public void Enter()
        {
            // Disable all inputs
            EventManager.Instance?.Publish(new MovementInputEvent { IsEnabled = false });
            EventManager.Instance?.Publish(new InventoryInputEvent { IsEnabled = false });
        }

        public void Update()
        {
        }

        public void Exit()
        {
        }

        public void FixedUpdate()
        {
        }
    }
}

