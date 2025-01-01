using ModiBuff.Core;
using ModiBuff.Core.Units;
using TagType = ModiBuff.Core.Units.TagType;

public static class Buffs
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
    public const string VITAL_SURGE = "Vital Surge";
}

public class MyModifierRecipes : ModifierRecipes
{
    public MyModifierRecipes(ModifierIdManager idManager) : base(idManager)
    {
        CreateGenerators();
    }

    protected override void SetupRecipes()
    {
        Add(Buffs.Mana.NATURAL_GROW, "Mana Natural Grow", "Restores energy passively every 2 second.")
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

            Add(Casts.VITAL_SURGE, "Vital Surge", "Restore 10 health when not silenced. Costs 20 mana.")
                .ApplyCondition(LegalAction.Cast)
                .ApplyCost(CostType.Mana, 20)
                .Effect(new HealEffect(10), EffectOn.Init);
        }
    }
}

public static class ModifierRecipeExtension
{
    public static ModifierRecipe LegalTarget(this ModifierRecipe recipe, LegalTarget target)
    {
        recipe.RemoveTag((ModiBuff.Core.TagType)TagType.LegalTargetAll);
        return recipe.Tag(target.ToTagType());
    }
}