using System;
using System.Collections.Generic;
using ItemChanger.Placements;
using ItemChanger.Serialization;

namespace ItemChanger.Locations;

/// <summary>
/// Helper location representing a high-cardinality choice of locations based on a selector.
/// </summary>
/// <seealso cref="DualLocation"/>
public class MultiLocation<T> : Location
    where T : notnull, Enum
{
    /// <summary>
    /// Selector determining which location is currently active
    /// </summary>
    public required IValueProvider<T> Selector { get; init; }

    /// <summary>
    /// An exhaustive lookup of locations for all possible values of <see cref="Selector"/>
    /// </summary>
    public required IReadOnlyDictionary<T, Location> Locations { get; init; }

    /// <inheritdoc/>
    protected override void DoLoad()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    protected override void DoUnload()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override Placement Wrap()
    {
        return new MultiPlacement<T>(Name) { Selector = Selector, Locations = Locations };
    }
}
