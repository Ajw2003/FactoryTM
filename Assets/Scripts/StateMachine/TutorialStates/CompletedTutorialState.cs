namespace StateMachine
{
    public class CompletedTutorialState : BaseTutorialState
    {
        public override TutorialState StateId => TutorialState.Completed;

        public CompletedTutorialState(TutorialManager manager) : base(manager) {}
    }
}
