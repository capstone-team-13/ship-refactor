using UnityEngine.InputSystem;

public struct PlayerJoinedEvent : IEvent
{
    public int Index;
    public InputDevice Device;

    public PlayerJoinedEvent(int index, InputDevice device)
    {
        Index = index;
        Device = device;
    }
}