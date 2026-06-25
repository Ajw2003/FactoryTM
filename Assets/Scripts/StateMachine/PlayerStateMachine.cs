using System;

namespace StateMachine
{
    [Serializable]
    public class PlayerStateMachine
    {
        public IState CurrentState { get; private set; }


        // reference to the state objects
        public PlayerMoveState walkState;
        public PlayerIdleState idleState;
        public PlayerDeadState deadState;
        public PlayerStoreState storeState;
        public PlayerDodgeState dodgeState;
        public PlayerBuildingUiState buildingUiState;


        // event to notify other objects of the state change
        public event Action<IState> stateChanged;


        // pass in necessary parameters into constructor 
        public PlayerStateMachine(PlayerController player)
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


            // notify other objects that state has changed
            stateChanged?.Invoke(state);
        }


        // exit this state and enter another
        public void TransitionTo(IState nextState)
        {
            CurrentState.Exit();
            CurrentState = nextState;
            nextState.Enter();


            // notify other objects that state has changed
            stateChanged?.Invoke(nextState);
        }


        // allow the StateMachine to update this state
        public void Update()
        {
            if (CurrentState != null)
            {
                CurrentState.Update();
            }
        }
    }
}

