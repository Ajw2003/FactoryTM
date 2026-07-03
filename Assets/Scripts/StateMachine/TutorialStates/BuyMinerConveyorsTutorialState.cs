using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
using Buildings;

namespace StateMachine
{
    public class BuyMinerConveyorsTutorialState : BaseTutorialState
    {
        public override TutorialState StateId => TutorialState.BuyMinerConveyors;

        public BuyMinerConveyorsTutorialState(TutorialManager manager) : base(manager) {}

        public override void CheckTransitions()
        {
            int minerCount = manager.GetInventoryCount(BuildingType.Miner);
            int conveyorCount = manager.GetInventoryCount(BuildingType.Conveyor);
            bool isStoreOpen = UiManager.Instance != null && UiManager.Instance.StorePanel != null && UiManager.Instance.StorePanel.activeSelf;
            if (minerCount >= 1 && conveyorCount >= 10 && !isStoreOpen)
            {
                manager.ChangeState(manager.setupAutomationState);
            }
        }
    }
}

