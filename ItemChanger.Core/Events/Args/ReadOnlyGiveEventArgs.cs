using System;
using ItemChanger.Enums;
using ItemChanger.Items;
using ItemChanger.Placements;
using UnityEngine;

namespace ItemChanger.Events.Args;

/// <summary>
/// Event arguments exposing read-only information about an item being given.
/// </summary>
public class ReadOnlyGiveEventArgs(
    Item orig,
    Item item,
    Placement? placement,
    GiveInfo info,
    ObtainState state
) : EventArgs
{
    /// <summary>Original item identifier passed into the give request.</summary>
    public Item Orig => orig;

    /// <summary>Item actually awarded after any mutations.</summary>
    public Item Item => item;

    /// <summary>Placement that initiated the give, if applicable.</summary>
    public Placement? Placement => placement;

    /// <summary>Name of the container used to present the item.</summary>
    public string? Container => info.Container;

    /// <summary>Fling type applied to spawned pickups.</summary>
    public FlingType Fling => info.FlingType;

    /// <summary>Transform controlling pickup placement.</summary>
    public Transform? Transform => info.Transform;

    /// <summary>UI message types permitted for showing the item.</summary>
    public MessageType MessageType => info.MessageType;

    /// <summary>Callback invoked after the UI message completes.</summary>
    public Action<Item>? Callback => info.Callback;

    /// <summary>Previous obtain state of the item before the give occurred.</summary>
    public ObtainState OriginalState => state;
}
