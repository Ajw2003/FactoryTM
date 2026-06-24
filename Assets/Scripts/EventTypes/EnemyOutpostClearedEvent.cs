using EventSystems;

public class EnemyOutpostClearedEvent : IEvent
{
    public EnemyOutpost Outpost { get; }

    public EnemyOutpostClearedEvent(EnemyOutpost outpost)
    {
        Outpost = outpost;
    }
}
