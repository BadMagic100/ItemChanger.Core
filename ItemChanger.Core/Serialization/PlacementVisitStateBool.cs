using ItemChanger.Enums;
using ItemChanger.Placements;
using Newtonsoft.Json;

namespace ItemChanger.Serialization;

/// <summary>
/// A bool provider which searches for a placement by name and checks whether its VisitState includes specified flags.
/// <br/>If the placement does not exist, defaults to the value of missingPlacementTest, or true if missingPlacementTest is null.
/// </summary>
public class PlacementVisitStateBool(
    string placementName,
    VisitState requiredFlags,
    IValueProvider<bool>? missingPlacementTest
) : IValueProvider<bool>
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

    private readonly IValueProvider<bool>? missingPlacementTest = missingPlacementTest;

    /// <summary>
    /// An optional test to use if the placement is not found.
    /// </summary>
    public IValueProvider<bool>? MissingPlacementTest => this.missingPlacementTest;

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
