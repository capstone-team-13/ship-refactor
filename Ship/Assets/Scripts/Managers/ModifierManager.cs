using JetBrains.Annotations;
using ModiBuff.Core;
using UnityEngine;

public class ModifierManager : SingletonBehaviour<ModifierManager>
{
    private MyModifierRecipes m_modifierRecipes;

    // Singleton Instance
    private ModifierPool m_modifierPool;

    // Singleton Instance
    private ModifierControllerPool m_modifierControllerPool;

    private EffectIdManager m_effectIdManager;

    // Singleton Instance
    private ModifierLessEffects m_modifierLessEffects;

    public ModifierIdManager ModifierIdManager { get; private set; }

    [UsedImplicitly]
    protected override void Awake()
    {
        base.Awake();

        m_effectIdManager = new EffectIdManager();
        m_modifierLessEffects = new ModifierLessEffects(m_effectIdManager);
        ModifierLessEffects.Instance.Finish();

        ModifierIdManager = new ModifierIdManager();
        m_modifierRecipes = new MyModifierRecipes(ModifierIdManager);

        m_modifierPool = new ModifierPool(m_modifierRecipes);

        m_modifierControllerPool = new ModifierControllerPool();
    }

    public bool TryAddApplierByName(ref IModifierApplierOwner owner, string applierName,
        ApplierType applierType, bool hasApplyChecks = false)
    {
        return owner.ModifierApplierController.TryAddApplier(
            ModifierIdManager.GetId(applierName), hasApplyChecks, applierType);
    }

    public bool TryAddModifierLessEffect(string effectName, IEffect[] effects)
    {
        Debug.Log("@TryAddModifierLessEffect");
        var result = ModifierLessEffects.Instance.Add(effectName, effects);

        return result;
    }

    public void ApplyModifierLessEffect(int id, IUnit target, IUnit source)
    {
        ModifierLessEffects.Instance.Apply(id, target, source);
    }

    public void ApplyModifierLessEffectByName(string effectName, IUnit target, IUnit source)
    {
        ApplyModifierLessEffect(m_effectIdManager.GetId(effectName), target, source);
    }

    public bool TryAddApplier(ref IModifierApplierOwner owner, int id, ApplierType applierType,
        bool hasApplyChecks = false)
    {
        return owner.ModifierApplierController.TryAddApplier(id, hasApplyChecks, applierType);
    }

    public int GetModifierId(string applierName)
    {
        return ModifierIdManager.GetId(applierName);
    }

    public int GetModifierLessEffectId(string effectName)
    {
        return m_effectIdManager.GetId(effectName);
    }

    public ModifierInfo GetModifierInfo(int id)
    {
        return m_modifierRecipes.GetModifierInfo(id);
    }
}