using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using System;

namespace StateMachine
{
    public class PlayerStateMachine : StateMachine.BaseStateMachine
    {
        // reference to the state objects
        public PlayerMoveState walkState;
        public PlayerIdleState idleState;
        public PlayerDeadState deadState;
        public PlayerStoreState storeState;
        public PlayerDodgeState dodgeState;
        public PlayerBuildingUiState buildingUiState;


        // event to notify other objects of the state change
        public event Action<IState> stateChanged;


        // pass in necessary parameters into Initialize 
        public void Initialize(PlayerController player)
        {
            // create an instance for each state and pass in PlayerController
            this.walkState = new PlayerMoveState(player);
            this.deadState = new PlayerDeadState(player);
            this.idleState = new PlayerIdleState(player);
            this.storeState = new PlayerStoreState(player);
            this.dodgeState = new PlayerDodgeState(player);
            this.buildingUiState = new PlayerBuildingUiState(player);
        }


        // set the starting state
        public void Initialize(IState state)
        {
            CurrentState = state;
            state.Enter();
            currentStateName = CurrentState?.ToString();

            // notify other objects that state has changed
            stateChanged?.Invoke(state);
        }


        // exit this state and enter another
        public override void ChangeState(IState nextState)
        {
            base.ChangeState(nextState);

            // notify other objects that state has changed
            stateChanged?.Invoke(nextState);
        }
    }
}


