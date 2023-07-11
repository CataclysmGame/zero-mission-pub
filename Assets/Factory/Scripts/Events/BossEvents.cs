public class BossActivatedEvent : IEvent
{
    public BossController Boss { get; private set; }

    public BossActivatedEvent(BossController boss)
    {
        Boss = boss;
    }
}

public class BossDiedEvent : IEvent
{
    public BossController Boss { get; private set; }

    public BossDiedEvent(BossController boss)
    {
        Boss = boss;
    }
}

public class BossHealthChangedEvent : IEvent
{
    public BossController Boss { get; private set; }
    public int CurrentHP { get; private set; }

    public BossHealthChangedEvent(BossController boss, int currentHP)
    {
        Boss = boss;
        CurrentHP = currentHP;
    }
}
