using ItemChanger.Tags.Constraints;

namespace ItemChanger.Tags;

/// <summary>
/// Indicates the original container type for a location, which will be used
/// if possible if no other preference is present. Also includes various ways that the original
/// container may take precedence over an item's preferred container.
/// </summary>
[LocationTag]
public class OriginalContainerTag : Tag
{
    /// <summary>
    /// The original container type
    /// </summary>
    public required string ContainerType { get; init; }

    /// <summary>
    /// Indicates whether the original container should take precedence over item-specified preferences
    /// during container selection. If the container doesn't have the required capabilities, it will be
    /// slected anyway with a warning.
    /// </summary>
    /// <remarks>
    /// If both Force and <see cref="Priority"/> are <see langword="true"/>, the Force behavior will be used.
    /// </remarks>
    public bool Force { get; init; }

    /// <summary>
    /// Indicates whether the original container should take precedence over item-specified preferences
    /// during container selection. If the container doesn't have the required capabilities, it will be
    /// discarded as a candidate.
    /// </summary>
    /// <remarks>
    /// If both Priority and <see cref="Force"/> are <see langword="true"/>, the <see cref="Force"/> behavior will be used.
    /// </remarks>
    public bool Priority { get; init; }

    /// <summary>
    /// Indicates whether the original container should be discarded as a fallback when item-specified
    /// preferences do not yield a valid container during container selection.
    /// </summary>
    /// <remarks>
    /// If <see cref="Force"/> or <see cref="Priority"/> is <see langword="true"/>, LowPriority is ignored.
    /// </remarks>
    public bool LowPriority { get; init; }
}
