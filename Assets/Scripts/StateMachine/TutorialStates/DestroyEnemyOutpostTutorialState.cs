using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
namespace StateMachine
{
    public class DestroyEnemyOutpostTutorialState : BaseTutorialState
    {
        public override TutorialState StateId => TutorialState.DestroyEnemyOutpost;

        public DestroyEnemyOutpostTutorialState(TutorialManager manager) : base(manager) {}

        public override void Enter()
        {
            manager.SetupOutpostPhase();
            base.Enter();
        }

        public override void CheckTransitions()
        {
            manager.SetupOutpostPhase();

            if (manager.TutorialOutpost != null)
            {
                manager.TutorialOutpost.buildings.RemoveAll(b => b == null);
                if (manager.TutorialOutpost.buildings.Count == 0)
                {
                    manager.MarkOutpostCleared();
                }
            }

            if (manager.HasClearedOutpost)
            {
                manager.ChangeState(manager.buyMinerConveyorsState);
            }
        }
    }
}

