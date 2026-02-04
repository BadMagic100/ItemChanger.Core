using System;
using System.Collections.Generic;
using System.Linq;
using ItemChanger.Containers;
using ItemChanger.Logging;
using ItemChanger.Placements;
using ItemChanger.Tags;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

namespace ItemChanger.Locations;

/// <summary>
/// Location type which supports placing multiple kinds of objects.
/// </summary>
public abstract class ContainerLocation : Location
{
    /// <summary>
    /// Whether to force a default single-item container at the location.
    /// </summary>
    public bool ForceDefaultContainer { get; init; }

    /// <summary>
    /// The original container type associated with this
    /// </summary>
    [JsonIgnore]
    public string? OriginalContainerType => GetTag<OriginalContainerTag>()?.ContainerType;

    /// <summary>
    /// Retrieves the container implementation that will be used for this location.
    /// </summary>
    public void GetContainer(Scene scene, out Container container, out ContainerInfo info)
    {
        if (Placement is not IContainerPlacement cp)
        {
            throw new InvalidOperationException(
                $"Cannot get container for {nameof(ContainerLocation)} {Name} because the placement {Placement?.Name} is not an {nameof(IContainerPlacement)}"
            );
        }
        cp.GetContainer(this, scene, out container, out info);
    }

    /// <summary>
    /// Determines whether the given container type can be used at this location.
    /// </summary>
    public virtual bool Supports(string containerType)
    {
        return containerType
                == ItemChangerHost.Singleton.ContainerRegistry.DefaultSingleItemContainer.Name
            || !ForceDefaultContainer;
    }

    /// <inheritdoc/>
    public override Placement Wrap()
    {
        return new MutablePlacement(Name)
        {
            Location = this,
            Cost = ImplicitCostTag.GetDefaultCost(this),
        };
    }

    #region Selection

    /// <summary>
    /// Determines whether the object is expected to be replaced with a new container.
    /// </summary>
    public bool WillBeReplaced() => GetOriginalContainerType() != ChooseContainerType();

    /// <summary>
    /// Gets the original container type for this location, or null if one was not specified
    /// </summary>
    public string? GetOriginalContainerType()
    {
        IEnumerable<Tag> relevantTags = Placement?.GetPlacementAndLocationTags() ?? Tags ?? [];
        OriginalContainerTag? originalContainerTag = relevantTags
            .OfType<OriginalContainerTag>()
            .FirstOrDefault();
        return originalContainerTag?.ContainerType;
    }

    /// <summary>
    /// Determines the most appropriate container type for the location based on placement context.
    /// Will always return a non-null container type other than <see cref="ContainerRegistry.UnknownContainerType"/>.
    /// </summary>
    public string ChooseContainerType()
    {
        ContainerRegistry reg = ItemChangerHost.Singleton.ContainerRegistry;
        if (ForceDefaultContainer)
        {
            return reg.DefaultSingleItemContainer.Name;
        }

        uint requestedCapabilities = GetNeededCapabilities();
        IEnumerable<Tag> relevantTags = Placement?.GetPlacementAndLocationTags() ?? Tags ?? [];
        HashSet<string> unsupported =
        [
            .. relevantTags.OfType<UnsupportedContainerTag>().Select(t => t.ContainerType),
        ];
        OriginalContainerTag? originalContainerTag = relevantTags
            .OfType<OriginalContainerTag>()
            .FirstOrDefault();

        if (
            IsOriginalContainerPrioritized(
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

        string? containerType = GetItemsPreferredContainer(unsupported, requestedCapabilities, reg);

        if (!string.IsNullOrEmpty(containerType))
        {
            return containerType!;
        }

        return GetLocationPreferredContainerOrFallback(
            originalContainerTag,
            unsupported,
            requestedCapabilities,
            reg
        );
    }

    /// <summary>
    /// Gets a bitfield of capabilities that need to be supported by containers at this location
    /// </summary>
    public uint GetNeededCapabilities()
    {
        uint neededCapabilities = (Placement?.GetPlacementAndLocationTags() ?? Tags ?? [])
            .OfType<INeedsContainerCapability>()
            .Select(x => x.RequestedCapabilities)
            .Aggregate(0u, (acc, next) => acc | next);

        if (Placement is ISingleCostPlacement { Cost: not null })
        {
            neededCapabilities |= ContainerCapabilities.PayCosts;
        }

        return neededCapabilities;
    }

    private bool IsOriginalContainerPrioritized(
        OriginalContainerTag? originalContainerTag,
        uint requestedCapabilities,
        HashSet<string> unsupported,
        ContainerRegistry registry,
        out string containerType
    )
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
            Supports(originalContainerTag.ContainerType)
            && !unsupported.Contains(originalContainerTag.ContainerType)
            && originalContainer.SupportsModifyInPlace
            && originalContainer.SupportsAll(requestedCapabilities);
        if (supported)
        {
            containerType = originalContainerTag.ContainerType;
            return true;
        }

        if (originalContainerTag.Force)
        {
            LoggerProxy.LogWarn(
                $"During container selection for location {Name}, the container "
                    + $"{originalContainer.Name} was forced despite being unsupported by the location or missing "
                    + $"necessary capabilities."
            );
            containerType = originalContainerTag.ContainerType;
            return true;
        }

        return false;
    }

    private string? GetItemsPreferredContainer(
        HashSet<string> unsupported,
        uint requestedCapabilities,
        ContainerRegistry registry
    )
    {
        return Placement
            ?.Items.Select(i => i.GetPreferredContainer())
            .FirstOrDefault(c =>
                Supports(c)
                && !unsupported.Contains(c)
                && registry.GetContainer(c) is Container ct
                && ct.SupportsInstantiate
                && ct.SupportsAll(requestedCapabilities) == true
            );
    }

    private string GetLocationPreferredContainerOrFallback(
        OriginalContainerTag? originalContainerTag,
        HashSet<string> unsupported,
        uint requestedCapabilities,
        ContainerRegistry registry
    )
    {
        if (
            originalContainerTag != null
            && registry.GetContainer(originalContainerTag.ContainerType) is Container ct
            && ct.SupportsModifyInPlace
            && ct.SupportsAll(requestedCapabilities) == true
        )
        {
            return originalContainerTag.ContainerType;
        }

        if (
            Placement?.Items.Skip(1).Any() == true
            && !unsupported.Contains(registry.DefaultMultiItemContainer.Name)
            && registry.DefaultMultiItemContainer.SupportsInstantiate
            && registry.DefaultMultiItemContainer.SupportsAll(requestedCapabilities)
        )
        {
            return registry.DefaultMultiItemContainer.Name;
        }

        return registry.DefaultSingleItemContainer.Name;
    }

    #endregion
}
