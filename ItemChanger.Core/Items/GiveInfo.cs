using System;
using ItemChanger.Enums;
using UnityEngine;

namespace ItemChanger.Items;

/// <summary>
/// The parameters included when an item is given. May be null.
/// </summary>
public class GiveInfo
{
    /// <summary>
    /// The best description of the most specific container for this item.
    /// </summary>
    public string? Container { get; set; }

    /// <summary>
    /// How objects are allowed to be flung.
    /// </summary>
    public FlingType FlingType { get; set; }

    /// <summary>
    /// The transform to use for flinging and similar actions. May be null.
    /// </summary>
    public Transform? Transform { get; set; }

    /// <summary>
    /// A flag enumeration of the allowed message types for the UIDef after the item is given.
    /// </summary>
    public MessageType MessageType { get; set; }

    /// <summary>
    /// A callback set by the location or placement to be executed by the UIDef when its message is complete.
    /// </summary>
    public Action<Item>? Callback { get; set; }

    /// <summary>
    /// Returns a shallow clone of the GiveInfo.
    /// </summary>
    public GiveInfo Clone()
    {
        return (GiveInfo)MemberwiseClone();
    }
}
