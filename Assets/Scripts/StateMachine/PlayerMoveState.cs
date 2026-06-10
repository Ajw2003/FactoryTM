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
            throw new System.NotImplementedException();
        }

        public void Update()
        {
            throw new System.NotImplementedException();
        }

        public void Exit()
        {
            throw new System.NotImplementedException();
        }

        public void FixedUpdate()
        {
            throw new System.NotImplementedException();
        }
    }
}
