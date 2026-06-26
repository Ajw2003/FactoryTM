using Buildings;
using Managers;

namespace StateMachine
{
    public class SetupAutomationTutorialState : BaseTutorialState
    {
        public override TutorialState StateId => TutorialState.SetupAutomation;

        public SetupAutomationTutorialState(TutorialManager manager) : base(manager) {}

        public override void Enter()
        {
            if (InterDimensionalTransporter.Instance != null && !manager.isAutomaticSaleSubscribed)
            {
                InterDimensionalTransporter.Instance.OnItemSold += manager.HandleAutomaticSale;
                manager.isAutomaticSaleSubscribed = true;
            }
            base.Enter();
        }

        public override void CheckTransitions()
        {
            if (InterDimensionalTransporter.Instance != null && !manager.isAutomaticSaleSubscribed)
            {
                InterDimensionalTransporter.Instance.OnItemSold += manager.HandleAutomaticSale;
                manager.isAutomaticSaleSubscribed = true;
            }

            if (manager.hasSoldAutomatically)
            {
                manager.ChangeState(manager.earnAndExpandState);
            }
        }

        public override void Exit()
        {
            if (DayNightManager.Instance != null)
            {
                DayNightManager.Instance.isTutorialActive = false;
                DayNightManager.Instance.timeRemaining = 25f; // Night in 25s
            }
        }
    }
}
