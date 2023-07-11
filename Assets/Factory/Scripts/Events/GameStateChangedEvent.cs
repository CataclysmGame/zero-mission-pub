public struct GameStateChangedEvent : IEvent
{
    public GameState PrevState { get; private set; }
    public GameState State { get; private set; }

    public GameStateChangedEvent(GameState prevState, GameState state)
    {
        PrevState = prevState;
        State = state;
    }
}
