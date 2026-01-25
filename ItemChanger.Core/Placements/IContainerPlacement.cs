using ItemChanger.Containers;
using ItemChanger.Locations;
using UnityEngine.SceneManagement;

namespace ItemChanger.Placements;

/// <summary>
/// Interace for placements which can be used by ContainerLocation. In other words, on demand the placement returns an object which is capable of giving its items.
/// </summary>
public interface IContainerPlacement
{
    /// <summary>
    /// Provides a container and associated metadata that can dispense the placement's items in the given location.
    /// </summary>
    /// <param name="location">Location requesting the container.</param>
    /// <param name="scene">The scene where the container is to be spawned</param>
    /// <param name="container">Container capable of dispensing the items.</param>
    /// <param name="info">Additional container metadata.</param>
    public void GetContainer(
        Location location,
        Scene scene,
        out Container container,
        out ContainerInfo info
    );
}
