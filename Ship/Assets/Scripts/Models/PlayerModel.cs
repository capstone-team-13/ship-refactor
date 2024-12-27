using JetBrains.Annotations;
using ModiBuff.Core;
using ModiBuff.Core.Units;
using ModiBuff.Core.Units.Interfaces.NonGeneric;
using UnityEngine;
using IDamagable = ModiBuff.Core.Units.Interfaces.NonGeneric.IDamagable;

public class PlayerModel : MonoBehaviour, IModifierOwner, IModifierApplierOwner,
    IStatusEffectOwner<LegalAction, StatusEffectType>,
    IDamagable, IHealable, IKillable, IManaOwner, IManaGrowable
{
    public ModifierController ModifierController { get; private set; }

    public IMultiInstanceStatusEffectController<LegalAction, StatusEffectType> StatusEffectController
    {
        get;
        private set;
    }

    public ModifierApplierController ModifierApplierController { get; private set; }

    [Header("Health")]
    [SerializeField]
    [Tooltip("Maximum health of the player. This value determines the player's health cap.")]
    private float m_maxHealth;

    public float Health { get; set; }

    public float MaxHealth
    {
        get => m_maxHealth;
        set => m_maxHealth = value;
    }

    public bool IsDead { get; set; }

    [Header("Mana")]
    [SerializeField]
    [Tooltip("The player's current mana. Used to set the initial mana value at the start of the game.")]
    private float m_mana;

    [SerializeField] [Tooltip("The player's maximum mana. Determines the upper limit for the mana pool.")]
    private float m_maxMana;

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

    [Header("Movement")] public float Speed;
    public Vector3 Direction;

    [Header("Jump")] public int MaxJumpCount;
    public int JumpCount { get; set; }
    public Vector3 JumpForce { get; set; }
    public Vector3 MaxJumpForce;
    [Range(0f, 1f)] public float JumpForceDecayRate = 0.5f;
    public bool JumpPressed { get; set; }

    [Header("Melee")] public float MeleeRadius = 3.0f;

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

        StatusEffectController = new MultiInstanceStatusEffectController(this);

        __M_AddAppliers();

        // Mana Grow
        ModifierManager modifierManager = ModifierManager.Instance;
        this.TryCast(modifierManager.GetModifierId(Buffs.Mana.NATURAL_GROW), this);
        ResetJumpCount();

        Health = 0;
    }

    [UsedImplicitly]
    private void Start()
    {
        __M_RaiseInitialEvents();
    }

    [UsedImplicitly]
    private void Update()
    {
        try
        {
            // Call the problematic method in the DLL
            StatusEffectController.Update(Time.deltaTime);
        }
        catch (System.InvalidOperationException ex)
        {
            // In .NET versions lower than 5.0, modifying the values of a Dictionary
            // while iterating through it using foreach can also trigger an InvalidOperationException.
            // This happens because the Dictionary internally maintains a version number (version),
            // which increments on any modification (including updating values).
            // If the version changes during enumeration, an InvalidOperationException is thrown.
            Debug.LogWarning($"Caught an exception during Update: {ex.Message}");
        }

        ModifierController.Update(Time.deltaTime);
        ModifierApplierController.Update(Time.deltaTime);
    }

    [UsedImplicitly]
    private void OnDestroy()
    {
        ModifierControllerPool.Instance.Return(ModifierController);
        ModifierControllerPool.Instance.ReturnApplier(ModifierApplierController);
    }

    #endregion

    #region API

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

        LevelManager.PlayerEventBus.Raise(new PlayerHealthChanged(Health, MaxHealth), gameObject, gameObject);

        return effectiveDamage;
    }

    public float Heal(float healAmount, IUnit source)
    {
        if (IsDead) return 0;

        float initialHealth = Health;
        Health += healAmount;
        Health = Mathf.Min(Health, MaxHealth);

        float effectiveHealing = Health - initialHealth;

        LevelManager.PlayerEventBus.Raise(new PlayerHealthChanged(Health, MaxHealth), gameObject, gameObject);

        return effectiveHealing;
    }

    public void UseMana(float value)
    {
        if (IsDead) return;

        Mana -= value;
        Mana = Mathf.Max(Mana, 0);

        LevelManager.PlayerEventBus.Raise(new PlayerManaChanged(Mana, MaxMana), gameObject, gameObject);
    }

    public float GrowMana(float manaAmount, IUnit source)
    {
        if (IsDead) return 0;

        float initialMana = Mana;
        Mana += manaAmount;
        Mana = Mathf.Min(Mana, MaxMana);

        float effectiveManaGrowing = Mana - initialMana;

        LevelManager.PlayerEventBus.Raise(new PlayerManaChanged(Mana, MaxMana), gameObject, gameObject);

        return effectiveManaGrowing;
    }

    public void ResetJumpCount()
    {
        JumpCount = MaxJumpCount;
    }

    public void HandleJump()
    {
        int power = MaxJumpCount - JumpCount;
        float forceDecayFactor = Mathf.Pow(JumpForceDecayRate, power);
        JumpForce = MaxJumpForce * forceDecayFactor;
        JumpPressed = true;
    }

    #endregion

    #region Event Handlers

    private void OnPlayerMoved(ref PlayerMoveEvent eventData, GameObject target, GameObject source)
    {
        Direction = eventData.Direction;
    }

    #endregion

    #region Internal

    private void __M_AddAppliers()
    {
        ModifierManager modifierManager = ModifierManager.Instance;
        IModifierApplierOwner applierOwner = this;

        modifierManager.TryAddApplierByName(ref applierOwner, Casts.VITAL_SURGE, ApplierType.Cast, true);
        modifierManager.TryAddApplierByName(ref applierOwner, Buffs.Mana.NATURAL_GROW, ApplierType.Cast);
    }

    private void __M_RaiseInitialEvents()
    {
        LevelManager.PlayerEventBus.Raise(new PlayerHealthChanged(Health, MaxHealth), gameObject, gameObject);
        LevelManager.PlayerEventBus.Raise(new PlayerManaChanged(Mana, MaxMana), gameObject, gameObject);
    }

    #endregion
}