using JetBrains.Annotations;
using UnityEngine;

public class ShootController : MonoBehaviour
{
    #region Editor API

    [SerializeField] private ShooterModel m_model;
    [SerializeField] private FollowObject m_followObject;

    #endregion

    [UsedImplicitly]
    private void Start()
    {
        __M_RaiseInitialEvents();
    }

    [UsedImplicitly]
    private void OnDisable()
    {
        LevelManager.PlayerEventBus.UnsubscribeFromTarget<PlayerReloadedEvent>(Owner, OnPlayerReloaded);
    }

    #region API

    public float ReloadingTime => m_model.ReloadingTime;

    public GameObject Owner
    {
        get => m_owner;
        set => __M_SetOwner(value);
    }

    public bool HasEnoughAmmo => __M_HasEnoughAmmo();
    public bool NotInCoolingDown => __M_NotInCoolingDown();

    public void Follow(Transform target)
    {
        m_followObject.Target = target;
    }

    public void Shoot()
    {
        if (!NotInCoolingDown || !HasEnoughAmmo) return;

        ShooterModel model = m_model;
        model.RemainingAmmo -= model.AmmoConsumption;
        ProjectileController projectile = ProjectileManager.Rent(model.projectileType);
        projectile.transform.position = model.LaunchPoint.position;

        Vector3 launchForce = Vector3.Scale(m_followObject.FacingDirection, model.LaunchForce);

        projectile.Launch(launchForce);

        model.LastAttackTime = Time.time;

        LevelManager.PlayerEventBus.Raise(new PlayerAmmoChangedEvent(model.RemainingAmmo, model.MaxAmmo),
            Owner, Owner);
    }

    #endregion

    #region Event Handlers

    private void OnPlayerReloaded(ref PlayerReloadedEvent eventData, GameObject target, GameObject source)
    {
        ShooterModel model = m_model;
        model.RemainingAmmo = model.MaxAmmo;

        LevelManager.PlayerEventBus.Raise(new PlayerAmmoChangedEvent(model.RemainingAmmo, model.MaxAmmo),
            Owner, Owner);
    }

    #endregion

    #region Internal

    private GameObject m_owner;

    private void __M_SetOwner(GameObject owner)
    {
        if (Owner != null)
            LevelManager.PlayerEventBus.UnsubscribeFromTarget<PlayerReloadedEvent>(Owner, OnPlayerReloaded);

        m_owner = owner;
        LevelManager.PlayerEventBus.SubscribeToTarget<PlayerReloadedEvent>(Owner, OnPlayerReloaded);
    }

    public bool __M_HasEnoughAmmo()
    {
        ShooterModel model = m_model;
        bool hasEnoughAmmo = model.RemainingAmmo >= model.AmmoConsumption;
        return hasEnoughAmmo;
    }

    public bool __M_NotInCoolingDown()
    {
        ShooterModel model = m_model;
        bool notInCoolingDown = Time.time - model.LastAttackTime >= model.AttackCooldown;
        return notInCoolingDown;
    }

    private void __M_RaiseInitialEvents()
    {
        LevelManager.PlayerEventBus.Raise(new PlayerAmmoChangedEvent(m_model.RemainingAmmo, m_model.MaxAmmo),
            Owner, Owner);
    }

    #endregion
}