using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using UnityEngine;

namespace StateMachine
{
    public class FuelDCTTutorialState : BaseTutorialState
    {
        public override TutorialState StateId => TutorialState.FuelDCT;

        public FuelDCTTutorialState(TutorialManager manager) : base(manager) {}

        public override void Enter()
        {
            TrySubscribeFuel();
            base.Enter();
        }

        // The IDT is spawned at runtime, so it may not exist yet when this state is entered.
        // Same late-binding pattern SetupAutomationTutorialState uses for OnItemSold.
        private void TrySubscribeFuel()
        {
            if (InterDimensionalTransporter.Instance != null && !manager.IsFuelSubscribed)
            {
                InterDimensionalTransporter.Instance.OnFuelAdded += manager.HandleFuelAdded;
                manager.MarkFuelSubscribed();
            }
        }

        public override void CheckTransitions()
        {
            TrySubscribeFuel();

            if (manager.CoalFedCount >= 5)
            {
                manager.ChangeState(manager.sellOtherOresState);
                
                if (UiManager.HasInstance)
                {
                    UiManager.Instance.ShowGeneralAlert("IDT ONLINE - COMMERCIAL PORT ONLINE", new Color(0.2f, 1f, 0.2f));
                }
            }
        }
    }
}

