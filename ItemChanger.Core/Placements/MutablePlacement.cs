using System;
using System.Collections.Generic;
using System.Linq;
using ItemChanger.Containers;
using ItemChanger.Costs;
using ItemChanger.Items;
using ItemChanger.Locations;
using ItemChanger.Logging;
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
        Location location,
        Scene scene,
        out Container container,
        out ContainerInfo info
    )
    {
        string containerType;
        if (this.ContainerType == ContainerRegistry.UnknownContainerType)
        {
            this.ContainerType = ChooseContainerType(this, location as ContainerLocation, Items);
        }

        containerType = this.ContainerType;
        Container? candidateContainer = ItemChangerHost.Singleton.ContainerRegistry.GetContainer(
            containerType
        );
        if (candidateContainer == null || !candidateContainer.SupportsInstantiate)
        {
            this.ContainerType = containerType = ChooseContainerType(
                this,
                location as ContainerLocation,
                Items
            );
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
    /// Determines the most appropriate container type for the placement and location context.
    /// </summary>
    public static string ChooseContainerType<T>(
        T placement,
        ContainerLocation? location,
        IEnumerable<Item> items
    )
        where T : Placement, ISingleCostPlacement
    {
        ContainerRegistry reg = ItemChangerHost.Singleton.ContainerRegistry;
        if (ShouldUseDefaultContainer(location))
        {
            return reg.DefaultSingleItemContainer.Name;
        }

        ContainerLocation targetLocation = location!;
        uint requestedCapabilities = DetermineRequestedCapabilities(placement);
        HashSet<string> unsupported = BuildUnsupportedSet(placement);
        OriginalContainerTag? originalContainerTag = placement
            .GetPlacementAndLocationTags()
            .OfType<OriginalContainerTag>()
            .FirstOrDefault();

        if (
            TryUsePrioritizedOriginalContainer(
                placement,
                targetLocation,
                originalContainerTag,
                requestedCapabilities,
                unsupported,
                reg,
                out string prioritizedContainer
            )
        )
        {
            return prioritizedContainer;
        }

        string? containerType = FindPreferredContainer(
            targetLocation,
            items,
            unsupported,
            requestedCapabilities,
            reg
        );

        if (!string.IsNullOrEmpty(containerType))
        {
            return containerType!;
        }

        return DetermineFallbackContainer(
            originalContainerTag,
            items,
            unsupported,
            requestedCapabilities,
            reg
        );
    }

    private static bool ShouldUseDefaultContainer(ContainerLocation? location) =>
        location?.ForceDefaultContainer ?? true;

    private static uint DetermineRequestedCapabilities<T>(T placement)
        where T : Placement, ISingleCostPlacement
    {
        uint requestedCapabilities = placement
            .GetPlacementAndLocationTags()
            .OfType<INeedsContainerCapability>()
            .Select(x => x.RequestedCapabilities)
            .Aggregate(0u, (acc, next) => acc | next);

        if (placement.Cost != null)
        {
            requestedCapabilities |= ContainerCapabilities.PayCosts;
        }

        return requestedCapabilities;
    }

    private static HashSet<string> BuildUnsupportedSet<T>(T placement)
        where T : Placement
    {
        return
        [
            .. placement
                .GetPlacementAndLocationTags()
                .OfType<UnsupportedContainerTag>()
                .Select(t => t.ContainerType),
        ];
    }

    private static bool TryUsePrioritizedOriginalContainer<T>(
        T placement,
        ContainerLocation location,
        OriginalContainerTag? originalContainerTag,
        uint requestedCapabilities,
        HashSet<string> unsupported,
        ContainerRegistry registry,
        out string containerType
    )
        where T : Placement
    {
        containerType = ContainerRegistry.UnknownContainerType;
        if (
            originalContainerTag == null
            || !(originalContainerTag.Force || originalContainerTag.Priority)
        )
        {
            return false;
        }

        Container? originalContainer = registry.GetContainer(originalContainerTag.ContainerType);
        if (originalContainer == null)
        {
            return false;
        }

        bool supported =
            location.Supports(originalContainerTag.ContainerType)
            && !unsupported.Contains(originalContainerTag.ContainerType)
            && originalContainer.SupportsAll(false, requestedCapabilities);
        if (supported)
        {
            containerType = originalContainerTag.ContainerType;
            return true;
        }

        if (originalContainerTag.Force)
        {
            LoggerProxy.LogWarn(
                $"During container selection for {placement.Name}, the container "
                    + $"{originalContainer.Name} was forced despite being unsupported by the location or missing "
                    + $"necessary capabilities."
            );
            containerType = originalContainerTag.ContainerType;
            return true;
        }

        return false;
    }

    private static string? FindPreferredContainer(
        ContainerLocation location,
        IEnumerable<Item> items,
        HashSet<string> unsupported,
        uint requestedCapabilities,
        ContainerRegistry registry
    )
    {
        return items
            .Select(i => i.GetPreferredContainer())
            .FirstOrDefault(c =>
                location.Supports(c)
                && !unsupported.Contains(c)
                && registry.GetContainer(c)?.SupportsAll(true, requestedCapabilities) == true
            );
    }

    private static string DetermineFallbackContainer(
        OriginalContainerTag? originalContainerTag,
        IEnumerable<Item> items,
        HashSet<string> unsupported,
        uint requestedCapabilities,
        ContainerRegistry registry
    )
    {
        if (
            originalContainerTag != null
            && registry
                .GetContainer(originalContainerTag.ContainerType)
                ?.SupportsAll(true, requestedCapabilities) == true
        )
        {
            return originalContainerTag.ContainerType;
        }

        if (
            items.Skip(1).Any()
            && !unsupported.Contains(registry.DefaultMultiItemContainer.Name)
            && registry.DefaultMultiItemContainer.SupportsAll(true, requestedCapabilities)
        )
        {
            return registry.DefaultMultiItemContainer.Name;
        }

        return registry.DefaultSingleItemContainer.Name;
    }

    /// <summary>
    /// Combines placement tags with tags exposed by the location.
    /// </summary>
    public override IEnumerable<Tag> GetPlacementAndLocationTags()
    {
        return base.GetPlacementAndLocationTags().Concat(Location.Tags ?? Enumerable.Empty<Tag>());
    }
}
