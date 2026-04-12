using System;
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
/// Placement which handles switching between several possible locations according to a selector.
/// </summary>
public class MultiPlacement<T>(string Name)
    : Placement(Name),
        IContainerPlacement,
        ISingleCostPlacement,
        IPrimaryLocationPlacement
    where T : notnull, Enum
{
    private T? cachedValue;

    /// <summary>
    /// Selector determining which location is currently active
    /// </summary>
    public required IValueProvider<T> Selector { get; init; }

    /// <summary>
    /// An exhaustive lookup of locations for all possible values of <see cref="Selector"/>
    /// </summary>
    public required IReadOnlyDictionary<T, Location> Locations { get; init; }

    /// <summary>
    /// The currently selected location. Attempting to read this before load will produce undefined behavior.
    /// </summary>
    /// <see cref="Selector"/>
    [JsonIgnore]
    public Location Location
    {
        get
        {
            cachedValue ??= Selector.Value;
            return Locations[cachedValue];
        }
    }

    /// <inheritdoc/>
    public override string MainContainerType
    {
        get
        {
            ContainerLocation? cl = Location as ContainerLocation;
            return cl?.ChooseBestContainerType() ?? ContainerRegistry.UnknownContainerType;
        }
    }

    /// <summary>
    /// Optional cost shared across all locations.
    /// </summary>
    public Cost? Cost { get; set; }

    /// <inheritdoc/>
    protected override void DoLoad()
    {
        if (!Enum.GetValues(typeof(T)).OfType<T>().All(Locations.ContainsKey))
        {
            throw new InvalidOperationException(
                $"{nameof(Locations)} is not exhaustive over the values of {typeof(T)}"
            );
        }
        cachedValue = Selector.Value;
        foreach (Location location in Locations.Values)
        {
            location.Placement = this;
        }
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
        RefreshLocation();
    }

    /// <summary>
    /// Trigger a re-check of Selector and loads the appropriate location if needed.
    /// Triggered by default on scene change, but also available for use by subclasses
    /// to be triggered by other events
    /// </summary>
    protected void RefreshLocation()
    {
        T value = Selector.Value;
        if (!value.Equals(cachedValue))
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
            .Concat(Locations.Values.SelectMany(l => l.Tags ?? Enumerable.Empty<Tag>()));
    }
}
