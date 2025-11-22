using System.Collections.Generic;
using System.Linq;
using ItemChanger.Enums;
using ItemChanger.Extensions;
using ItemChanger.Placements;
using Newtonsoft.Json;

namespace ItemChanger.Serialization;

/// <summary>
/// Interface which can supply a bool value. Used frequently for serializable bool tests.
/// </summary>
public interface IBool : IFinderCloneable
{
    /// <summary>
    /// The defined value
    /// </summary>
    bool Value { get; }
}

/// <summary>
/// IBool which supports write operations.
/// </summary>
public interface IWritableBool : IBool
{
    /// <inheritdoc/>
    new bool Value { get; set; }
}

/// <summary>
/// IBool which represents a constant value.
/// </summary>
public class BoxedBool(bool value) : IWritableBool
{
    /// <inheritdoc/>
    public bool Value { get; set; } = value;
}

/// <summary>
/// IBool which represents comparison on a PlayerData int.
/// <br/>Supports IWritableBool in one direction only (direction depends on comparison operator).
/// </summary>
public class IntComparisonBool(
    IInteger ToCompare,
    int Amount,
    ComparisonOperator op = ComparisonOperator.Ge
) : IBool
{
    /// <inheritdoc/>
    [JsonIgnore]
    public bool Value
    {
        get { return ToCompare.Value.Compare(op, Amount); }
    }
}

/// <summary>
/// IBool which searches for a placement by name and checks whether all items on the placement are obtained.
/// <br/>If the placement does not exist, defaults to the value of missingPlacementTest, or true if missingPlacementTest is null.
/// </summary>
public class PlacementAllObtainedBool(string placementName, IBool? missingPlacementTest = null)
    : IBool
{
    /// <summary>
    /// Name of the placement whose items should be monitored.
    /// </summary>
    public string PlacementName => placementName;

    /// <summary>
    /// Optional test that determines the fallback value when the placement cannot be found.
    /// </summary>
    public IBool? MissingPlacementTest => missingPlacementTest;

    /// <inheritdoc/>
    [JsonIgnore]
    public bool Value
    {
        get
        {
            if (
                ItemChangerHost.Singleton.ActiveProfile!.TryGetPlacement(
                    placementName,
                    out Placement? p
                )
                && p != null
            )
            {
                return p.AllObtained();
            }
            return MissingPlacementTest?.Value ?? true;
        }
    }
}

/// <summary>
/// IBool which searches for a placement by name and checks whether its VisitState includes specified flags.
/// <br/>If the placement does not exist, defaults to the value of missingPlacementTest, or true if missingPlacementTest is null.
/// </summary>
public class PlacementVisitStateBool(
    string placementName,
    VisitState requiredFlags,
    IBool? missingPlacementTest
) : IBool
{
    /// <summary>
    /// Name of the placement whose visit state should be inspected.
    /// </summary>
    public string PlacementName => placementName;

    /// <summary>
    /// Flags that must be present on the placement's visit state.
    /// </summary>
    public VisitState RequiredFlags => requiredFlags;

    /// <summary>
    /// If true, requires any flag in requiredFlags to be contained in the VisitState. If false, requires all flags in requiredFlags to be contained in VisitState. Defaults to false.
    /// </summary>
    public bool RequireAny { get; }

    private readonly IBool? missingPlacementTest = missingPlacementTest;

    /// <summary>
    /// An optional test to use if the placement is not found.
    /// </summary>
    public IBool? MissingPlacementTest => this.missingPlacementTest;

    /// <inheritdoc/>
    [JsonIgnore]
    public bool Value
    {
        get
        {
            if (
                ItemChangerHost.Singleton.ActiveProfile!.TryGetPlacement(
                    PlacementName,
                    out Placement? p
                )
                && p != null
            )
            {
                return RequireAny
                    ? p.CheckVisitedAny(RequiredFlags)
                    : p.CheckVisitedAll(RequiredFlags);
            }
            return MissingPlacementTest?.Value ?? true;
        }
    }
}

/// <summary>
/// Composite IBool that returns true when any child evaluates to true.
/// </summary>
public class Disjunction : IBool
{
    [JsonProperty("Bools")]
    private readonly List<IBool> bools = [];

    /// <summary>
    /// Creates an empty disjunction.
    /// </summary>
    public Disjunction() { }

    /// <summary>
    /// Creates a disjunction from the provided bools.
    /// </summary>
    public Disjunction(IEnumerable<IBool> bools)
    {
        this.bools.AddRange(bools);
    }

    /// <summary>
    /// Creates a disjunction from the provided bool params.
    /// </summary>
    public Disjunction(params IBool[] bools)
    {
        this.bools.AddRange(bools);
    }

    /// <inheritdoc/>
    [JsonIgnore]
    public bool Value => bools.Any(b => b.Value);

    /// <summary>
    /// Produces a new disjunction containing the existing bools plus the provided one.
    /// </summary>
    public Disjunction OrWith(IBool b) =>
        b is Disjunction d ? new([.. bools, .. d.bools]) : new([.. bools, b]);
}

/// <summary>
/// Composite IBool that returns true only when every child evaluates to true.
/// </summary>
public class Conjunction : IBool
{
    [JsonProperty("Bools")]
    private readonly List<IBool> bools = [];

    /// <summary>
    /// Creates an empty conjunction.
    /// </summary>
    public Conjunction() { }

    /// <summary>
    /// Creates a conjunction from the provided bools.
    /// </summary>
    public Conjunction(IEnumerable<IBool> bools)
    {
        this.bools.AddRange(bools);
    }

    /// <summary>
    /// Creates a conjunction from the provided bool params.
    /// </summary>
    public Conjunction(params IBool[] bools)
    {
        this.bools.AddRange(bools);
    }

    /// <inheritdoc/>
    [JsonIgnore]
    public bool Value => bools.All(b => b.Value);

    /// <summary>
    /// Produces a new conjunction containing the existing bools plus the provided one.
    /// </summary>
    public Conjunction AndWith(IBool b) =>
        b is Conjunction c ? new([.. bools, .. c.bools]) : new([.. bools, b]);
}

/// <summary>
/// IBool that negates the result of the wrapped bool.
/// </summary>
[method: JsonConstructor]
public class Negation(IBool @bool) : IBool
{
    /// <summary>
    /// Wrapped bool whose result is negated.
    /// </summary>
    public IBool Bool => @bool;

    /// <inheritdoc/>
    [JsonIgnore]
    public bool Value => !Bool.Value;
}
