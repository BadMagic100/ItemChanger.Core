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

        string container = location.ChooseBestContainerType();

        Assert.Equal(registry.DefaultSingleItemContainer.Name, container);
    }

    [Fact]
    public void ChooseContainerType_UsesPrioritizedOriginalContainerWhenSupported()
    {
        string originalType = RegisterContainer("Original");
        string preferredType = RegisterContainer("Preferred");
        TestContainerLocation location = new("WithOriginal") { ForceDefaultContainer = false };
        Placement placement = location.Wrap();
        location.AddTag(new OriginalContainerTag { ContainerType = originalType, Priority = true });
        placement.Add(new PreferredContainerItem("Item", preferredType));
        placement.LoadOnce();

        string container = location.ChooseBestContainerType();

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

        string container = location.ChooseBestContainerType();

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

        string container = location.ChooseBestContainerType();

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
        location.AddTag(new OriginalContainerTag { ContainerType = forcedType, Force = true });
        placement.Add(new PreferredContainerItem("Item", fallbackType));
        placement.LoadOnce();

        string container = location.ChooseBestContainerType();

        Assert.Equal(forcedType, container);
    }

    [Fact]
    public void ChooseContainerType_NoPlacement_ReturnsOriginalContainerTypeWhenSpecified()
    {
        string preferredType = RegisterContainer("Preferred");
        TestContainerLocation location = new("Test");
        location.AddTag(new OriginalContainerTag { ContainerType = preferredType });

        string container = location.ChooseBestContainerType();
        Assert.Equal(preferredType, container);
    }

    [Fact]
    public void ChooseContainerType_NoPlacement_ReturnsDefaultContainerTypeWhenNoOriginalSpecified()
    {
        TestContainerLocation location = new("Test");

        string container = location.ChooseBestContainerType();
        Assert.Equal(host.ContainerRegistry.DefaultSingleItemContainer.Name, container);
    }

    [Fact]
    public void OriginalContainerType_ReturnsOriginalContainerTypeWhenSpecified()
    {
        string originalType = "OriginalType";
        TestContainerLocation location = new("TestLocation");
        location.AddTag(new OriginalContainerTag { ContainerType = originalType });

        string? result = location.OriginalContainerType;

        Assert.Equal(originalType, result);
    }

    [Fact]
    public void OriginalContainerType_ReturnsNullWhenNoOriginalContainerSpecified()
    {
        TestContainerLocation location = new("TestLocation");

        string? result = location.OriginalContainerType;

        Assert.Null(result);
    }

    [Fact]
    public void ChooseContainerType_MultiItemPlacementUsesMultiItemContainer()
    {
        TestContainerLocation location = new("MultiItem") { ForceDefaultContainer = false };
        Placement placement = location.Wrap();
        placement.Add(new DebugItem() { Name = "Item1" });
        placement.Add(new DebugItem() { Name = "Item2" });
        placement.LoadOnce();

        string container = location.ChooseBestContainerType();

        Assert.Equal(registry.DefaultMultiItemContainer.Name, container);
    }

    [Fact]
    public void ChooseContainerType_UnsupportedItemPreferredContainerFallsBack()
    {
        string unsupportedType = RegisterContainer("UnsupportedContainer");
        TestContainerLocation location = new("UnsupportedItemPreferred")
        {
            ForceDefaultContainer = false,
        };
        location.Disallow(unsupportedType);
        Placement placement = location.Wrap();
        placement.Add(new PreferredContainerItem("Item", unsupportedType));
        placement.LoadOnce();

        string container = location.ChooseBestContainerType();

        Assert.Equal(registry.DefaultSingleItemContainer.Name, container);
    }

    [Fact]
    public void ChooseContainerType_OriginalContainerLowPriorityNotSelected()
    {
        string lowPriorityType = RegisterContainer("LowPriorityContainer");
        TestContainerLocation location = new("LowPriority") { ForceDefaultContainer = false };
        Placement placement = location.Wrap();
        location.AddTag(
            new OriginalContainerTag { ContainerType = lowPriorityType, LowPriority = true }
        );
        placement.Add(new DebugItem() { Name = "Item" });
        placement.LoadOnce();

        string container = location.ChooseBestContainerType();

        Assert.Equal(registry.DefaultSingleItemContainer.Name, container);
    }

    [Fact]
    public void ChooseContainerType_OriginalContainerMissingCapabilitiesNotSelectedEvenIfPriority()
    {
        string originalType = RegisterContainer(
            "OriginalContainer",
            instantiate: true,
            capabilities: ContainerCapabilities.None
        );
        string fallbackType = RegisterContainer("Fallback");
        TestContainerLocation location = new("MissingCapabilities")
        {
            ForceDefaultContainer = false,
        };
        Placement placement = location.Wrap();
        location.AddTag(new OriginalContainerTag { ContainerType = originalType, Priority = true });
        location.AddTag(
            new RequiredCapabilitiesTag { RequestedCapabilities = ContainerCapabilities.PayCosts }
        );

        placement.Add(new PreferredContainerItem("Item", fallbackType));
        placement.LoadOnce();

        string container = location.ChooseBestContainerType();

        Assert.Equal(fallbackType, container);
    }

    [Fact]
    public void ChooseContainerType_NoValidContainersAvailable()
    {
        string unsupportedType1 = RegisterContainer("Unsupported1");
        string unsupportedType2 = RegisterContainer("Unsupported2");
        TestContainerLocation location = new("NoValid") { ForceDefaultContainer = false };
        location.Disallow(unsupportedType1);
        location.Disallow(unsupportedType2);
        Placement placement = location.Wrap();
        placement.Add(new PreferredContainerItem("Item1", unsupportedType1));
        placement.Add(new PreferredContainerItem("Item2", unsupportedType2));
        placement.LoadOnce();

        string container = location.ChooseBestContainerType();

        Assert.Equal(registry.DefaultSingleItemContainer.Name, container);
    }

    [Fact]
    public void ChooseContainerType_EmptyPlacementAndTags_ReturnsDefaultSingleItemContainer()
    {
        TestContainerLocation location = new("EmptyPlacementAndTags")
        {
            ForceDefaultContainer = false,
        };
        Placement placement = location.Wrap();
        placement.LoadOnce();

        string container = location.ChooseBestContainerType();

        Assert.Equal(registry.DefaultSingleItemContainer.Name, container);
    }

    private string RegisterContainer(
        string name,
        bool instantiate = true,
        bool modifyInPlace = true,
        uint capabilities = uint.MaxValue
    )
    {
        StubContainer container = new(name, instantiate, modifyInPlace, capabilities);
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

    private sealed class StubContainer(
        string name,
        bool instantiate,
        bool modifyInPlace,
        uint supportedCapabilities
    ) : Container
    {
        public override string Name { get; } = name;

        public override bool SupportsInstantiate => instantiate;

        public override bool SupportsModifyInPlace => modifyInPlace;

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
