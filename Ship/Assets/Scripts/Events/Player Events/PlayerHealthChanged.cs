public struct PlayerHealthChanged : IEvent
{
    public float Current;
    public float Max;

    public PlayerHealthChanged(float current, float max)
    {
        Current = current;
        Max = max;
    }
}