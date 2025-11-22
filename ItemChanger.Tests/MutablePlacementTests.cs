using System.Diagnostics.CodeAnalysis;
using ItemChanger.Containers;
using ItemChanger.Costs;
using ItemChanger.Items;
using ItemChanger.Locations;
using ItemChanger.Placements;
using ItemChanger.Tags;
using ItemChanger.Tests.Fixtures;

namespace ItemChanger.Tests;

[Collection(RequiresHostCollection.NAME)]
public sealed class MutablePlacementTests : IDisposable
{
    private const string PlacementName = "Placement";
    private readonly TestHost host;
    private readonly ContainerRegistry registry;

    public MutablePlacementTests()
    {
        host = new TestHost();
        registry = host.ContainerRegistry;
    }

    public void Dispose()
    {
        host.Profile.Dispose();
        host.Dispose();
    }

    [Fact]
    public void ChooseContainerType_ForceDefaultUsesDefaultSingle()
    {
        TestContainerLocation location = new("ForceDefault") { ForceDefaultContainer = true };
        MutablePlacement placement = new(PlacementName) { Location = location };
        placement.Add(new PreferredContainerItem("Item", "Custom"));

        string container = MutablePlacement.ChooseContainerType(
            placement,
            location,
            placement.Items
        );

        Assert.Equal(registry.DefaultSingleItemContainer.Name, container);
    }

    [Fact]
    public void ChooseContainerType_UsesPrioritizedOriginalContainerWhenSupported()
    {
        string originalType = RegisterContainer("Original");
        string preferredType = RegisterContainer("Preferred");
        TestContainerLocation location = new("WithOriginal") { ForceDefaultContainer = false };
        MutablePlacement placement = new(PlacementName) { Location = location };
        placement.AddTag(
            new OriginalContainerTag { ContainerType = originalType, Priority = true }
        );
        placement.Add(new PreferredContainerItem("Item", preferredType));

        string container = MutablePlacement.ChooseContainerType(
            placement,
            location,
            placement.Items
        );

        Assert.Equal(originalType, container);
    }

    [Fact]
    public void ChooseContainerType_RespectsItemPreferredContainer()
    {
        string preferredType = RegisterContainer("ItemPreferred");
        TestContainerLocation location = new("ItemPreference") { ForceDefaultContainer = false };
        MutablePlacement placement = new(PlacementName) { Location = location };
        placement.Add(new PreferredContainerItem("Item", preferredType));

        string container = MutablePlacement.ChooseContainerType(
            placement,
            location,
            placement.Items
        );

        Assert.Equal(preferredType, container);
    }

    [Fact]
    public void ChooseContainerType_ForcedOriginalUsedEvenWhenMissingCapabilities()
    {
        string forcedType = RegisterContainer(
            "ForcedOriginal",
            instantiate: true,
            capabilities: ContainerCapabilities.None
        );
        string fallbackType = RegisterContainer("Capable");
        TestContainerLocation location = new("NeedsPay") { ForceDefaultContainer = false };
        MutablePlacement placement = new(PlacementName)
        {
            Location = location,
            Cost = new TestCost(),
        };
        placement.AddTag(new OriginalContainerTag { ContainerType = forcedType, Force = true });
        placement.Add(new PreferredContainerItem("Item", fallbackType));

        string container = MutablePlacement.ChooseContainerType(
            placement,
            location,
            placement.Items
        );

        Assert.Equal(forcedType, container);
    }

    private string RegisterContainer(
        string name,
        bool instantiate = true,
        uint capabilities = uint.MaxValue
    )
    {
        StubContainer container = new(name, instantiate, capabilities);
        registry.DefineContainer(container);
        return container.Name;
    }

    private sealed class TestContainerLocation : ContainerLocation
    {
        [SetsRequiredMembers]
        public TestContainerLocation(string name)
        {
            Name = name;
            SceneName = "Scene";
        }

        protected override void DoLoad() { }

        protected override void DoUnload() { }

        public override Placement Wrap() =>
            throw new NotImplementedException("Wrap is unused in these tests.");
    }

    private sealed class PreferredContainerItem : Item
    {
        private readonly string preferredContainer;

        [SetsRequiredMembers]
        public PreferredContainerItem(string name, string preferredContainer)
        {
            Name = name;
            this.preferredContainer = preferredContainer;
        }

        public override string GetPreferredContainer() => preferredContainer;

        public override void GiveImmediate(GiveInfo info) { }
    }

    private sealed class StubContainer(string name, bool instantiate, uint supportedCapabilities)
        : Container
    {
        public override string Name { get; } = name;

        public override bool SupportsInstantiate => instantiate;

        public override uint SupportedCapabilities { get; } = supportedCapabilities;

        protected override void Load() { }

        protected override void Unload() { }
    }

    private sealed class TestCost : Cost
    {
        public override bool CanPay() => true;

        public override void OnPay() { }

        public override string GetCostText() => "Test Cost";

        public override bool IsFree => false;

        public override bool HasPayEffects() => false;
    }
}
