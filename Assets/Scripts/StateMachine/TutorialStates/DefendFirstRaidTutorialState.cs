using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
using Managers;

namespace StateMachine
{
    public class DefendFirstRaidTutorialState : BaseTutorialState
    {
        public override TutorialState StateId => TutorialState.DefendFirstRaid;

        public DefendFirstRaidTutorialState(TutorialManager manager) : base(manager) {}

        public override void CheckTransitions()
        {
            if (DayNightManager.Instance != null && DayNightManager.Instance.currentPhase == CyclePhase.UpgradePhase)
            {
                manager.ChangeState(manager.completedState);
            }
        }

        public override void Exit()
        {
            manager.CompleteTutorial(true);
        }
    }
}

