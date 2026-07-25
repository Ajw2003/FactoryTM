using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
namespace StateMachine
{
    public class CompletedTutorialState : BaseTutorialState
    {
        public override TutorialState StateId => TutorialState.Completed;

        public CompletedTutorialState(TutorialManager manager) : base(manager) {}
    }
}

