using ModiBuff.Core;

public interface IManaGrowable<in TEnergy, out TReturnEnergyInfo> : IUnit
{
    TReturnEnergyInfo GrowMana(TEnergy energy, IUnit source);
}

public interface IManaGrowable : IManaGrowable<float, float>
{
}