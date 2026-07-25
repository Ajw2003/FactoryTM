using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
namespace StateMachine
{
    public class NotStartedTutorialState : BaseTutorialState
    {
        public override TutorialState StateId => TutorialState.NotStarted;

        public NotStartedTutorialState(TutorialManager manager) : base(manager) {}
    }
}

