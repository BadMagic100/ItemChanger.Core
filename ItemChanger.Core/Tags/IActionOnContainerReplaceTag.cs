using UnityEngine;
using UnityEngine.SceneManagement;

namespace ItemChanger.Tags;

/// <summary>
/// Interface for tags that take an additional action when an <see cref="Locations.IReplaceableLocation"/> is replaced
/// </summary>
public interface IActionOnContainerReplaceTag
{
    /// <summary>
    /// Action to take when the container is replace.
    /// </summary>
    /// <param name="scene">The scene the event is occurring in.</param>
    /// <param name="newContainer">The newly created container GameObject.</param>
    public void OnReplace(Scene scene, GameObject newContainer);
}
