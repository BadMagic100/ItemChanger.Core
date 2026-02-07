using System.Collections.Generic;
using System.Linq;
using ItemChanger.Containers;
using ItemChanger.Costs;
using ItemChanger.Events.Args;
using ItemChanger.Locations;
using ItemChanger.Logging;
using ItemChanger.Serialization;
using ItemChanger.Tags;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

namespace ItemChanger.Placements;

/// <summary>
/// Placement which handles switching between two possible locations according to a test.
/// </summary>
public class DualPlacement(string Name)
    : Placement(Name),
        IContainerPlacement,
        ISingleCostPlacement,
        IPrimaryLocationPlacement
{
    /// <summary>
    /// Location used when <see cref="Test"/> evaluates to true.
    /// </summary>
    public required Location TrueLocation { get; init; }

    /// <summary>
    /// Location used when <see cref="Test"/> evaluates to false.
    /// </summary>
    public required Location FalseLocation { get; init; }

    /// <summary>
    /// Test determining which location is active.
    /// </summary>
    public required IValueProvider<bool> Test { get; init; }

    private bool cachedValue;

    /// <inheritdoc/>
    public override string MainContainerType
    {
        get
        {
            ContainerLocation? cl = cachedValue
                ? TrueLocation as ContainerLocation
                : FalseLocation as ContainerLocation;
            return cl?.ChooseBestContainerType() ?? ContainerRegistry.UnknownContainerType;
        }
    }

    /// <summary>
    /// Gets the location currently selected by <see cref="Test"/>.
    /// </summary>
    [JsonIgnore]
    public Location Location => cachedValue ? TrueLocation : FalseLocation;

    /// <summary>
    /// Optional cost shared across both locations.
    /// </summary>
    public Cost? Cost { get; set; }

    /// <inheritdoc/>
    protected override void DoLoad()
    {
        cachedValue = Test.Value;
        TrueLocation.Placement = this;
        FalseLocation.Placement = this;
        Location.LoadOnce();
        Cost?.LoadOnce();
        ItemChangerHost.Singleton.GameEvents.BeforeNextSceneLoaded += BeforeNextSceneLoaded;
    }

    /// <inheritdoc/>
    protected override void DoUnload()
    {
        Location.UnloadOnce();
        Cost?.UnloadOnce();
        ItemChangerHost.Singleton.GameEvents.BeforeNextSceneLoaded -= BeforeNextSceneLoaded;
    }

    private void BeforeNextSceneLoaded(BeforeSceneLoadedEventArgs _)
    {
        bool value = Test.Value;
        if (cachedValue != value)
        {
            Location.UnloadOnce();
            cachedValue = value;
            Location.LoadOnce();
        }
    }

    // MutablePlacement implementation of GetContainer
    /// <summary>
    /// The <see cref="MutablePlacement"/> implementation for selecting a container at runtime.
    /// </summary>
    public void GetContainer(
        ContainerLocation location,
        Scene scene,
        out Container container,
        out ContainerInfo info
    )
    {
        string containerType = location.ChooseBestContainerType();
        ContainerRegistry reg = ItemChangerHost.Singleton.ContainerRegistry;

        Container? candidateContainer = reg.GetContainer(containerType);
        if (candidateContainer is null)
        {
            LoggerProxy.LogWarn(
                $"For placement {Name}, the location {location.Name} returned an invalid container type. Falling back to default single-item container."
            );
            candidateContainer = reg.DefaultSingleItemContainer;
        }

        container = candidateContainer;
        info = ContainerInfo.FromPlacement(
            this,
            scene,
            candidateContainer.Name,
            location.FlingType,
            Cost
        );
    }

    /// <inheritdoc/>
    public override IEnumerable<Tag> GetPlacementAndLocationTags()
    {
        return base.GetPlacementAndLocationTags()
            .Concat(FalseLocation.Tags ?? Enumerable.Empty<Tag>())
            .Concat(TrueLocation.Tags ?? Enumerable.Empty<Tag>());
    }
}
