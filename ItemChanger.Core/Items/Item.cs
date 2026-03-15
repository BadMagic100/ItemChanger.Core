using System;
using System.Collections.Generic;
using ItemChanger.Containers;
using ItemChanger.Enums;
using ItemChanger.Events.Args;
using ItemChanger.Extensions;
using ItemChanger.Logging;
using ItemChanger.Placements;
using Newtonsoft.Json;
using UnityEngine;

namespace ItemChanger.Items;

/// <summary>
/// The base class for all items.
/// </summary>
public abstract class Item : TaggableObject, IFinderCloneable
{
    /// <summary>
    /// Whether the item is loaded
    /// </summary>
    [JsonIgnore]
    public bool Loaded { get; private set; }

    [JsonProperty("ObtainState")]
    private ObtainState obtainState;

    private List<IDisposable> disposables = [];

    /// <summary>
    /// The name of the item. Item names are not guaranteed to be unique.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The UIDef associated to an item. GetResolvedUIDef() is preferred in most cases, since it accounts for the hooks which may modify the item.
    /// </summary>
    public UIDef? UIDef { get; init; }

    /// <summary>
    /// Method allowing derived item classes to initialize and place hooks. Called once during loading.
    /// </summary>
    protected virtual void DoLoad() { }

    /// <summary>
    /// Loads the item. If the item is already loaded, does nothing. Typically called by the item's placement during loading.
    /// <br/>Execution order is (modules load -> placement tags load -> items load -> placements load)
    /// </summary>
    public void LoadOnce()
    {
        if (!Loaded)
        {
            try
            {
                LoadTags();
                DoLoad();
            }
            catch (Exception e)
            {
                LoggerProxy.LogError($"Error loading item {Name}:\n{e}");
            }
            Loaded = true;
        }
    }

    /// <summary>
    /// Method allowing derived item classes to dispose hooks. Called once during unloading.
    /// </summary>
    protected virtual void DoUnload() { }

    /// <summary>
    /// Unloads the item. If the item is not loaded, does nothing. Typically called by the item's placement during unloading.
    /// <br/>Execution order is (modules unload -> placement tags unload -> items unload -> placements unload)
    /// </summary>
    public void UnloadOnce()
    {
        if (Loaded)
        {
            try
            {
                UnloadTags();
                DoUnload();
            }
            catch (Exception e)
            {
                LoggerProxy.LogError($"Error unloading item {Name}:\n{e}");
            }
            finally
            {
                disposables.DisposeAll();
            }
            Loaded = false;
        }
    }

    /// <summary>
    /// Registers a disposable such as a hook subscription for cleanup with the item unloads.
    /// </summary>
    /// <param name="disposable">The disposable to register</param>
    public void Using(IDisposable disposable)
    {
        disposables.Add(disposable);
    }

    /// <summary>
    /// Used by some placements to decide what container to use for the item. A value of "Unknown" is ignored, and usually leads to a shiny item by default.
    /// </summary>
    public virtual string GetPreferredContainer() => ContainerRegistry.UnknownContainerType;

    /// <summary>
    /// Indicates that the item can be given early in a special way from the given container.
    /// <br/> For example, SpawnGeoItem can be given early from Container.Chest by flinging geo directly from the chest.
    /// </summary>
    public virtual bool GiveEarly(string containerType) => false;

    /// <summary>
    /// Method used to determine if a unique item should be replaced (i.e. duplicates, etc). No relation to ObtainState.
    /// </summary>
    /// <returns></returns>
    public virtual bool Redundant()
    {
        return false;
    }

    /// <summary>
    /// The method called to give an item.
    /// </summary>
    public void Give(Placement? placement, GiveInfo info)
    {
        ObtainState originalState = obtainState;
        ReadOnlyGiveEventArgs readOnlyArgs = new(this, this, placement, info, originalState);
        BeforeGiveInvoke(readOnlyArgs);

        GiveEventArgs giveArgs = new(this, this, placement, info, originalState);
        ResolveItem(giveArgs);

        SetObtained();
        placement?.OnObtainedItem(this);

        Item item = giveArgs.Item!;
        info = giveArgs.Info!;

        readOnlyArgs = new(giveArgs.Orig, item, placement, info, originalState);
        OnGiveInvoke(readOnlyArgs);

        try
        {
            item.GiveImmediate(info);
        }
        catch (Exception e)
        {
            LoggerProxy.LogError($"Error on GiveImmediate for item {item.Name}:\n{e}");
        }

        AfterGiveInvoke(readOnlyArgs);

        if (item.UIDef != null)
        {
            try
            {
                item.UIDef.SendMessage(info.MessageType, () => info.Callback?.Invoke(item));
            }
            catch (Exception e)
            {
                LoggerProxy.LogError($"Error on SendMessage for item {item.Name}:\n{e}");
                info.Callback?.Invoke(item);
            }
        }
        else
        {
            info.Callback?.Invoke(item);
        }
    }

    /// <summary>
    /// Specifies the effect of giving a particular item.
    /// </summary>
    public abstract void GiveImmediate(GiveInfo info);

    /// <summary>
    /// Gets the display name of the item for display
    /// </summary>
    public string GetPreviewName(Placement? placement = null)
    {
        if (
            HasTag<Tags.DisableItemPreviewTag>()
            || placement != null && placement.HasTag<Tags.DisableItemPreviewTag>()
        )
        {
            return "???";
        }

        UIDef? def = GetResolvedUIDef(placement);
        return def?.GetPreviewName() ?? "???";
    }

    /// <summary>
    /// Gets the display sprite of the item for display
    /// </summary>
    public Sprite? GetPreviewSprite(Placement? placement = null)
    {
        if (
            HasTag<Tags.DisableItemPreviewTag>()
            || placement != null && placement.HasTag<Tags.DisableItemPreviewTag>()
        )
        {
            return null;
        }

        UIDef? def = GetResolvedUIDef(placement);
        return def?.GetSprite();
    }

    /// <summary>
    /// Returns the UIDef of the item yielded after all of the events for modifying items.
    /// </summary>
    public UIDef? GetResolvedUIDef(Placement? placement = null)
    {
        GiveEventArgs args = new(this, this, placement, null, obtainState);
        ResolveItem(args);
        return args.Item!.UIDef;
    }

    /// <summary>
    /// Determines the item yielded after all of the events for modifying items, by acting in place on the GiveEventArgs.
    /// </summary>
    public virtual void ResolveItem(GiveEventArgs args)
    {
        ModifyItemInvoke(args);

        if (args.Item?.Redundant() ?? true)
        {
            ModifyRedundantItemInvoke(args);
        }

        args.Item ??= NullItem.Create();
    }

    /// <summary>
    /// Marks the item as available to be given again. Used, for example, with persistent and semipersistent items.
    /// </summary>
    public void RefreshObtained()
    {
        if (obtainState == ObtainState.Obtained)
        {
            obtainState = ObtainState.Refreshed;
        }
    }

    /// <summary>
    /// Marks the item as obtained and no longer eligible to be given. Called by Give().
    /// </summary>
    public void SetObtained()
    {
        obtainState = ObtainState.Obtained;
    }

    /// <summary>
    /// Returns whether the item is currently obtained. A value of true indicates the item is not eligible to be given.
    /// </summary>
    public bool IsObtained()
    {
        return obtainState == ObtainState.Obtained;
    }

    /// <summary>
    /// Returns whether the item has ever been obtained, regardless of whether it is currently refreshed.
    /// </summary>
    public bool WasEverObtained()
    {
        return obtainState != ObtainState.Unobtained;
    }

    /// <summary>
    /// Event invoked by this item at the start of Give(), giving access to the initial give parameters.
    /// </summary>
    public event Action<ReadOnlyGiveEventArgs>? BeforeGive;

    /// <summary>
    /// Event invoked by each item at the start of Give(), giving access to the initial give parameters.
    /// </summary>
    public static event Action<ReadOnlyGiveEventArgs>? BeforeGiveGlobal;

    private void BeforeGiveInvoke(ReadOnlyGiveEventArgs args)
    {
        try
        {
            BeforeGiveGlobal?.Invoke(args);
            BeforeGive?.Invoke(args);
        }
        catch (Exception e)
        {
            string? placement = args?.Placement?.Name;
            if (placement != null)
            {
                LoggerProxy.LogError(
                    $"Error invoking BeforeGive for item {Name} at placement {placement}:\n{e}"
                );
            }
            else
            {
                LoggerProxy.LogError(
                    $"Error invoking BeforeGive for item {Name} with placement unavailable:\n{e}"
                );
            }
        }
    }

    /// <summary>
    /// Event invoked by this item during Give() to allow modification of any of the give parameters, including the item given.
    /// </summary>
    public event Action<GiveEventArgs>? ModifyItem;

    /// <summary>
    /// Event invoked by each item during Give() to allow modification of any of the give parameters, including the item given.
    /// </summary>
    public static event Action<GiveEventArgs>? ModifyItemGlobal;

    private void ModifyItemInvoke(GiveEventArgs args)
    {
        try
        {
            ModifyItemGlobal?.Invoke(args);
            ModifyItem?.Invoke(args);
        }
        catch (Exception e)
        {
            string? placement = args?.Placement?.Name;
            if (placement != null)
            {
                LoggerProxy.LogError(
                    $"Error invoking ModifyItem for item {Name} at placement {placement}:\n{e}"
                );
            }
            else
            {
                LoggerProxy.LogError(
                    $"Error invoking ModifyItem for item {Name} with placement unavailable:\n{e}"
                );
            }
        }
    }

    /// <summary>
    /// Event invoked by this item after the ModifyItem events, if the resulting item is null or redundant.
    /// </summary>
    public event Action<GiveEventArgs>? ModifyRedundantItem;

    /// <summary>
    /// Event invoked by each item after the ModifyItem events, if the resulting item is null or redundant.
    /// </summary>
    public static event Action<GiveEventArgs>? ModifyRedundantItemGlobal;

    private void ModifyRedundantItemInvoke(GiveEventArgs args)
    {
        try
        {
            ModifyRedundantItemGlobal?.Invoke(args);
            ModifyRedundantItem?.Invoke(args);
        }
        catch (Exception e)
        {
            string? placement = args?.Placement?.Name;
            if (placement != null)
            {
                LoggerProxy.LogError(
                    $"Error invoking ModifyRedundantItem for item {Name} at placement {placement}:\n{e}"
                );
            }
            else
            {
                LoggerProxy.LogError(
                    $"Error invoking ModifyRedundantItem for item {Name} with placement unavailable:\n{e}"
                );
            }
        }
    }

    /// <summary>
    /// Event invoked by this item just before GiveImmediate(), giving access to the final give parameters.
    /// </summary>
    public event Action<ReadOnlyGiveEventArgs>? OnGive;

    /// <summary>
    /// Event invoked by each item just before GiveImmediate(), giving access to the final give parameters.
    /// </summary>
    public static event Action<ReadOnlyGiveEventArgs>? OnGiveGlobal;

    private void OnGiveInvoke(ReadOnlyGiveEventArgs args)
    {
        try
        {
            OnGiveGlobal?.Invoke(args);
            OnGive?.Invoke(args);
        }
        catch (Exception e)
        {
            string? placement = args?.Placement?.Name;
            if (placement != null)
            {
                LoggerProxy.LogError(
                    $"Error invoking OnGive for item {Name} at placement {placement}:\n{e}"
                );
            }
            else
            {
                LoggerProxy.LogError(
                    $"Error invoking OnGive for item {Name} with placement unavailable:\n{e}"
                );
            }
        }
    }

    /// <summary>
    /// Event invoked by this item just after GiveImmediate(), giving access to the final give parameters.
    /// </summary>
    public event Action<ReadOnlyGiveEventArgs>? AfterGive;

    /// <summary>
    /// Event invoked by each item just after GiveImmediate(), giving access to the final give parameters.
    /// </summary>
    public static event Action<ReadOnlyGiveEventArgs>? AfterGiveGlobal;

    private void AfterGiveInvoke(ReadOnlyGiveEventArgs args)
    {
        try
        {
            AfterGiveGlobal?.Invoke(args);
            AfterGive?.Invoke(args);
        }
        catch (Exception e)
        {
            string? placement = args?.Placement?.Name;
            if (placement != null)
            {
                LoggerProxy.LogError(
                    $"Error invoking AfterGive for item {Name} at placement {placement}:\n{e}"
                );
            }
            else
            {
                LoggerProxy.LogError(
                    $"Error invoking AfterGive for item {Name} with placement unavailable:\n{e}"
                );
            }
        }
    }
}
