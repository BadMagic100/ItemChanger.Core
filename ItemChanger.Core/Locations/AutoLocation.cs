using System;
using ItemChanger.Containers;
using ItemChanger.Enums;
using ItemChanger.Items;
using ItemChanger.Placements;
using ItemChanger.Tags;
using Newtonsoft.Json;
using UnityEngine;

namespace ItemChanger.Locations;

/// <summary>
/// Location type which cannot accept a container, and thus must implement itself. Examples include items given in dialogue, etc.
/// </summary>
public abstract class AutoLocation : Location
{
    /// <summary>
    /// Builds the give-info descriptor used when this location awards items.
    /// </summary>
    public virtual GiveInfo GetGiveInfo()
    {
        return new GiveInfo
        {
            FlingType = FlingType,
            Callback = null,
            Container = ContainerRegistry.UnknownContainerType,
            MessageType = MessageType.Any,
        };
    }

    /// <summary>
    /// Gives every item using the default give info immediately.
    /// </summary>
    public void GiveAll()
    {
        Placement!.GiveAll(GetGiveInfo());
    }

    /// <summary>
    /// Gives all items, invoking a callback after completion.
    /// </summary>
    public void GiveAll(Action callback)
    {
        Placement!.GiveAll(GetGiveInfo(), callback);
    }

    /// <summary>
    /// Produces an asynchronous wrapper that gives all items after the provided callback completes.
    /// </summary>
    public Action<Action> GiveAllAsync(Transform t)
    {
        GiveInfo gi = GetGiveInfo();
        gi.Transform = t;
        return (callback) => Placement!.GiveAll(gi, callback);
    }

    /// <summary>
    /// Indicates whether this auto location can handle costs directly.
    /// </summary>
    [JsonIgnore]
    public virtual bool SupportsCost => false;

    /// <inheritdoc/>
    public override Placement Wrap()
    {
        return new AutoPlacement(Name)
        {
            Location = this,
            Cost = ImplicitCostTag.GetDefaultCost(this),
        };
    }
}
