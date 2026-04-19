using ItemChanger.Enums;
using ItemChanger.Events.Args;
using ItemChanger.Items;
using ItemChanger.Locations;
using ItemChanger.Placements;
using ItemChanger.Serialization;

namespace ItemChanger.Tags;

/// <summary>
/// Tag which sets a writable bool provider when its parent item is given.
/// If attached to a location or placement, sets the bool when <see cref="VisitState.ObtainedAnyItem"/>
/// is first set on the placement.
/// </summary>
public class SetBoolOnGiveTag : Tag
{
    /// <summary>
    /// Bool updated when the tag triggers.
    /// </summary>
    public required IWritableValueProvider<bool> Bool { get; init; }

    /// <summary>
    /// Value assigned to <see cref="Bool"/> upon trigger.
    /// </summary>
    public required bool Value { get; init; }

    /// <inheritdoc/>
    protected override void DoLoad(TaggableObject parent)
    {
        if (parent is Item item)
        {
            item.OnGive += OnGive;
        }
        else
        {
            Placement? placement = parent as Placement ?? (parent as Location)?.Placement;
            if (placement is not null)
            {
                placement.OnVisited += OnVisited;
            }
        }
    }

    /// <inheritdoc/>
    protected override void DoUnload(TaggableObject parent)
    {
        if (parent is Item item)
        {
            item.OnGive -= OnGive;
        }
        else
        {
            Placement? placement = parent as Placement ?? (parent as Location)?.Placement;
            if (placement is not null)
            {
                placement.OnVisited -= OnVisited;
            }
        }
    }

    private void OnGive(ReadOnlyGiveEventArgs obj)
    {
        Bool.Value = Value;
    }

    private void OnVisited(PlacementVisitedEventArgs obj)
    {
        if (obj.HasChangedFlags(VisitState.ObtainedAnyItem))
        {
            Bool.Value = Value;
        }
    }
}
