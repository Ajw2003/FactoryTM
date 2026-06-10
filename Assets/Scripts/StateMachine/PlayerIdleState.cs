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
            //move into idle state, this enables movement, building and shooting input and is the transition state from every other state
        }
        
        public void Exit()
        {
        
        }

        public void FixedUpdate()
        {
        
        }

        public void Update()
        {
        
        }
    }
}
