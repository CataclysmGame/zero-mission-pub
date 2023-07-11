namespace Factory.Scripts.Events
{
    public struct PlayerReloadStartedEvent : IEvent
    {
        public int CurrentMunitions { get; private set; }
        
        public int MaxMunitions { get; private set; }
        
        public float Duration { get; private set; }

        public PlayerReloadStartedEvent(int currentMunitions, int maxMunitions, float duration)
        {
            CurrentMunitions = currentMunitions;
            MaxMunitions = maxMunitions;
            Duration = duration;
        }
    }
}