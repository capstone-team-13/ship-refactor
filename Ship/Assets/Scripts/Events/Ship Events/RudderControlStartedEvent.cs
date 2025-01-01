using UnityEngine;

public struct RudderControlStartedEvent : IEvent
{
    public Transform RudderControlPoint;

    public RudderControlStartedEvent(Transform rudderControlPoint)
    {
        RudderControlPoint = rudderControlPoint;
    }
}