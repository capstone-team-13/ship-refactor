using UnityEngine;

public struct ShipMoveEvent : IEvent
{
    public Vector3 TargetPosition;

    public ShipMoveEvent(Vector3 targetPosition)
    {
        TargetPosition = targetPosition;
    }
}