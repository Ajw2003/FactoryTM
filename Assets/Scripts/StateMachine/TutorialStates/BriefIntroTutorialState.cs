using Managers;
using Placeables;


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

