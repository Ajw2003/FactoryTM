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
            //move into idle state and disable movement, shooting and building only allow ui input
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
