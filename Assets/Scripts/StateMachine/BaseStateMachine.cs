using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using System;
using UnityEngine;

namespace StateMachine
{
    public abstract class BaseStateMachine : MonoBehaviour
    {
        public IState CurrentState { get; set; }
        public string currentStateName;

        public virtual void ChangeState(IState newState)// Change state with a pass through for the IState Interface
        {
            if (newState == CurrentState)
                return;

            IState oldState = CurrentState;
            CurrentState = newState;
            currentStateName = CurrentState?.ToString();

            oldState?.Exit();
            CurrentState?.Enter();
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

