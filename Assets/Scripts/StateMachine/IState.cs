using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
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


