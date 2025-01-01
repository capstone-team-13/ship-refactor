using ModiBuff.Core;

public class ManaGrowEffect : IEffect
{
    private readonly float m_value;

    public ManaGrowEffect(float value)
    {
        m_value = value;
    }

    public void Effect(IUnit target, IUnit source)
    {
        ((IManaGrowable)target).GrowMana(m_value, source);
    }
}