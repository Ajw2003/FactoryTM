namespace StateMachine
{
    public class PlayerDodgeState : IState
    {
    
        private PlayerController player;
   
        public PlayerDodgeState(PlayerController player)
        {
            this.player = player;
        }
        public void Enter()
        {
            //I frames and not take damage while in state 
        }

        public void Update()
        {
        }

        public void Exit()
        {
            //when dodge finishes immediately exit and disable invulnerability state
        }

        public void FixedUpdate()
        {
        }
    }
}
