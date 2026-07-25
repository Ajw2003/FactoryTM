using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using Managers;

namespace StateMachine
{
    public class MineCoalManuallyTutorialState : BaseTutorialState
    {
        public override TutorialState StateId => TutorialState.MineCoalManually;

        public MineCoalManuallyTutorialState(TutorialManager manager) : base(manager) {}

        public override void CheckTransitions()
        {
            int coalCount = BuildingUiManager.Instance != null ? BuildingUiManager.Instance.GetResourceCount(specificItemType.Coal) : 0;
            if (coalCount >= 5)
            {
                manager.ChangeState(manager.fuelDCTState);
            }
        }
    }
}

