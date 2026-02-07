using ItemChanger.Enums;
using ItemChanger.Extensions;
using Newtonsoft.Json;

namespace ItemChanger.Serialization;

/// <summary>
/// A bool provider which represents comparison on an integer provider.
/// </summary>
public class IntComparisonBool(
    IValueProvider<int> ToCompare,
    int Amount,
    ComparisonOperator op = ComparisonOperator.Ge
) : IValueProvider<bool>
{
    /// <inheritdoc/>
    [JsonIgnore]
    public bool Value
    {
        get { return ToCompare.Value.Compare(op, Amount); }
    }
}
