namespace StateMachine
{
    public class NotStartedTutorialState : BaseTutorialState
    {
        public override TutorialState StateId => TutorialState.NotStarted;

        public NotStartedTutorialState(TutorialManager manager) : base(manager) {}
    }
}
