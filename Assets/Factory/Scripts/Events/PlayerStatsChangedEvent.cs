public struct PlayerStatsChanged : IEvent
{
    public PowerUpType? PowerUpType { get; private set; }
    public float Stat { get; private set; }

    public PlayerStatsChanged(PowerUpType? powerUpType, float stat)
    {
        PowerUpType = powerUpType;
        Stat = stat;
    }
}
