public struct PlayerAmmoChangedEvent : IEvent
{
    public int RemainingAmmo;
    public int MaxAmmo;

    public PlayerAmmoChangedEvent(int remainingAmmo, int maxAmmo)
    {
        RemainingAmmo = remainingAmmo;
        MaxAmmo = maxAmmo;
    }
}