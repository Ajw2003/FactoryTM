using UnityEngine;

namespace StateMachine
{
    public class FuelDCTTutorialState : BaseTutorialState
    {
        public override TutorialState StateId => TutorialState.FuelDCT;

        public FuelDCTTutorialState(TutorialManager manager) : base(manager) {}

        public override void CheckTransitions()
        {
            if (manager.coalFedCount >= 5)
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
