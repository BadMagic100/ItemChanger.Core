using ItemChanger.Modules;
using ItemChanger.Tests.Fixtures;

namespace ItemChanger.Tests.Modules;

[Collection(RequiresHostCollection.NAME)]
public class ModuleCollectionTests : IDisposable
{
    private readonly ITestOutputHelper output;
    private readonly TestHost host;
    private readonly ItemChangerProfile profile;

    public ModuleCollectionTests(ITestOutputHelper output)
    {
        this.output = output;
        host = new TestHost();
        profile = host.Profile;
    }

    public void Dispose()
    {
        profile.Dispose();
        host.Dispose();
    }

    [Fact]
    public void AddParameterless_CanAddMultipleNonSingleton()
    {
        profile.Modules.Add<A>();
        profile.Modules.Add<A>();
        profile.Modules.Add<B>();

        Assert.Equal(3, profile.Modules.Count);
        Assert.Empty(host.ErrorMessages);
    }

    [Fact]
    public void AddParameterless_CanAddMultipleNonSingleton_BoundGeneric()
    {
        profile.Modules.Add<A<int>>();
        profile.Modules.Add<A<int>>();
        profile.Modules.Add<B<int, string>>();

        Assert.Equal(3, profile.Modules.Count);
        Assert.Empty(host.ErrorMessages);
    }

    [Fact]
    public void AddParameterless_CanAddMultipleSingleton_SameType_DifferentBoundGeneric()
    {
        profile.Modules.Add<SA<int>>();
        profile.Modules.Add<SA<string>>();

        Assert.Equal(2, profile.Modules.Count);
        Assert.Empty(host.ErrorMessages);
    }

    [Fact]
    public void AddParameterless_CanAddMultipleSingleton_DerivedType_DifferentBoundGeneric()
    {
        profile.Modules.Add<SA<int>>();
        profile.Modules.Add<SB<string, string>>();

        Assert.Equal(2, profile.Modules.Count);
        Assert.Empty(host.ErrorMessages);
    }

    [Fact]
    public void AddParameterless_CanRemoveAndReAddSingleton()
    {
        Module m = profile.Modules.Add<SA>();
        profile.Modules.Remove(m);
        profile.Modules.Add<SA>();

        Assert.Equal(1, profile.Modules.Count);
        Assert.Empty(host.ErrorMessages);
    }

    [Fact]
    public void AddParameterless_CannotAddMultipleSingleton_SameType()
    {
        profile.Modules.Add<SA>();
        profile.Modules.Add<SA>();

        Assert.Equal(1, profile.Modules.Count);
        string? msg = Assert.Single(host.ErrorMessages);
        Assert.Equal(
            "Attempted to add another instance of singleton module typed ItemChanger.Tests.Modules.ModuleCollectionTests+SA",
            msg
        );
    }

    [Fact]
    public void AddParameterless_CannotAddMultipleSingleton_DerivedType()
    {
        profile.Modules.Add<SA>();
        profile.Modules.Add<SB>();

        Assert.Equal(1, profile.Modules.Count);
        string? msg = Assert.Single(host.ErrorMessages);
        Assert.Equal(
            "Attempted to add another instance of singleton module typed ItemChanger.Tests.Modules.ModuleCollectionTests+SA",
            msg
        );
    }

    [Fact]
    public void AddParameterless_CannotAddMultipleSingleton_SameType_BoundGeneric()
    {
        profile.Modules.Add<SA<int>>();
        profile.Modules.Add<SA<int>>();

        Assert.Equal(1, profile.Modules.Count);
        string? msg = Assert.Single(host.ErrorMessages);
        Assert.Equal(
            "Attempted to add another instance of singleton module typed ItemChanger.Tests.Modules.ModuleCollectionTests+SA`1[System.Int32]",
            msg
        );
    }

    [Fact]
    public void AddParameterless_CannotAddMultipleSingleton_DerivedType_BoundGeneric()
    {
        profile.Modules.Add<SA<int>>();
        profile.Modules.Add<SB<int, string>>();

        Assert.Equal(1, profile.Modules.Count);
        string? msg = Assert.Single(host.ErrorMessages);
        Assert.Equal(
            "Attempted to add another instance of singleton module typed ItemChanger.Tests.Modules.ModuleCollectionTests+SA`1[System.Int32]",
            msg
        );
    }

    [Fact]
    public void AddInstance_CanAddMultipleNonSingleton()
    {
        profile.Modules.Add(new A());
        profile.Modules.Add(new A());
        profile.Modules.Add(new B());

        Assert.Equal(3, profile.Modules.Count);
        Assert.Empty(host.ErrorMessages);
    }

    [Fact]
    public void AddInstance_CanAddMultipleNonSingleton_BoundGeneric()
    {
        profile.Modules.Add(new A<int>());
        profile.Modules.Add(new A<int>());
        profile.Modules.Add(new B<int, string>());

        Assert.Equal(3, profile.Modules.Count);
        Assert.Empty(host.ErrorMessages);
    }

    [Fact]
    public void AddInstance_CanAddMultipleSingleton_SameType_DifferentBoundGeneric()
    {
        profile.Modules.Add(new SA<int>());
        profile.Modules.Add(new SA<string>());

        Assert.Equal(2, profile.Modules.Count);
        Assert.Empty(host.ErrorMessages);
    }

    [Fact]
    public void AddInstance_CanAddMultipleSingleton_DerivedType_DifferentBoundGeneric()
    {
        profile.Modules.Add(new SA<int>());
        profile.Modules.Add(new SB<string, string>());

        Assert.Equal(2, profile.Modules.Count);
        Assert.Empty(host.ErrorMessages);
    }

    [Fact]
    public void AddInstance_CanRemoveAndReAddSingleton()
    {
        Module m = profile.Modules.Add(new SA());
        profile.Modules.Remove(m);
        profile.Modules.Add(new SA());

        Assert.Equal(1, profile.Modules.Count);
        Assert.Empty(host.ErrorMessages);
    }

    [Fact]
    public void AddInstance_CannotAddMultipleSingleton_SameType()
    {
        profile.Modules.Add(new SA());
        profile.Modules.Add(new SA());

        Assert.Equal(1, profile.Modules.Count);
        string? msg = Assert.Single(host.ErrorMessages);
        Assert.Equal(
            "Attempted to add another instance of singleton module typed ItemChanger.Tests.Modules.ModuleCollectionTests+SA",
            msg
        );
    }

    [Fact]
    public void AddInstance_CannotAddMultipleSingleton_DerivedType()
    {
        profile.Modules.Add(new SA());
        profile.Modules.Add(new SB());

        Assert.Equal(1, profile.Modules.Count);
        string? msg = Assert.Single(host.ErrorMessages);
        Assert.Equal(
            "Attempted to add another instance of singleton module typed ItemChanger.Tests.Modules.ModuleCollectionTests+SA",
            msg
        );
    }

    [Fact]
    public void AddInstance_CannotAddMultipleSingleton_SameType_BoundGeneric()
    {
        profile.Modules.Add(new SA<int>());
        profile.Modules.Add(new SA<int>());

        Assert.Equal(1, profile.Modules.Count);
        string? msg = Assert.Single(host.ErrorMessages);
        Assert.Equal(
            "Attempted to add another instance of singleton module typed ItemChanger.Tests.Modules.ModuleCollectionTests+SA`1[System.Int32]",
            msg
        );
    }

    [Fact]
    public void AddInstance_CannotAddMultipleSingleton_DerivedType_BoundGeneric()
    {
        profile.Modules.Add(new SA<int>());
        profile.Modules.Add(new SB<int, string>());

        Assert.Equal(1, profile.Modules.Count);
        string? msg = Assert.Single(host.ErrorMessages);
        Assert.Equal(
            "Attempted to add another instance of singleton module typed ItemChanger.Tests.Modules.ModuleCollectionTests+SA`1[System.Int32]",
            msg
        );
    }

    [Fact]
    public void AddType_CanAddMultipleNonSingleton()
    {
        profile.Modules.Add(typeof(A));
        profile.Modules.Add(typeof(A));
        profile.Modules.Add(typeof(B));

        Assert.Equal(3, profile.Modules.Count);
        Assert.Empty(host.ErrorMessages);
    }

    [Fact]
    public void AddType_CanAddMultipleNonSingleton_BoundGeneric()
    {
        profile.Modules.Add(typeof(A<int>));
        profile.Modules.Add(typeof(A<int>));
        profile.Modules.Add(typeof(B<int, string>));

        Assert.Equal(3, profile.Modules.Count);
        Assert.Empty(host.ErrorMessages);
    }

    [Fact]
    public void AddType_CanAddMultipleSingleton_SameType_DifferentBoundGeneric()
    {
        profile.Modules.Add(typeof(SA<int>));
        profile.Modules.Add(typeof(SA<string>));

        Assert.Equal(2, profile.Modules.Count);
        Assert.Empty(host.ErrorMessages);
    }

    [Fact]
    public void AddType_CanAddMultipleSingleton_DerivedType_DifferentBoundGeneric()
    {
        profile.Modules.Add(typeof(SA<int>));
        profile.Modules.Add(typeof(SB<string, string>));

        Assert.Equal(2, profile.Modules.Count);
        Assert.Empty(host.ErrorMessages);
    }

    [Fact]
    public void AddType_CanRemoveAndReAddSingleton()
    {
        Module m = profile.Modules.Add(typeof(SA));
        profile.Modules.Remove(m);
        profile.Modules.Add(typeof(SA));

        Assert.Equal(1, profile.Modules.Count);
        Assert.Empty(host.ErrorMessages);
    }

    [Fact]
    public void AddType_CannotAddMultipleSingleton_SameType()
    {
        profile.Modules.Add(typeof(SA));
        profile.Modules.Add(typeof(SA));

        Assert.Equal(1, profile.Modules.Count);
        string? msg = Assert.Single(host.ErrorMessages);
        Assert.Equal(
            "Attempted to add another instance of singleton module typed ItemChanger.Tests.Modules.ModuleCollectionTests+SA",
            msg
        );
    }

    [Fact]
    public void AddType_CannotAddMultipleSingleton_DerivedType()
    {
        profile.Modules.Add(typeof(SA));
        profile.Modules.Add(typeof(SB));

        Assert.Equal(1, profile.Modules.Count);
        string? msg = Assert.Single(host.ErrorMessages);
        Assert.Equal(
            "Attempted to add another instance of singleton module typed ItemChanger.Tests.Modules.ModuleCollectionTests+SA",
            msg
        );
    }

    [Fact]
    public void AddType_CannotAddMultipleSingleton_SameType_BoundGeneric()
    {
        profile.Modules.Add(typeof(SA<int>));
        profile.Modules.Add(typeof(SA<int>));

        Assert.Equal(1, profile.Modules.Count);
        string? msg = Assert.Single(host.ErrorMessages);
        Assert.Equal(
            "Attempted to add another instance of singleton module typed ItemChanger.Tests.Modules.ModuleCollectionTests+SA`1[System.Int32]",
            msg
        );
    }

    [Fact]
    public void AddType_CannotAddMultipleSingleton_DerivedType_BoundGeneric()
    {
        profile.Modules.Add(typeof(SA<int>));
        profile.Modules.Add(typeof(SB<int, string>));

        Assert.Equal(1, profile.Modules.Count);
        string? msg = Assert.Single(host.ErrorMessages);
        Assert.Equal(
            "Attempted to add another instance of singleton module typed ItemChanger.Tests.Modules.ModuleCollectionTests+SA`1[System.Int32]",
            msg
        );
    }

    // below tests are a regression test against https://github.com/BadMagic100/ItemChanger.Core/issues/28#issuecomment-3904875032
    [Fact]
    public void GetOrAddParameterless_DoesNotErrorOnOverlyBroadType()
    {
        profile.Modules.Add<A>();
        profile.Modules.GetOrAdd<SA>();

        Assert.Equal(2, profile.Modules.Count);
        Assert.Empty(host.ErrorMessages);
    }

    [Fact]
    public void GetOrAddInstance_DoesNotErrorOnOverlyBroadType()
    {
        profile.Modules.Add(new A());
        profile.Modules.GetOrAdd(new SA());

        Assert.Equal(2, profile.Modules.Count);
        Assert.Empty(host.ErrorMessages);
    }

    [Fact]
    public void GetOrAddType_DoesNotErrorOnOverlyBroadType()
    {
        profile.Modules.Add(typeof(A));
        profile.Modules.GetOrAdd(typeof(SA));

        Assert.Equal(2, profile.Modules.Count);
        Assert.Empty(host.ErrorMessages);
    }

    private class A : Module
    {
        protected override void DoLoad() { }

        protected override void DoUnload() { }
    }

    private class B : A
    {
        protected override void DoLoad() { }

        protected override void DoUnload() { }
    }

    private class A<T> : Module
    {
        protected override void DoLoad() { }

        protected override void DoUnload() { }
    }

    private class B<T1, T2> : A<T1>
    {
        protected override void DoLoad() { }

        protected override void DoUnload() { }
    }

    [SingletonModule]
    private class SA : Module
    {
        protected override void DoLoad() { }

        protected override void DoUnload() { }
    }

    private class SB : SA
    {
        protected override void DoLoad() { }

        protected override void DoUnload() { }
    }

    [SingletonModule]
    private class SA<T> : Module
    {
        protected override void DoLoad() { }

        protected override void DoUnload() { }
    }

    private class SB<T1, T2> : SA<T1>
    {
        protected override void DoLoad() { }

        protected override void DoUnload() { }
    }
}
