using ItemChanger.Placements;
using Newtonsoft.Json;

namespace ItemChanger.Serialization;

/// <summary>
/// A bool provider which searches for a placement by name and checks whether all items on the placement are obtained.
/// <br/>If the placement does not exist, defaults to the value of missingPlacementTest, or true if missingPlacementTest is null.
/// </summary>
public class PlacementAllObtainedBool(
    string placementName,
    IValueProvider<bool>? missingPlacementTest = null
) : IValueProvider<bool>
{
    /// <summary>
    /// Name of the placement whose items should be monitored.
    /// </summary>
    public string PlacementName => placementName;

    /// <summary>
    /// Optional test that determines the fallback value when the placement cannot be found.
    /// </summary>
    public IValueProvider<bool>? MissingPlacementTest => missingPlacementTest;

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
