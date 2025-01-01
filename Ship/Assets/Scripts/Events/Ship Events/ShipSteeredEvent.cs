public struct ShipSteeredEvent : IEvent
{
    public float DeltaX;

    public ShipSteeredEvent(float deltaX)
    {
        DeltaX = deltaX;
    }
}