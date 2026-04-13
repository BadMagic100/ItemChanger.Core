using System.Collections.Generic;
using System.Linq;
using ItemChanger.Costs;
using ItemChanger.Locations;
using ItemChanger.Tags;

namespace ItemChanger.Placements;

/// <summary>
/// Placement for self-implementing locations (anything that drives its own logic without being placed in a container), typically used when a location awards items via in-scene scripts rather than spawned objects.
/// </summary>
public class AutoPlacement(string Name)
    : Placement(Name),
        IPrimaryLocationPlacement,
        ISingleCostPlacement
{
    /// <summary>
    /// Location responsible for delivering the placement logic.
    /// </summary>
    public required AutoLocation Location { get; init; }

    Location IPrimaryLocationPlacement.Location => Location;

    /// <summary>
    /// Optional cost that the <see cref="AutoLocation"/> may enforce.
    /// </summary>
    public Cost? Cost { get; set; }

    /// <summary>
    /// Indicates whether the underlying <see cref="AutoLocation"/> can enforce costs.
    /// </summary>
    public virtual bool SupportsCost => Location.SupportsCost;

    /// <inheritdoc/>
    protected override void DoLoad()
    {
        Location.Placement = this;
        Location.LoadOnce();
        Cost?.LoadOnce();
    }

    /// <inheritdoc/>
    protected override void DoUnload()
    {
        Location.UnloadOnce();
        Cost?.UnloadOnce();
    }

    /// <inheritdoc/>
    public override IEnumerable<Tag> GetPlacementAndLocationTags()
    {
        return base.GetPlacementAndLocationTags().Concat(Location.Tags ?? Enumerable.Empty<Tag>());
    }
}
