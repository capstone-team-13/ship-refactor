using JetBrains.Annotations;
using UnityEngine;

public enum ProjectileType
{
    None = 0,
    Regular,
    Bouncing,
    Homing,
    Timed
}

public class ShooterModel : MonoBehaviour
{
    [HideInInspector] public int RemainingAmmo;
    [Header("Ammo Settings")] public int MaxAmmo;
    public int AmmoConsumption = 1;

    [Header("Attack Settings")] public float AttackSpeed = 1f;
    public float AttackCooldown { get; set; }
    public float LastAttackTime { get; set; }

    public int ReloadingTime;

    [Header("Description")] public string Description;

    [Header("Projectile Settings")] public ProjectileType projectileType;

    [Header("Launch Settings")] public Vector3 LaunchForce;
    public Transform LaunchPoint;

    [UsedImplicitly]
    private void Awake()
    {
        RemainingAmmo = MaxAmmo;

        AttackCooldown = 1f / AttackSpeed;
        LastAttackTime = -AttackCooldown;
    }
}