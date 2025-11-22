using System;
using ItemChanger.Events.Args;
using ItemChanger.Items;
using ItemChanger.Tags.Constraints;

namespace ItemChanger.Tags;

/// <summary>
/// Tag which triggers a recursive search through the AbstractItem.ModifyItem hook.
/// <br />Recursion is by looking up the predecessor and successor items in Finder, and basing a search at their ItemChainTags.
/// <br />Selected item is first nonredundant item in the sequence, or null (handled by AbstractItem) if all items are redundant.
/// </summary>
[ItemTag]
public class ItemChainTag : Tag
{
    /// <summary>
    /// The previous item in the item chain
    /// </summary>
    public string? Predecessor { get; init; }

    /// <summary>
    /// The subsequent item in the item chain
    /// </summary>
    public string? Successor { get; init; }

    /// <inheritdoc/>
    protected override void DoLoad(TaggableObject parent)
    {
        Item item = (Item)parent;
        item.ModifyItem += ModifyItem;
    }

    /// <inheritdoc/>
    protected override void DoUnload(TaggableObject parent)
    {
        Item item = (Item)parent;
        item.ModifyItem -= ModifyItem;
    }

    /// <summary>
    /// Retrieves an item by name. Default implementation queries the global <see cref="Finder"/>, but subclasses can resolve names differently.
    /// </summary>
    protected virtual Item GetItem(string name)
    {
        return ItemChangerHost.Singleton.Finder.GetItem(name) ?? throw new ArgumentException(
                "Could not find item " + name,
                nameof(name)
            );
    }

    private void ModifyItem(GiveEventArgs args)
    {
        Item? current = args.Item;
        if (current == null)
        {
            return;
        }

        args.Item = current.Redundant()
            ? TraverseSuccessors(current)
            : TraversePredecessors(current);
    }

    private Item? TraverseSuccessors(Item item)
    {
        Item? current = item;
        while (
            current != null
            && current.GetTag<ItemChainTag>()?.Successor is string successor
            && !string.IsNullOrEmpty(successor)
        )
        {
            current = GetItem(successor);
            if (current is not null && !current.Redundant())
            {
                return current;
            }
        }

        return null;
    }

    private Item? TraversePredecessors(Item item)
    {
        Item? current = item;
        while (
            current?.GetTag<ItemChainTag>()?.Predecessor is string predecessor
            && !string.IsNullOrEmpty(predecessor)
        )
        {
            Item candidate = GetItem(predecessor);
            if (candidate.Redundant())
            {
                return current;
            }

            current = candidate;
        }

        return current;
    }
}
