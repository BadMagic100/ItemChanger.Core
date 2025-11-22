using System;

namespace ItemChanger.Enums;

/// <summary>
/// Flag enum describing the states a placement has reached during gameplay.
/// </summary>
[Flags]
public enum VisitState
{
    /// <summary>
    /// The placement has not been visited
    /// </summary>
    None = 0,

    /// <summary>
    /// Any item from the placement has been obtained
    /// </summary>
    ObtainedAnyItem = 1 << 0,

    /// <summary>
    /// The content of the placement has been previewed, such as through a local hint box or shop UI.
    /// </summary>
    Previewed = 1 << 1,

    /// <summary>
    /// The state of the related container is permanently changed. When reloading, the
    /// container should retain the new state. For example, a chest is opened and remains open.
    /// </summary>
    Opened = 1 << 2,

    /// <summary>
    /// The event which causes the location to produce a container has been completed. If respawned or revisited
    /// without collecting items, the container should appear. For example, an enemy location drops the container
    /// after being defeated and does not need to be re-fought to respawn the container.
    /// </summary>
    Dropped = 1 << 3,

    /// <summary>
    /// The in-game event tied to the location has been completed. If respawned, it should be replaced by a container.
    /// For example, an NPC offers you an item and you accept the item. The NPC will be replaced by container if the
    /// item is respawned.
    /// </summary>
    Accepted = 1 << 4,

    /// <summary>
    /// Defined on a per-placement basis.
    /// </summary>
    Special = 1 << 31,
}
