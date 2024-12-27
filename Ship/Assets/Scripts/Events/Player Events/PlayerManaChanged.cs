public struct PlayerManaChanged : IEvent
{
    public float Current;
    public float Max;

    public PlayerManaChanged(float current, float max)
    {
        Current = current;
        Max = max;
    }
}