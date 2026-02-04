using System;
using System.Collections.Generic;
using System.Linq;
using ItemChanger.Containers;
using ItemChanger.Costs;
using ItemChanger.Locations;
using ItemChanger.Tags;
using UnityEngine.SceneManagement;

namespace ItemChanger.Placements;

/// <summary>
/// The default placement for most use cases.
/// Chooses an item container for its location based on its item list.
/// </summary>
public class MutablePlacement(string Name)
    : Placement(Name),
        IContainerPlacement,
        ISingleCostPlacement,
        IPrimaryLocationPlacement
{
    /// <summary>
    /// Location that provides the placement's interaction surface.
    /// </summary>
    public required ContainerLocation Location { get; init; }
    Location IPrimaryLocationPlacement.Location => Location;

    /// <inheritdoc/>
    public override string MainContainerType => ContainerType;

    /// <summary>
    /// Gets the currently selected container type.
    /// </summary>
    public string ContainerType { get; private set; } = ContainerRegistry.UnknownContainerType;

    /// <summary>
    /// Optional cost paid before retrieving items from this placement.
    /// </summary>
    public Cost? Cost { get; set; }

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

    /// <summary>
    /// Resolves a container suited for the placement at runtime.
    /// </summary>
    public void GetContainer(
        ContainerLocation location,
        Scene scene,
        out Container container,
        out ContainerInfo info
    )
    {
        string containerType;
        if (this.ContainerType == ContainerRegistry.UnknownContainerType)
        {
            this.ContainerType = location.ChooseContainerType();
        }

        containerType = this.ContainerType;
        Container? candidateContainer = ItemChangerHost.Singleton.ContainerRegistry.GetContainer(
            containerType
        );
        if (candidateContainer == null || !candidateContainer.SupportsInstantiate)
        {
            this.ContainerType = containerType = location.ChooseContainerType();
            candidateContainer = ItemChangerHost.Singleton.ContainerRegistry.GetContainer(
                containerType
            );
            if (candidateContainer == null)
            {
                throw new InvalidOperationException(
                    $"Unable to resolve container type {containerType} for placement {Name}!"
                );
            }
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

    /// <summary>
    /// Combines placement tags with tags exposed by the location.
    /// </summary>
    public override IEnumerable<Tag> GetPlacementAndLocationTags()
    {
        return base.GetPlacementAndLocationTags().Concat(Location.Tags ?? Enumerable.Empty<Tag>());
    }
}
