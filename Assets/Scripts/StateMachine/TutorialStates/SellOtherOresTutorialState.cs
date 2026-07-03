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
    public class SellOtherOresTutorialState : BaseTutorialState
    {
        public override TutorialState StateId => TutorialState.SellOtherOres;

        public SellOtherOresTutorialState(TutorialManager manager) : base(manager) {}

        public override void CheckTransitions()
        {
            float currentMoney = CurrencyManager.Instance != null ? CurrencyManager.Instance.currentCurrencyValue : 0f;
            if (currentMoney >= 50f)
            {
                manager.ChangeState(manager.buyFirstWeaponState);
            }
        }
    }
}

