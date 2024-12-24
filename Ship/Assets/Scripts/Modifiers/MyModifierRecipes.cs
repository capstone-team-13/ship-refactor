using ModiBuff.Core;
using ModiBuff.Core.Units;

public static class Modifiers
{
    public static class Mana
    {
        public const string NATURAL_GROW = "Mana Grow";
    }
}

// Player
public static class Casts
{
    public const string MELEE = "Melee";
    public const string STUN = "Stun";
    public const string HEAL = "Heal";
}

public class MyModifierRecipes : ModifierRecipes
{
    public MyModifierRecipes(ModifierIdManager idManager) : base(idManager)
    {
        CreateGenerators();
    }

    protected override void SetupRecipes()
    {
        Add(Modifiers.Mana.NATURAL_GROW, "Mana Natural Grow", "Restores energy passively every 2 second.")
            .Interval(2).Effect(new ManaGrowEffect(1), EffectOn.Interval);

        // Player Actions
        {
            Add(Casts.MELEE, "Melee", "Melee")
                .ApplyCondition(LegalAction.Act)
                .Effect(new DamageEffect(2), EffectOn.Init);

            Add(Casts.STUN, "Stun", "Damage and stun 2s")
                .ApplyCost(CostType.Mana, 10)
                .ApplyCondition(LegalAction.Act)
                .Effect(new DamageEffect(4), EffectOn.Init)
                .Effect(new StatusEffectEffect(StatusEffectType.Stun, 2), EffectOn.Init).Remove(2).Refresh();

            Add(Casts.HEAL, "Range Heal", "Heal player in range")
                .ApplyCondition(LegalAction.Act)
                .Effect(new HealEffect(2), EffectOn.Init);
        }
    }
}