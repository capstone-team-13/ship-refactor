using ModiBuff.Core;

public interface IManaGrowable<in TEnergy, out TReturnEnergyInfo> : IUnit
{
    TReturnEnergyInfo GrowMana(TEnergy mana, IUnit source);
}

public interface IManaGrowable : IManaGrowable<float, float>
{
}