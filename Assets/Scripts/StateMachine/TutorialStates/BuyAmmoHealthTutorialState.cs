namespace StateMachine
{
    public class BuyAmmoHealthTutorialState : BaseTutorialState
    {
        public override TutorialState StateId => TutorialState.BuyAmmoHealth;

        public BuyAmmoHealthTutorialState(TutorialManager manager) : base(manager) {}

        public override void CheckTransitions()
        {
            bool isStoreOpen = UiManager.Instance != null && UiManager.Instance.StorePanel != null && UiManager.Instance.StorePanel.activeSelf;
            if (manager.hasPurchasedAmmo && manager.hasPurchasedHealthPack && !isStoreOpen)
            {
                manager.ChangeState(manager.destroyOutpostState);
            }
        }
    }
}
