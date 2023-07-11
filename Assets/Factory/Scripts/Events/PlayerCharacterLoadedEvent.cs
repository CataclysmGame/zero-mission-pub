namespace Factory.Scripts.Events
{
    public struct PlayerCharacterLoadedEvent : IEvent
    {
        public Character Character { get; private set; }

        public PlayerCharacterLoadedEvent(Character character)
        {
            Character = character;
        }
    }
}