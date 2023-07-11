public struct PowerUpUnlockedEvent : IEvent
{
    public PowerUpType PowerUpType { get; private set; }

    public PowerUpUnlockedEvent(PowerUpType powerUpType)
    {
        PowerUpType = powerUpType;
    }
}
