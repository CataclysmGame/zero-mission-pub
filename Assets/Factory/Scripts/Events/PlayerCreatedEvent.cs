public struct PlayerCreatedEvent : IEvent
{
    public PlayerController Player { get; private set; }

    public PlayerCreatedEvent(PlayerController player)
    {
        Player = player;
    }
}
