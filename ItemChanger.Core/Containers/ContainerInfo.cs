using System.Collections.Generic;
using System.Linq;
using ItemChanger.Costs;
using ItemChanger.Enums;
using ItemChanger.Items;
using ItemChanger.Placements;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ItemChanger.Containers;

/// <summary>
/// Data for instructing a Container class to make changes. The ContainerGiveInfo field must not be null.
/// </summary>
public class ContainerInfo
{
    /// <summary>
    /// The scene that the container should be spawned in
    /// </summary>
    public required Scene ContainingScene { get; init; }

    /// <summary>
    /// Container type used to fulfill the instructions.
    /// </summary>
    public required string ContainerType { get; init; }

    /// <summary>
    /// The capabilities requested by the related <see cref="Locations.ContainerLocation"/> and its placement.
    /// </summary>
    public uint RequestedCapabilities { get; init; }

    /// <summary>
    /// Details about how items should be dispensed.
    /// </summary>
    public required ContainerGiveInfo GiveInfo { get; init; }

    /// <summary>
    /// Optional cost enforcement configuration.
    /// </summary>
    public ContainerCostInfo? CostInfo { get; init; }

    /// <summary>
    /// Creates a new ContainerInfo instance based on the specified scene, placement, container type, fling type, and
    /// optional cost information.
    /// </summary>
    /// <param name="placement">The placement details specifying where and how the container is positioned. Cannot be null.</param>
    /// <param name="scene">The scene in which the container is placed. Cannot be null.</param>
    /// <param name="containerType">The type of container to create. Cannot be null or empty.</param>
    /// <param name="flingType">The type of fling action associated with the container. Determines how items are given.</param>
    /// <param name="cost">Optional cost information for the container. If null, the container will not have associated cost data.</param>
    /// <returns>A ContainerInfo object initialized with the provided scene, placement, container type, fling type, and optional
    /// cost information.</returns>
    public static ContainerInfo FromPlacement(
        Placement placement,
        Scene scene,
        string containerType,
        FlingType flingType,
        Cost? cost = null
    )
    {
        return FromPlacementAndItems(
            placement,
            placement.Items,
            scene,
            containerType,
            flingType,
            cost
        );
    }

    /// <summary>
    /// Creates a new ContainerInfo instance using the specified placement, items, scene, container type, fling type,
    /// and optional cost information.
    /// </summary>
    /// <param name="placement">The placement information that determines where the container is located.</param>
    /// <param name="items">The collection of items to include in the container. Cannot be null.</param>
    /// <param name="scene">The scene in which the container will be placed. Cannot be null.</param>
    /// <param name="containerType">The type of the container to create. Cannot be null or empty.</param>
    /// <param name="flingType">The fling type to associate with the container's give information.</param>
    /// <param name="cost">An optional cost to associate with the container. If null, the container will not have cost information.</param>
    /// <returns>A new ContainerInfo instance populated with the specified placement, items, scene, container type, fling type,
    /// and optional cost information.</returns>
    public static ContainerInfo FromPlacementAndItems(
        Placement placement,
        IEnumerable<Item> items,
        Scene scene,
        string containerType,
        FlingType flingType,
        Cost? cost = null
    )
    {
        uint neededCapabilities = placement
            .GetPlacementAndLocationTags()
            .OfType<INeedsContainerCapability>()
            .Select(x => x.RequestedCapabilities)
            .Aggregate(0u, (acc, next) => acc | next);

        if (cost != null)
        {
            neededCapabilities |= ContainerCapabilities.PayCosts;
        }

        return new()
        {
            ContainerType = containerType,
            ContainingScene = scene,
            RequestedCapabilities = neededCapabilities,
            GiveInfo = new()
            {
                FlingType = flingType,
                Placement = placement,
                Items = items,
            },
            CostInfo =
                cost == null
                    ? null
                    : new()
                    {
                        Cost = cost,
                        Placement = placement,
                        PreviewItems = items,
                    },
        };
    }

    /// <summary>
    /// Searches for ContainerInfo on a ContainerInfoComponent. Returns null if neither is found.
    /// </summary>
    public static ContainerInfo? FindContainerInfo(GameObject obj)
    {
        ContainerInfoComponent cdc = obj.GetComponent<ContainerInfoComponent>();
        if (cdc != null)
        {
            return cdc.Info;
        }

        return null;
    }
}
