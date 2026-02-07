using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ItemChanger.Serialization;

/// <summary>
/// Composite bool provider that returns true only when every child evaluates to true.
/// </summary>
public class Conjunction : IValueProvider<bool>
{
    [JsonProperty("Bools")]
    private readonly List<IValueProvider<bool>> bools = [];

    /// <summary>
    /// Creates an empty conjunction.
    /// </summary>
    public Conjunction() { }

    /// <summary>
    /// Creates a conjunction from the provided bools.
    /// </summary>
    public Conjunction(IEnumerable<IValueProvider<bool>> bools)
    {
        this.bools.AddRange(bools);
    }

    /// <summary>
    /// Creates a conjunction from the provided bool params.
    /// </summary>
    public Conjunction(params IValueProvider<bool>[] bools)
    {
        this.bools.AddRange(bools);
    }

    /// <inheritdoc/>
    [JsonIgnore]
    public bool Value => bools.All(b => b.Value);

    /// <summary>
    /// Produces a new conjunction containing the existing bools plus the provided one.
    /// </summary>
    public Conjunction AndWith(IValueProvider<bool> b) =>
        b is Conjunction c ? new([.. bools, .. c.bools]) : new([.. bools, b]);
}
