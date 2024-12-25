using JetBrains.Annotations;
using ModiBuff.Core;
using ModiBuff.Core.Units;
using ModiBuff.Core.Units.Interfaces.NonGeneric;
using UnityEngine;
using IDamagable = ModiBuff.Core.Units.Interfaces.NonGeneric.IDamagable;

public class PlayerModel : MonoBehaviour, IModifierOwner, ISingleStatusEffectOwner, IModifierApplierOwner,
    IManaOwner, IDamagable, IHealable, IKillable
{
    public ModifierController ModifierController { get; private set; }

    public ISingleInstanceStatusEffectController<LegalAction, StatusEffectType> StatusEffectController
    {
        get;
        private set;
    }

    public ModifierApplierController ModifierApplierController { get; private set; }

    public int Index;

    [Header("Health")]
    [SerializeField]
    [Tooltip("Maximum health of the player. This value determines the player's health cap.")]
    private float m_maxHealth;

    public float Health { get; set; }

    [Tooltip("max health of the player")]
    public float MaxHealth
    {
        get => m_maxHealth;
        set => m_maxHealth = value;
    }

    [Header("Mana")]
    [SerializeField]
    [Tooltip("The player's current mana. Used to set the initial mana value at the start of the game.")]
    private float m_mana;

    [SerializeField] [Tooltip("The player's maximum mana. Determines the upper limit for the mana pool.")]
    private float m_maxMana;


    [Header("Movement")] public float Speed;
    public Vector3 Direction;

    public float Mana
    {
        get => m_mana;
        private set => m_mana = value;
    }

    public float MaxMana
    {
        get => m_maxMana;
        private set => m_maxMana = value;
    }

    public bool IsDead { get; set; }

    public float TakeDamage(float damage, IUnit source)
    {
        if (IsDead) return 0;

        float initialHealth = Health;
        Health -= damage;
        Health = Mathf.Max(Health, 0);

        if (Health <= 0)
        {
            IsDead = true;
            ModifierControllerPool.Instance.Return(ModifierController);
        }

        float effectiveDamage = initialHealth - Health;
        return effectiveDamage;
    }

    public float Heal(float healAmount, IUnit source)
    {
        if (IsDead) return 0;

        float initialHealth = Health;
        Health += healAmount;
        Health = Mathf.Min(Health, MaxHealth);

        float effectiveHealing = Health - initialHealth;

        return effectiveHealing;
    }

    public void UseMana(float value)
    {
        throw new System.NotImplementedException();
    }

    #region Unity Callbacks

    [UsedImplicitly]
    private void OnEnable()
    {
        LevelManager.PlayerEventBus.SubscribeToTarget<PlayerMoveEvent>(gameObject, OnPlayerMoved);
    }


    [UsedImplicitly]
    private void OnDisable()
    {
        LevelManager.PlayerEventBus.UnsubscribeFromTarget<PlayerMoveEvent>(gameObject, OnPlayerMoved);
    }

    [UsedImplicitly]
    protected void Awake()
    {
        ModifierController = ModifierControllerPool.Instance.Rent();
        ModifierApplierController = ModifierControllerPool.Instance.RentApplier();

        if (ModifierController == null || ModifierApplierController == null)
        {
            string errorMessage = "Failed to initialize " + nameof(ModifierController) + " or " +
                                  nameof(ModifierApplierController) + ". " + "Ensure that a '" +
                                  nameof(ModifierManager) + "' is present in the scene.";
            throw new UnityException(errorMessage);
        }

        StatusEffectController = new StatusEffectController();
    }

    [UsedImplicitly]
    private void Update()
    {
        ModifierController.Update(Time.deltaTime);
        ModifierApplierController.Update(Time.deltaTime);
        StatusEffectController.Update(Time.deltaTime);
    }

    #endregion

    #region Event Handlers

    private void OnPlayerMoved(ref PlayerMoveEvent eventData, GameObject target, GameObject source)
    {
        Direction = eventData.Direction;
    }

    #endregion
}