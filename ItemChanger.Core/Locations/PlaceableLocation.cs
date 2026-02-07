using ItemChanger.Containers;

namespace ItemChanger.Locations;

/// <summary>
/// Base type for ContainerLocations which support receiving receiving a container at a placement-controlled time and manner
/// in addition to being placed independently and requesting a container at a time of their own choosing.
/// The classic example from Hollow Knight is CoordinateLocation, which is used to place non-item-granting containers
/// at shops (e.g. the tablet at Grubfather).
/// </summary>
public abstract class PlaceableLocation : ContainerLocation
{
    /// <summary>
    /// A managed ContainerLocation receives its container through PlaceContainer, rather than by requesting it in GetContainer.
    /// </summary>
    public required bool Managed { get; init; }

    /// <summary>
    /// Creates and places a container of the given type.
    /// </summary>
    /// <param name="container">The container type to be placed</param>
    /// <param name="info">The container info to be applied to the container during creation</param>
    public abstract void PlaceContainer(Container container, ContainerInfo info);
}
