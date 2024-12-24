using ModiBuff.Core;
using ModiBuff.Core.Units.Interfaces.NonGeneric;

public class DamageEffect : IEffect, IPostEffectOwner<DamageEffect, float>
{
    private readonly float m_value;
    private IPostEffect<float>[] m_postEffects;

    public DamageEffect(float value)
    {
        m_value = value;
    }

    public void Effect(IUnit target, IUnit source)
    {
        float effectiveDamage = ((IDamagable)target).TakeDamage(m_value, source);

        if (m_postEffects == null) return;
        foreach (var postEffect in m_postEffects)
            postEffect.Effect(effectiveDamage, target, source);
    }

    public DamageEffect SetPostEffects(params IPostEffect<float>[] postEffects)
    {
        m_postEffects = postEffects;
        return this;
    }
}