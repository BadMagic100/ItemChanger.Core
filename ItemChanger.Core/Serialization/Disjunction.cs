using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ItemChanger.Serialization;

/// <summary>
/// Composite bool provider that returns true when any child evaluates to true.
/// </summary>
public class Disjunction : IValueProvider<bool>
{
    [JsonProperty("Bools")]
    private readonly List<IValueProvider<bool>> bools = [];

    /// <summary>
    /// Creates an empty disjunction.
    /// </summary>
    public Disjunction() { }

    /// <summary>
    /// Creates a disjunction from the provided bools.
    /// </summary>
    public Disjunction(IEnumerable<IValueProvider<bool>> bools)
    {
        this.bools.AddRange(bools);
    }

    /// <summary>
    /// Creates a disjunction from the provided bool params.
    /// </summary>
    public Disjunction(params IValueProvider<bool>[] bools)
    {
        this.bools.AddRange(bools);
    }

    /// <inheritdoc/>
    [JsonIgnore]
    public bool Value => bools.Any(b => b.Value);

    /// <summary>
    /// Produces a new disjunction containing the existing bools plus the provided one.
    /// </summary>
    public Disjunction OrWith(IValueProvider<bool> b) =>
        b is Disjunction d ? new([.. bools, .. d.bools]) : new([.. bools, b]);
}
