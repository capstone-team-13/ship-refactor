using UnityEngine.InputSystem;

public struct PlayerExitedEvent : IEvent
{
    public int Index;
    public InputDevice Device;

    public PlayerExitedEvent(int index, InputDevice device)
    {
        Index = index;
        Device = device;
    }
}