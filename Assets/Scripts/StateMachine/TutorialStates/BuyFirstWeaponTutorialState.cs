using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
namespace StateMachine
{
    public class BuyFirstWeaponTutorialState : BaseTutorialState
    {
        public override TutorialState StateId => TutorialState.BuyFirstWeapon;

        public BuyFirstWeaponTutorialState(TutorialManager manager) : base(manager) {}

        public override void Enter()
        {
            manager.UnlockWeaponsInShop();
            base.Enter();
        }

        public override void CheckTransitions()
        {
            manager.UnlockWeaponsInShop();
            if (manager.hasPurchasedWeapon)
            {
                manager.ChangeState(manager.buyAmmoHealthState);
            }
        }
    }
}

