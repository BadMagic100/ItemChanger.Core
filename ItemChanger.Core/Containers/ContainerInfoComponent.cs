using UnityEngine;

namespace ItemChanger.Containers;

/// <summary>
/// Component that is attached to GameObjects by Containers in <see cref="Container.GetNewContainer(ContainerInfo)"/>
/// or <see cref="Container.ModifyContainerInPlace(GameObject, ContainerInfo)"/> in order to attach container information
/// to modified objects.
/// </summary>
/// <seealso cref="ContainerInfo.FindContainerInfo(GameObject)"/>
public sealed class ContainerInfoComponent : MonoBehaviour
{
    /// <summary>
    /// The ContainerInfo to attach
    /// </summary>
    public required ContainerInfo Info { get; init; }
}
