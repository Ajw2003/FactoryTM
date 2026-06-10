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
            //disable all input except pause menu
        }

        void IState.Update()
        {
            Update();
        }

        public void Exit()
        {
            throw new System.NotImplementedException();
        }

        public void FixedUpdate()
        {
            throw new System.NotImplementedException();
        }

        void Update()
        {
        
        }
    }
}
