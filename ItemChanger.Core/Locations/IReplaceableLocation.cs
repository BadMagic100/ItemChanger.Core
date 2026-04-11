using ItemChanger.Containers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ItemChanger.Locations;

/// <summary>
/// Interface for a location which can replace its original content with a container.
/// </summary>
public interface IReplaceableLocation
{
    /// <summary>
    /// Replaces the location content with a container.
    /// </summary>
    /// <param name="scene">The scene to replace the content in</param>
    /// <param name="container">The container to place. Must support instantiate.</param>
    /// <param name="info">The container info to be applied.</param>
    /// <returns>The newly created container GameObject</returns>
    /// <seealso cref="Container.SupportsInstantiate"/>
    public GameObject ReplaceWithContainer(Scene scene, Container container, ContainerInfo info);
}
