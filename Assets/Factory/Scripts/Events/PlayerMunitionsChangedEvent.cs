public struct PlayerMunitionsChangedEvent : IEvent
{
    public int CurMunitions { get; private set; }
    public int MaxMunitions { get; private set; }

    public PlayerMunitionsChangedEvent(int curMunitions, int maxMunitions)
    {
        CurMunitions = curMunitions;
        MaxMunitions = maxMunitions;
    }
}
