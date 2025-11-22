using System.Diagnostics.CodeAnalysis;
using ItemChanger.Enums;
using ItemChanger.Events.Args;
using ItemChanger.Items;
using ItemChanger.Tags;

namespace ItemChanger.Tests;

public sealed class ItemChainTagTests
{
    [Fact]
    public void RedundantItemAdvancesToFirstNonRedundantSuccessor()
    {
        Dictionary<string, ChainTestItem> items = [];
        ChainTestItem redundant = CreateItem("First", redundant: true, items);
        ChainTestItem target = CreateItem("Second", redundant: false, items);

        redundant.AddTag(new StubItemChainTag(items) { Successor = target.Name });
        redundant.LoadOnce();
        target.LoadOnce();

        GiveEventArgs args = CreateArgs(redundant);
        redundant.ResolveItem(args);

        Assert.Same(target, args.Item);
    }

    [Fact]
    public void NonRedundantItemWalksBackToEarliestAvailablePredecessor()
    {
        Dictionary<string, ChainTestItem> items = [];
        ChainTestItem root = CreateItem("Root", redundant: false, items);
        ChainTestItem middle = CreateItem("Middle", redundant: false, items);
        ChainTestItem leaf = CreateItem("Leaf", redundant: false, items);

        middle.AddTag(new StubItemChainTag(items) { Predecessor = root.Name });
        leaf.AddTag(new StubItemChainTag(items) { Predecessor = middle.Name });
        root.LoadOnce();
        middle.LoadOnce();
        leaf.LoadOnce();

        GiveEventArgs args = CreateArgs(leaf);
        leaf.ResolveItem(args);

        Assert.Same(root, args.Item);
    }

    [Fact]
    public void RedundantChainFallsBackToNullItem()
    {
        Dictionary<string, ChainTestItem> items = [];
        ChainTestItem a = CreateItem("A", redundant: true, items);
        ChainTestItem b = CreateItem("B", redundant: true, items);
        ChainTestItem c = CreateItem("C", redundant: true, items);

        a.AddTag(new StubItemChainTag(items) { Successor = b.Name });
        b.AddTag(new StubItemChainTag(items) { Successor = c.Name });

        a.LoadOnce();
        b.LoadOnce();
        c.LoadOnce();

        GiveEventArgs args = CreateArgs(a);
        a.ResolveItem(args);

        Assert.IsType<NullItem>(args.Item);
    }

    private static ChainTestItem CreateItem(
        string name,
        bool redundant,
        IDictionary<string, ChainTestItem> lookup
    )
    {
        ChainTestItem item = new(name, redundant);
        lookup[name] = item;
        return item;
    }

    private static GiveEventArgs CreateArgs(Item item) =>
        new(item, item, placement: null, info: null, ObtainState.Unobtained);

    private sealed class ChainTestItem : Item
    {
        private readonly bool redundant;

        [SetsRequiredMembers]
        public ChainTestItem(string name, bool redundant)
        {
            Name = name;
            this.redundant = redundant;
        }

        public override bool Redundant() => redundant;

        public override void GiveImmediate(GiveInfo info) { }
    }

    private sealed class StubItemChainTag(IDictionary<string, ChainTestItem> lookup) : ItemChainTag
    {
        protected override Item GetItem(string name) =>
            lookup.TryGetValue(name, out ChainTestItem? item)
                ? item
                : throw new ArgumentException($"Unknown item {name}", nameof(name));
    }
}
