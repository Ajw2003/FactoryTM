using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
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
            if (InterDimensionalTransporter.Instance != null && !manager.IsAutomaticSaleSubscribed)
            {
                InterDimensionalTransporter.Instance.OnItemSold += manager.HandleAutomaticSale;
                manager.MarkAutomaticSaleSubscribed();
            }
            base.Enter();
        }

        public override void CheckTransitions()
        {
            if (InterDimensionalTransporter.Instance != null && !manager.IsAutomaticSaleSubscribed)
            {
                InterDimensionalTransporter.Instance.OnItemSold += manager.HandleAutomaticSale;
                manager.MarkAutomaticSaleSubscribed();
            }

            if (manager.HasSoldAutomatically)
            {
                manager.ChangeState(manager.earnAndExpandState);
            }
        }

        public override void Exit()
        {
            if (DayNightManager.Instance != null)
            {
                DayNightManager.Instance.SetTutorialActive(false);
                DayNightManager.Instance.SetTimeRemaining(25f); // Night in 25s
            }
        }
    }
}

