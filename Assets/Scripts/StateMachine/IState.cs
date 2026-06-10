using UnityEngine;

namespace StateMachine
{
    public interface IState
    {
        void Enter();
        void Update();
        void Exit();
        void FixedUpdate();
    }
}

