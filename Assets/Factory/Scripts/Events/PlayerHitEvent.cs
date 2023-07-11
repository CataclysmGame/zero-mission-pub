public struct PlayerHitEvent : IEvent
{
    public int DamageReceived { get; private set; }

    public PlayerHitEvent(int damageReceived)
    {
        DamageReceived = damageReceived;
    }
}
