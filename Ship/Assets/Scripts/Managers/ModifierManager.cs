using ModiBuff.Core;

public class ModifierManager : SingletonBehaviour<ModifierManager>
{
    private ModifierIdManager m_modifierIdManager;

    private MyModifierRecipes m_modifierRecipes;

    // Singleton Instance
    private ModifierPool m_modifierPool;

    // Singleton Instance
    private ModifierControllerPool m_modifierControllerPool;

    // Singleton Instance
    private EffectTypeIdManager m_effectTypeIdManager;

    protected override void Awake()
    {
        base.Awake();

        m_effectTypeIdManager = new EffectTypeIdManager();
        m_effectTypeIdManager.RegisterEffectTypes(typeof(DamageEffect), typeof(ManaGrowEffect));

        m_modifierIdManager = new ModifierIdManager();
        m_modifierRecipes = new MyModifierRecipes(m_modifierIdManager);

        m_modifierPool = new ModifierPool(m_modifierRecipes);

        m_modifierControllerPool = new ModifierControllerPool();
    }

    public bool TryAddApplierByName(ref IModifierApplierOwner owner, string applierName,
        ApplierType applierType, bool hasApplyChecks = false)
    {
        return owner.ModifierApplierController.TryAddApplier(
            m_modifierIdManager.GetId(applierName), hasApplyChecks, applierType);
    }

    public bool TryAddApplier(ref IModifierApplierOwner owner, int id, ApplierType applierType,
        bool hasApplyChecks = false)
    {
        return owner.ModifierApplierController.TryAddApplier(id, hasApplyChecks, applierType);
    }

    public int GetModifierId(string applierName)
    {
        return m_modifierIdManager.GetId(applierName);
    }

    public ModifierInfo GetModifierInfo(int id)
    {
        return m_modifierRecipes.GetModifierInfo(id);
    }

    // private void Start()
    // {
    //     m_shipModel.ModifierApplierController.TryAddApplierByName(
    //         m_modifierIdManager.GetId(Modifiers.DAMAGE_OVER_TIME), false, ApplierType.Cast);
    //
    //     m_shipModel.TryCast(modifierIds[0], m_shipModel);
    //
    //     var modifierIds = m_shipModel.ModifierApplierController.GetApplierCastModifierIds();
    //     for (int i = 0; i < modifierIds.Count; i++)
    //     {
    //         ModifierInfo modifierInfo = m_modifierRecipes.GetModifierInfo(modifierIds[i]);
    //         Debug.Log($"{i + 1} - {modifierInfo.DisplayName} - {modifierInfo.Description}");
    //     }
    //
    //     m_shipModel.TryCast(modifierIds[0], m_shipModel);
    // }
}