using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ItemChanger.Events;
using ItemChanger.Events.Args;
using ItemChanger.Items;
using ItemChanger.Locations;
using ItemChanger.Logging;

namespace ItemChanger;

/// <summary>
/// Represents a named collection of lookup entries that the <see cref="Finder"/> can search through.
/// </summary>
/// <typeparam name="T">Type stored inside the sheet.</typeparam>
public class FinderSheet<T>(Dictionary<string, T> members, float priority)
{
    /// <summary>
    /// Gets the order in which this sheet should be queried relative to other sheets.
    /// </summary>
    public float Priority => priority;

    /// <summary>
    /// Gets all names exposed by this sheet.
    /// </summary>
    public IEnumerable<string> Names => members.Keys;

    /// <summary>
    /// Returns whether the sheet participates in searches.
    /// </summary>
    public virtual bool Enabled => true;

    /// <summary>
    /// Attempts to resolve an entry by name.
    /// </summary>
    /// <param name="name">Entry to locate.</param>
    /// <param name="result">Resolved entry if found.</param>
    /// <returns><see langword="true"/> if the entry exists; otherwise <see langword="false"/>.</returns>
    public bool TryGet(string name, [NotNullWhen(true)] out T? result)
    {
        return members.TryGetValue(name, out result!);
    }
}

/// <summary>
/// Provides lookup utilities for items and locations registered within ItemChanger, acting as a template registry so consumers can define and retrieve canonical objects instead of instantiating them ad-hoc.
/// </summary>
public class Finder
{
    /// <summary>
    /// Invoked by Finder.GetItem. The initial arguments are the requested name, and null. If the event finishes with a non-null item, that item is returned to the requester.
    /// <br/>Otherwise, the ItemChanger internal implementation of that item is cloned and returned, if it exists. Otherwise, null is returned.
    /// </summary>
    public event Action<GetItemEventArgs> GetItemOverride
    {
        add => getItemOverrideSubscribers.Add(value);
        remove => getItemOverrideSubscribers.Remove(value);
    }
    private readonly List<Action<GetItemEventArgs>> getItemOverrideSubscribers = [];

    /// <summary>
    /// Invoked by Finder.GetLocation. The initial arguments are the requested name, and null. If the event finishes with a non-null location, that location is returned to the requester.
    /// <br/>Otherwise, the ItemChanger internal implementation of that location is cloned and returned, if it exists. Otherwise, null is returned.
    /// </summary>
    public event Action<GetLocationEventArgs> GetLocationOverride
    {
        add => getLocationOverrideSubscribers.Add(value);
        remove => getLocationOverrideSubscribers.Remove(value);
    }
    private readonly List<Action<GetLocationEventArgs>> getLocationOverrideSubscribers = [];

    private readonly Dictionary<string, Item> Items = [];
    private readonly Dictionary<string, Location> Locations = [];

    private readonly List<FinderSheet<Item>> ItemSheets = [];
    private readonly List<FinderSheet<Location>> LocationSheets = [];

    /// <summary>
    /// Gets all known item names from both the internal catalog and any registered sheets.
    /// </summary>
    public IEnumerable<string> ItemNames =>
        Items.Keys.Concat(ItemSheets.SelectMany(s => s.Names)).Distinct();

    /// <summary>
    /// Gets all known location names from both the internal catalog and any registered sheets.
    /// </summary>
    public IEnumerable<string> LocationNames =>
        Locations.Keys.Concat(LocationSheets.SelectMany(s => s.Names)).Distinct();

    /// <summary>
    /// The most general method for looking up an item. Invokes an event to allow subscribers to modify the search result. Return value defaults to that of GetItemInternal.
    /// </summary>
    public Item? GetItem(string name)
    {
        GetItemEventArgs args = new(name);
        InvokeHelper.InvokeList(args, getItemOverrideSubscribers);
        if (args.Current != null)
        {
            return args.Current.DeepClone();
        }
        else
        {
            return GetItemInternal(name);
        }
    }

    /// <summary>
    /// Searches for the item by name, first in the custom item list, then in the list of enabled additional item sheets by priority. Returns null if not found.
    /// </summary>
    public Item? GetItemInternal(string name)
    {
        if (Items.TryGetValue(name, out Item? item))
        {
            return item.DeepClone();
        }
        foreach (FinderSheet<Item> sheet in ItemSheets)
        {
            if (sheet.Enabled && sheet.TryGet(name, out Item? item1))
            {
                return item1.DeepClone();
            }
        }
        return null;
    }

    /// <summary>
    /// The most general method for looking up a location. Invokes an event to allow subscribers to modify the search result. Return value defaults to that of GetLocationInternal.
    /// </summary>
    public Location? GetLocation(string name)
    {
        GetLocationEventArgs args = new(name);
        InvokeHelper.InvokeList(args, getLocationOverrideSubscribers);
        if (args.Current != null)
        {
            return args.Current.DeepClone();
        }
        else
        {
            return GetLocationInternal(name);
        }
    }

    /// <summary>
    /// Searches for the location by name, first in the custom location list, then in the list of enabled additional location sheets by priority. Returns null if not found.
    /// </summary>
    public Location? GetLocationInternal(string name)
    {
        if (Locations.TryGetValue(name, out Location? location))
        {
            return location.DeepClone();
        }
        foreach (FinderSheet<Location> sheet in LocationSheets)
        {
            if (sheet.Enabled && sheet.TryGet(name, out Location? location1))
            {
                return location1.DeepClone();
            }
        }
        return null;
    }

    /// <summary>
    /// Registers a custom item for later lookup.
    /// </summary>
    /// <param name="item">Item to register.</param>
    /// <param name="overwrite">If <see langword="true"/>, replaces any existing item with the same name.</param>
    /// <exception cref="ArgumentException">Thrown when the name already exists and <paramref name="overwrite"/> is false.</exception>
    public void DefineItem(Item item, bool overwrite = false)
    {
        if (Items.ContainsKey(item.Name) && !overwrite)
        {
            throw new ArgumentException(
                $"Item {item.Name} is already defined (type is {item.GetType()})."
            );
        }

        Items[item.Name] = item;
    }

    /// <summary>
    /// Registers a custom location for later lookup.
    /// </summary>
    /// <param name="loc">Location to register.</param>
    /// <param name="overwrite">If <see langword="true"/>, replaces any existing location with the same name.</param>
    /// <exception cref="ArgumentException">Thrown when the name already exists and <paramref name="overwrite"/> is false.</exception>
    public void DefineLocation(Location loc, bool overwrite = false)
    {
        if (Locations.ContainsKey(loc.Name) && !overwrite)
        {
            throw new ArgumentException(
                $"Location {loc.Name} is already defined (type is {loc.GetType()})."
            );
        }

        Locations[loc.Name] = loc;
    }

    /// <summary>
    /// Adds an ItemSheet to finder.
    /// </summary>
    public void DefineItemSheet(FinderSheet<Item> sheet)
    {
        int i = 0;
        while (i < ItemSheets.Count && ItemSheets[i].Priority > sheet.Priority)
        {
            i++;
        }
        ItemSheets.Insert(i, sheet);
    }

    /// <summary>
    /// Adds a LocationSheet to finder.
    /// </summary>
    public void DefineLocationSheet(FinderSheet<Location> sheet)
    {
        int i = 0;
        while (i < LocationSheets.Count && LocationSheets[i].Priority > sheet.Priority)
        {
            i++;
        }
        LocationSheets.Insert(i, sheet);
    }
}
