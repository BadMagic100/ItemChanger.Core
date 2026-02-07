using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    public bool WillBeReplaced() => OriginalContainerType != ChooseBestContainerType();

    /// <summary>
    /// Determines the most appropriate container type for the location based on placement context.
    /// Will always return a non-null container type other than <see cref="ContainerRegistry.UnknownContainerType"/>.
    /// </summary>
    public string ChooseBestContainerType()
    {
        ContainerRegistry reg = ItemChangerHost.Singleton.ContainerRegistry;
        if (ForceDefaultContainer)
        {
            return reg.DefaultSingleItemContainer.Name;
        }

        uint requestedCapabilities = GetNeededCapabilities();
        HashSet<string> unsupported =
        [
            .. (Placement?.GetPlacementAndLocationTags() ?? Tags ?? [])
                .OfType<UnsupportedContainerTag>()
                .Select(t => t.ContainerType),
        ];
        OriginalContainerTag? originalContainerTag = (Tags ?? [])
            .OfType<OriginalContainerTag>()
            .FirstOrDefault();

        if (
            IsOriginalContainerPrioritizedAndValid(
                originalContainerTag,
                requestedCapabilities,
                unsupported,
                reg,
                out string? containerType
            )
        )
        {
            return containerType;
        }

        containerType = GetItemsPreferredContainer(
            originalContainerTag,
            unsupported,
            requestedCapabilities,
            reg
        );

        if (containerType != null)
        {
            // GetItemsPreferredContainer does the modify/instantiate check for us
            return containerType;
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

    /// <summary>
    /// Determines whether the original container should be prioritized above item preferences,
    /// considering whether the container can be instantiated or modified in place.
    /// </summary>
    private bool IsOriginalContainerPrioritizedAndValid(
        OriginalContainerTag? originalContainerTag,
        uint requestedCapabilities,
        HashSet<string> unsupported,
        ContainerRegistry registry,
        [NotNullWhen(true)] out string? containerType
    )
    {
        containerType = null;
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
            && originalContainer.SupportsAll(requestedCapabilities);
        if (supported && SupportsNeededInstantiateOrModify(originalContainerTag, originalContainer))
        {
            containerType = originalContainer.Name;
            return true;
        }

        if (originalContainerTag.Force)
        {
            LoggerProxy.LogWarn(
                $"During container selection for location {Name}, the container "
                    + $"{originalContainer.Name} was forced despite being unsupported by the location or missing "
                    + $"necessary capabilities."
            );
            if (SupportsNeededInstantiateOrModify(originalContainerTag, originalContainer))
            {
                containerType = originalContainer.Name;
                return true;
            }
            else
            {
                LoggerProxy.LogWarn(
                    $"{originalContainer.Name} does not support in-place modification or instantiation."
                );
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the first valid container based on the current placement's items' preferences,
    /// considering whether the container can be either modified or instantiated depending
    /// on the location's original container
    /// </summary>
    private string? GetItemsPreferredContainer(
        OriginalContainerTag? originalContainerTag,
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
                && SupportsNeededInstantiateOrModify(originalContainerTag, ct)
                && ct.SupportsAll(requestedCapabilities) == true
            );
    }

    /// <summary>
    /// Gets the location's preferred container type or the appropriate fallback container,
    /// considering whether containers can be either modified or instantiated depending on the
    /// location's original container.
    /// </summary>
    private string GetLocationPreferredContainerOrFallback(
        OriginalContainerTag? originalContainerTag,
        HashSet<string> unsupported,
        uint requestedCapabilities,
        ContainerRegistry registry
    )
    {
        if (
            originalContainerTag != null
            && !originalContainerTag.LowPriority
            && registry.GetContainer(originalContainerTag.ContainerType) is Container ct
            && SupportsNeededInstantiateOrModify(originalContainerTag, ct)
            && ct.SupportsAll(requestedCapabilities) == true
        )
        {
            return originalContainerTag.ContainerType;
        }

        if (
            Placement?.Items.Skip(1).Any() == true
            && !unsupported.Contains(registry.DefaultMultiItemContainer.Name)
            && SupportsNeededInstantiateOrModify(
                originalContainerTag,
                registry.DefaultMultiItemContainer
            )
            && registry.DefaultMultiItemContainer.SupportsAll(requestedCapabilities)
        )
        {
            return registry.DefaultMultiItemContainer.Name;
        }

        return registry.DefaultSingleItemContainer.Name;
    }

    /// <summary>
    /// Determines whether the container supports in-place modification and/or instantiation, based on the original
    /// container
    /// </summary>
    private static bool SupportsNeededInstantiateOrModify(
        OriginalContainerTag? originalContainerTag,
        Container container
    )
    {
        if (originalContainerTag?.ContainerType == container.Name)
        {
            // theoretically it's possible this is false! not very realistic, but possible.
            return container.SupportsModifyInPlace || container.SupportsInstantiate;
        }
        return container.SupportsInstantiate;
    }

    #endregion
}
