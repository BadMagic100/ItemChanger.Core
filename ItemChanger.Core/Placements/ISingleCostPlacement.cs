using ItemChanger.Costs;

namespace ItemChanger.Placements;

/// <summary>
/// Interface which indicates that placement expects all items to share a common cost.
/// </summary>
public interface ISingleCostPlacement
{
    /// <summary>
    /// Gets or sets the cost shared across the placement's items.
    /// </summary>
    /// <remarks>
    /// Locations that wrap to an implementation of ISingleCostPlacement are expected to
    /// set the default cost using <see cref="ItemChanger.Tags.DefaultCostTag.GetDefaultCost(Locations.Location)"/>
    /// </remarks>
    Cost? Cost { get; set; }
}
