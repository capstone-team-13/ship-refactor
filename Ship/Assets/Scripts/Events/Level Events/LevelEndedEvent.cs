public struct LevelEndedEvent : IEvent
{
    public string Name;

    public LevelEndedEvent(string name)
    {
        Name = name;
    }
}