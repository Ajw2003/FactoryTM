using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
using StateMachine;

namespace StateMachine
{
    public abstract class BaseTutorialState : IState
    {
        protected TutorialManager manager;

        public abstract TutorialState StateId { get; }

        public BaseTutorialState(TutorialManager manager)
        {
            this.manager = manager;
        }

        public virtual void Enter()
        {
            manager.UpdateObjectiveText();
        }

        public virtual void Update()
        {
            CheckTransitions();
        }

        public virtual void Exit() {}

        public virtual void FixedUpdate() {}

        public virtual void CheckTransitions() {}
    }
}

