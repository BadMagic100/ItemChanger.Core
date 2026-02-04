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
public sealed class ContainerLocationTests : IDisposable
{
    private readonly TestHost host;
    private readonly ContainerRegistry registry;

    public ContainerLocationTests()
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
        Placement placement = location.Wrap();
        placement.Add(new PreferredContainerItem("Item", "Custom"));
        placement.LoadOnce();

        string container = location.ChooseContainerType();

        Assert.Equal(registry.DefaultSingleItemContainer.Name, container);
    }

    [Fact]
    public void ChooseContainerType_UsesPrioritizedOriginalContainerWhenSupported()
    {
        string originalType = RegisterContainer("Original");
        string preferredType = RegisterContainer("Preferred");
        TestContainerLocation location = new("WithOriginal") { ForceDefaultContainer = false };
        Placement placement = location.Wrap();
        placement.AddTag(
            new OriginalContainerTag { ContainerType = originalType, Priority = true }
        );
        placement.Add(new PreferredContainerItem("Item", preferredType));
        placement.LoadOnce();

        string container = location.ChooseContainerType();

        Assert.Equal(originalType, container);
    }

    [Fact]
    public void ChooseContainerType_ReplacesPrioritizedOriginalContainerWhenUnsupported()
    {
        string originalType = RegisterContainer("Original");
        string preferredType = RegisterContainer("Preferred");
        TestContainerLocation location = new("WithoutOriginal") { ForceDefaultContainer = false };
        location.Disallow(originalType);
        Placement placement = location.Wrap();
        placement.AddTag(
            new OriginalContainerTag { ContainerType = originalType, Priority = true }
        );
        placement.Add(new PreferredContainerItem("Item", preferredType));
        placement.LoadOnce();

        string container = location.ChooseContainerType();

        Assert.Equal(preferredType, container);
    }

    [Fact]
    public void ChooseContainerType_RespectsItemPreferredContainer()
    {
        string preferredType = RegisterContainer("ItemPreferred");
        TestContainerLocation location = new("ItemPreference") { ForceDefaultContainer = false };
        Placement placement = location.Wrap();
        placement.Add(new PreferredContainerItem("Item", preferredType));
        placement.LoadOnce();

        string container = location.ChooseContainerType();

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
        MutablePlacement placement = (MutablePlacement)location.Wrap();
        placement.Cost = new TestCost();
        placement.AddTag(new OriginalContainerTag { ContainerType = forcedType, Force = true });
        placement.Add(new PreferredContainerItem("Item", fallbackType));
        placement.LoadOnce();

        string container = location.ChooseContainerType();
        bool replaced = location.WillBeReplaced();

        Assert.Equal(forcedType, container);
    }

    [Fact]
    public void ChooseContainerType_NoPlacement_ReturnsOriginalContainerTypeWhenSpecified()
    {
        string preferredType = RegisterContainer("Preferred");
        TestContainerLocation location = new("Test");
        location.AddTag(new OriginalContainerTag { ContainerType = preferredType });

        string container = location.ChooseContainerType();
        Assert.Equal(preferredType, container);
    }

    [Fact]
    public void ChooseContainerType_NoPlacement_ReturnsDefaultContainerTypeWhenNoOriginalSpecified()
    {
        TestContainerLocation location = new("Test");

        string container = location.ChooseContainerType();
        Assert.Equal(host.ContainerRegistry.DefaultSingleItemContainer.Name, container);
    }

    [Fact]
    public void GetOriginalContainerType_ReturnsOriginalContainerTypeWhenSpecified()
    {
        string originalType = "OriginalType";
        TestContainerLocation location = new("TestLocation");
        location.AddTag(new OriginalContainerTag { ContainerType = originalType });

        string? result = location.GetOriginalContainerType();

        Assert.Equal(originalType, result);
    }

    [Fact]
    public void GetOriginalContainerType_ReturnsNullWhenNoOriginalContainerSpecified()
    {
        TestContainerLocation location = new("TestLocation");

        string? result = location.GetOriginalContainerType();

        Assert.Null(result);
    }

    [Fact]
    public void GetOriginalContainerType_ReturnsOriginalContainerTypeFromPlacementTags()
    {
        string originalType = "OriginalTypeFromPlacement";
        TestContainerLocation location = new("TestLocation");
        Placement placement = location.Wrap();
        placement.AddTag(new OriginalContainerTag { ContainerType = originalType });
        placement.LoadOnce();

        string? result = location.GetOriginalContainerType();

        Assert.Equal(originalType, result);
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
        private readonly HashSet<string> disallowedContainers = [];

        [SetsRequiredMembers]
        public TestContainerLocation(string name)
        {
            Name = name;
            SceneName = "Scene";
        }

        public void Disallow(string containerType)
        {
            disallowedContainers.Add(containerType);
        }

        public override bool Supports(string containerType)
        {
            if (disallowedContainers.Contains(containerType))
            {
                return false;
            }

            return base.Supports(containerType);
        }

        protected override void DoLoad() { }

        protected override void DoUnload() { }

        public override Placement Wrap() => new MutablePlacement(Name) { Location = this };
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
