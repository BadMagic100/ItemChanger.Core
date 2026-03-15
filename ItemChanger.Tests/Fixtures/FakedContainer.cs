using ItemChanger.Containers;

namespace ItemChanger.Tests.Fixtures;

internal class FakedContainer : Container
{
    public override string Name => "Fake";

    public override uint SupportedCapabilities => uint.MaxValue;

    protected override void DoLoad() { }

    protected override void DoUnload() { }
}
