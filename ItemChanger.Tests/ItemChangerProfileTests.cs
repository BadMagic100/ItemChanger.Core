using System.Diagnostics.CodeAnalysis;
using ItemChanger.Enums;
using ItemChanger.Items;
using ItemChanger.Placements;
using ItemChanger.Tests.Fixtures;

namespace ItemChanger.Tests;

[Collection(RequiresHostCollection.NAME)]
public sealed class ItemChangerProfileTests : IDisposable
{
    private readonly TestHost host;
    private readonly ItemChangerProfile profile;

    public ItemChangerProfileTests()
    {
        host = new TestHost();
        profile = host.Profile;
    }

    public void Dispose()
    {
        profile.Dispose();
        host.Dispose();
    }

    [Fact]
    public void AddPlacement_MergeKeepingNew_ReplacesPlacementAndUnloadsOriginal()
    {
        TrackingPlacement original = new("Alpha");
        TrackingItem oldItem = new("Old");
        original.Add(oldItem);
        profile.AddPlacement(original);

        profile.Load();

        TrackingPlacement replacement = new("Alpha");
        TrackingItem newItem = new("New");
        replacement.Add(newItem);

        profile.AddPlacement(replacement, PlacementConflictResolution.MergeKeepingNew);

        Placement result = profile.GetPlacement("Alpha");
        Assert.Same(replacement, result);
        Assert.Equal(1, replacement.LoadCount);
        Assert.Equal(1, original.UnloadCount);
        Assert.Equal(2, replacement.Items.Count);
        Assert.Equal(2, oldItem.LoadCount);
        Assert.Equal(1, oldItem.UnloadCount);
        Assert.Equal(1, newItem.LoadCount);
    }

    [Fact]
    public void AddPlacement_MergeKeepingOld_LoadsIncomingItemsWhenProfileLoaded()
    {
        TrackingPlacement placement = new("Beta");
        TrackingItem existing = new("Existing");
        placement.Add(existing);
        profile.AddPlacement(placement);

        profile.Load();

        TrackingItem incomingItem = new("Incoming");
        TrackingPlacement incoming = new("Beta");
        incoming.Add(incomingItem);

        profile.AddPlacement(incoming, PlacementConflictResolution.MergeKeepingOld);

        Assert.Contains(incomingItem, placement.Items);
        Assert.Equal(1, incomingItem.LoadCount);
        Assert.Equal(0, placement.UnloadCount);
        Assert.Equal(0, incoming.LoadCount);
    }

    private sealed class TrackingPlacement(string name) : Placement(name)
    {
        public int LoadCount { get; private set; }
        public int UnloadCount { get; private set; }

        protected override void DoLoad()
        {
            LoadCount++;
        }

        protected override void DoUnload()
        {
            UnloadCount++;
        }
    }

    private sealed class TrackingItem : Item
    {
        [SetsRequiredMembers]
        public TrackingItem(string name)
        {
            Name = name;
        }

        public int LoadCount { get; private set; }
        public int UnloadCount { get; private set; }

        public override void GiveImmediate(GiveInfo info) { }

        protected override void DoLoad()
        {
            LoadCount++;
        }

        protected override void DoUnload()
        {
            UnloadCount++;
        }
    }
}
