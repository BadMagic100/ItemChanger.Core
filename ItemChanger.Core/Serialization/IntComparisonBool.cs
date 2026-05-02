using ItemChanger.Enums;
using ItemChanger.Extensions;
using Newtonsoft.Json;

namespace ItemChanger.Serialization;

/// <summary>
/// A bool provider which represents comparison on an integer provider against a threshold.
/// </summary>
public class IntComparisonBool : IValueProvider<bool>
{
    /// <summary>
    /// The integer to compare against
    /// </summary>
    public required IValueProvider<int> ToCompare { get; init; }

    /// <summary>
    /// The threshold amount
    /// </summary>
    public required int Amount { get; init; }

    /// <summary>
    /// The comparison operator to use
    /// </summary>
    public ComparisonOperator Operator { get; init; } = ComparisonOperator.Ge;

    /// <inheritdoc/>
    [JsonIgnore]
    public bool Value
    {
        get { return ToCompare.Value.Compare(Operator, Amount); }
    }
}
