using UnityEngine;

namespace ItemChanger.Containers;

public sealed class ContainerInfoComponent : MonoBehaviour
{
    public required ContainerInfo Info { get; init; }
}
