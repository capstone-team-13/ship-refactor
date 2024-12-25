public struct LevelStartedEvent : IEvent
{
    public string Name;

    public LevelStartedEvent(string name)
    {
        Name = name;
    }
}