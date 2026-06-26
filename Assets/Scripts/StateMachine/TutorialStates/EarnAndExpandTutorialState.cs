using Managers;

namespace StateMachine
{
    public class EarnAndExpandTutorialState : BaseTutorialState
    {
        public override TutorialState StateId => TutorialState.EarnAndExpand;

        public EarnAndExpandTutorialState(TutorialManager manager) : base(manager) {}

        public override void CheckTransitions()
        {
            if (DayNightManager.Instance != null && DayNightManager.Instance.currentPhase == CyclePhase.Evening)
            {
                manager.ChangeState(manager.defendFirstRaidState);
            }
        }
    }
}
