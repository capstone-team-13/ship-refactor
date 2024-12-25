public struct LevelCountingDownEvent : IEvent
{
    public float remainingTime;

    public LevelCountingDownEvent(float remainingTime)
    {
        this.remainingTime = remainingTime;
    }
}