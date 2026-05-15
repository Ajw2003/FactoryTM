using System;
using UnityEngine;

namespace StateMachine
{
    public abstract class BaseStateMachine : MonoBehaviour
    {
        protected IState CurrentState { get; set; }
        protected string CurrentStateName;

        public virtual void ChangeState(IState newState)// Change state with a pass through for the IState Interface 
        {
            if (newState == CurrentState)
                return;
            
            CurrentState = newState;
            CurrentState?.Enter();
            CurrentStateName = CurrentState?.ToString();
        }

        public virtual void Update()
        {
            CurrentState?.Update();
        }
        
        public virtual void FixedUpdate()
        {
            CurrentState?.FixedUpdate();
        }
        
    }
}
