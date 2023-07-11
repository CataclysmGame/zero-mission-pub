public class PlayerHealthChangedEvent : IEvent
{
    public int CurrentHP { get; private set; }

    public PlayerHealthChangedEvent(int currentHP)
    {
        CurrentHP = currentHP;
    }
}
