using ItemChanger.Containers;
using ItemChanger.Tags.Constraints;

namespace ItemChanger.Tags;

/// <summary>
/// Requests capabilities for a container at the attached location or placement
/// </summary>
[LocationTag]
[PlacementTag]
public class RequiredCapabilitiesTag : Tag, INeedsContainerCapability
{
    /// <inheritdoc/>
    public uint RequestedCapabilities { get; set; }
}
