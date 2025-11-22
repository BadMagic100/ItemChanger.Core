using System.Collections.Generic;
using System.Linq;
using ItemChanger.Costs;
using ItemChanger.Locations;
using ItemChanger.Tags.Constraints;

namespace ItemChanger.Tags;

/// <summary>
/// A tag which provides a default cost for a location.
/// </summary>
[LocationTag]
public class ImplicitCostTag : Tag
{
    /// <summary>
    /// Cost applied when this tag is present.
    /// </summary>
    public required Cost Cost { get; init; }

    /// <summary>
    /// An inherent cost always applies. A non-inherent cost applies as a substitute when the placement does not have a (non-null) cost.
    /// </summary>
    public bool Inherent { get; init; }

    /// <inheritdoc/>
    protected override void DoLoad(TaggableObject parent)
    {
        Cost.LoadOnce();
    }

    /// <inheritdoc/>
    protected override void DoUnload(TaggableObject parent)
    {
        Cost.UnloadOnce();
    }

    /// <summary>
    /// Helper function to get the default cost the should be applied to a location when wrapping it into a single-cost placement.
    /// </summary>
    /// <param name="loc">The location to inspect</param>
    public static Cost? GetDefaultCost(Location loc)
    {
        List<Cost> costs = [.. loc.GetTags<ImplicitCostTag>().Select(c => c.Cost.DeepClone()!)];
        return costs.Count switch
        {
            0 => null,
            1 => costs[0],
            _ => new MultiCost(costs),
        };
    }
}
