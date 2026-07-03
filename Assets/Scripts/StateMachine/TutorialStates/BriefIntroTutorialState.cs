using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
using Buildings;
using Managers;

namespace StateMachine
{
    public class BriefIntroTutorialState : BaseTutorialState
    {
        public override TutorialState StateId => TutorialState.BriefIntro;

        public BriefIntroTutorialState(TutorialManager manager) : base(manager) {}

        public override void CheckTransitions()
        {
            if (BuildingUiManager.Instance != null && BuildingUiManager.Instance.CurrentOpenBuilding is InterDimensionalTransporter)
            {
                manager.ChangeState(manager.mineCoalState);
            }
        }
    }
}

