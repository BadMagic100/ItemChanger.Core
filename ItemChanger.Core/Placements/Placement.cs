using System;
using System.Collections.Generic;
using System.Linq;
using ItemChanger.Containers;
using ItemChanger.Enums;
using ItemChanger.Events.Args;
using ItemChanger.Items;
using ItemChanger.Logging;
using ItemChanger.Tags;
using Newtonsoft.Json;

namespace ItemChanger.Placements;

/// <summary>
/// The base class for all placements. Placements carry a list of items and specify how to implement those items, often using locations.
/// </summary>
/// <remarks>
/// Creates a placement with the given name.
/// </remarks>
public abstract class Placement(string name) : TaggableObject
{
    /// <summary>
    /// Whether the placement is loaded.
    /// </summary>
    [JsonIgnore]
    public bool Loaded { get; private set; }

    /// <summary>
    /// The name of the placement. Placement names are enforced to be unique.
    /// </summary>
    public string Name => name;

    /// <summary>
    /// The items attached to the placement.
    /// </summary>
    public List<Item> Items { get; } = [];

    /// <summary>
    /// An enumeration of visit flags accrued by the placement. Which flags may be set depends on the placement type and other factors.
    /// </summary>
    [JsonProperty]
    public VisitState Visited { get; private set; }

    #region Give

    /// <summary>
    /// Helper method for giving all of the items of the placement in sequence, so that the UIDef message of one leads into giving the next.
    /// </summary>
    public void GiveAll(GiveInfo info, Action? callback = null)
    {
        List<Item>.Enumerator enumerator = Items.GetEnumerator();

        GiveRecursive();

        void GiveRecursive(Item? _ = null)
        {
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.IsObtained())
                {
                    continue;
                }

                GiveInfo next = info.Clone();
                next.Callback = GiveRecursive;
                enumerator.Current.Give(this, next);
                return;
            }

            callback?.Invoke();
        }
    }

    /// <summary>
    /// Called when one of the placement's items is obtained.
    /// </summary>
    /// <param name="item">Item that was obtained.</param>
    public virtual void OnObtainedItem(Item item)
    {
        AddVisitFlag(VisitState.ObtainedAnyItem);
    }

    /// <summary>
    /// Records the provided preview text on the placement.
    /// </summary>
    /// <param name="previewText">Preview message produced by the hint source.</param>
    public virtual void OnPreview(string previewText)
    {
        GetOrAddTag<PreviewRecordTag>().PreviewText = previewText;
        AddVisitFlag(VisitState.Previewed);
    }

    /// <summary>
    /// Combines and returns the preview names of the unobtained items at the placement. Used for most hints or previews.
    /// </summary>
    public string GetUIName()
    {
        return GetUIName(maxLength: 120);
    }

    /// <summary>
    /// Combines and returns the preview names of the unobtained items at the placement, trimmed to the specified length.
    /// </summary>
    public string GetUIName(int maxLength)
    {
        IEnumerable<string> itemNames = Items
            .Where(i => !i.IsObtained())
            .Select(i => i.GetPreviewName(this) ?? "Unknown Item");
        string itemText = string.Join(", ", itemNames);
        if (itemText.Length > maxLength)
        {
            itemText = itemText[..(maxLength > 3 ? maxLength - 3 : 0)] + "...";
        }

        return itemText;
    }

    #endregion

    #region Control

    /// <summary>
    /// Returns true when the placement currently has no items to give.
    /// </summary>
    public bool AllObtained()
    {
        return Items.All(i => i.IsObtained());
    }

    /// <summary>
    /// Sets the visit state of the placement to the union of its current flags and the parameter flags.
    /// </summary>
    public void AddVisitFlag(VisitState flag)
    {
        InvokeVisitStateChanged(flag);
        Visited |= flag;
    }

    /// <summary>
    /// Returns true if the flags have nonempty intersection with the placement's visit state.
    /// </summary>
    public bool CheckVisitedAny(VisitState flags)
    {
        return (Visited & flags) != VisitState.None;
    }

    /// <summary>
    /// Returns true if the flags are a subset of the placement's visit state.
    /// </summary>
    public bool CheckVisitedAll(VisitState flags)
    {
        return (Visited & flags) == flags;
    }

    #endregion

    #region Hooks

    /// <summary>
    /// Loads the placement. If the placement is already loaded, does nothing. Typically called when starting or resuming a profile.
    /// <br/>Execution order is (modules load -> placement tags load -> items load -> placements load)
    /// </summary>
    public void LoadOnce()
    {
        if (!Loaded)
        {
            try
            {
                LoadTags();
                foreach (Item item in Items)
                {
                    item.LoadOnce();
                }

                DoLoad();
            }
            catch (Exception e)
            {
                LoggerProxy.LogError($"Error loading placement {Name}:\n{e}");
            }
            Loaded = true;
        }
    }

    /// <summary>
    /// Unloads the placement. If the placement is not loaded, does nothing. Typically called when unloading a profile.
    /// <br/>Execution order is (modules unload -> placement tags unload -> items unload -> placements unload)
    /// </summary>
    public void Unload()
    {
        if (Loaded)
        {
            try
            {
                UnloadTags();
                foreach (Item item in Items)
                {
                    item.UnloadOnce();
                }

                DoUnload();
            }
            catch (Exception e)
            {
                LoggerProxy.LogError($"Error unloading placement {Name}:\n{e}");
            }
            Loaded = false;
        }
    }

    /// <summary>
    /// Method allowing derived placement classes to initialize and place hooks. Called once during loading.
    /// </summary>
    protected abstract void DoLoad();

    /// <summary>
    /// Method allowing derived placement classes to dispose hooks. Called once during unloading.
    /// </summary>
    protected abstract void DoUnload();

    /// <summary>
    /// Event invoked by each placement whenever new flags are added to its Visited. Skipped if added flags are a subset of Visited.
    /// </summary>
    public static event Action<VisitStateChangedEventArgs>? OnVisitStateChangedGlobal;

    /// <summary>
    /// Event invoked by this placement whenever AddVisitFlag is called. Use the NoChange property of the args to detect whether a change will occur.
    /// </summary>
    public event Action<VisitStateChangedEventArgs>? OnVisitStateChanged;

    private void InvokeVisitStateChanged(VisitState newFlags)
    {
        VisitStateChangedEventArgs args = new(this, newFlags);
        try
        {
            OnVisitStateChangedGlobal?.Invoke(args);
            OnVisitStateChanged?.Invoke(args);
        }
        catch (Exception e)
        {
            LoggerProxy.LogError($"Error invoking OnVisitStateChanged for placement {Name}:\n{e}");
        }
    }

    #endregion

    /// <summary>
    /// The container type that best describes the placement as a whole.
    /// </summary>
    [JsonIgnore]
    public virtual string MainContainerType => ContainerRegistry.UnknownContainerType;

    /// <summary>
    /// Returns all tags attached to the placement and any associated locations.
    /// </summary>
    public virtual IEnumerable<Tag> GetPlacementAndLocationTags()
    {
        return Tags ?? Enumerable.Empty<Tag>();
    }

    /// <summary>
    /// Adds an item to the item list.
    /// </summary>
    public virtual Placement Add(Item item)
    {
        Items.Add(item);
        return this;
    }

    /// <summary>
    /// Adds a range of items to the item list.
    /// </summary>
    public Placement Add(IEnumerable<Item> items)
    {
        foreach (Item i in items)
        {
            Add(i);
        }

        return this;
    }

    /// <summary>
    /// Adds a range of items to the item list.
    /// </summary>
    public Placement Add(params Item[] items)
    {
        foreach (Item item in items)
        {
            Add(item);
        }

        return this;
    }
}
