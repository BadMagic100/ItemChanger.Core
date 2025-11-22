using ItemChanger.Locations;
using ItemChanger.Logging;
using ItemChanger.Placements;

namespace ItemChanger.Tags;

/// <summary>
/// Tag which generates a warning message on load if an incompatible placement exists.
/// Generally it is expected that incompatible placements are mutually incompatible; that is,
/// if A is incompatible with B, then B is also incompatible with A. If this is not satisfied, there
/// can be edge cases where no warning message is generated.
/// </summary>
public class IncompatibilityWarningTag : Tag
{
    /// <summary>
    /// Placement name that should not coexist with the tagged placement.
    /// </summary>
    public required string IncompatiblePlacementName { get; init; }

    /// <inheritdoc/>
    protected override void DoLoad(TaggableObject parent)
    {
        string? parentPlacementName = parent switch
        {
            Placement parentPlacement => parentPlacement.Name,
            Location parentLocation => parentLocation.Placement!.Name,
            _ => null,
        };

        if (
            ItemChangerHost.Singleton.ActiveProfile!.TryGetPlacement(
                IncompatiblePlacementName,
                out _
            )
        )
        {
            LoggerProxy.LogWarn(
                $"Placement {parentPlacementName} is incompatible with {IncompatiblePlacementName}, but both are present."
            );
        }
    }
}
