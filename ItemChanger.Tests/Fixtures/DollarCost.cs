using ItemChanger.Costs;
using ItemChanger.Serialization;

namespace ItemChanger.Tests.Fixtures;

internal class DollarCost : ConsumableIntCost
{
    public IWritableValueProvider<int> Source { get; set; } = new BoxedInteger(50);

    public override string GetCostText() => $"Pay ${GetValueSource().Value}";

    protected override IWritableValueProvider<int> GetValueSource() => Source;
}
