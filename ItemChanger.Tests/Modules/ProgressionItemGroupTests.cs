using ItemChanger.Events.Args;
using ItemChanger.Items;
using ItemChanger.Locations;
using ItemChanger.Modules;
using ItemChanger.Placements;
using ItemChanger.Tags;
using ItemChanger.Tests.Fixtures;

namespace ItemChanger.Tests.Modules;

[Collection(RequiresHostCollection.NAME)]
public class ProgressionItemGroupTests : IDisposable
{
    private readonly ITestOutputHelper output;
    private readonly TestHost host;
    private readonly ItemChangerProfile profile;

    public ProgressionItemGroupTests(ITestOutputHelper output)
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

    // A model in which there are 3 items: L,R,S. (from Hollow Knight, Left_Mothwing_Cloak, Right_Mothwing_Cloak, Split_Shade_Cloak).
    // S cannot be given before one of L or R. If S is collected first, it must be replaced. There are no other constraints.
    // Say we are left-biased: S is replaced by L if collected first.
    // Then the condition that items are commutative determines what the items give when collected in any order, in the table below.
    // We also test a few combinations with duplicates of progression-maximal items.
    [Theory]
    [InlineData((string[])["L", "R", "S"], (string[])["L", "R", "S"])]
    [InlineData((string[])["L", "S", "R"], (string[])["L", "S", "R"])]
    [InlineData((string[])["S", "L", "R"], (string[])["L", "S", "R"])]
    [InlineData((string[])["S", "R", "L"], (string[])["L", "R", "S"])]
    [InlineData((string[])["R", "L", "S"], (string[])["R", "L", "S"])]
    [InlineData((string[])["R", "S", "L"], (string[])["R", "L", "S"])]
    [InlineData((string[])["S", "S", "L", "R"], (string[])["L", "S", "S", "R"])]
    [InlineData((string[])["S", "S", "R", "L"], (string[])["L", "S", "R", "S"])]
    [InlineData((string[])["S", "L", "S", "R"], (string[])["L", "S", "S", "R"])]
    [InlineData((string[])["S", "R", "S", "L"], (string[])["L", "R", "S", "S"])]
    public void LeftBiasedSplitCloakProgressionTest(string[] input, string[] expectedOutput)
    {
        // item/placement setup
        DefineTaggedItem("L");
        DefineTaggedItem("R");
        DefineTaggedItem("S");
        Placement p = CreatePlacement(input);
        profile.AddPlacement(p);

        profile.Modules.Add(
            new ProgressiveItemGroupModule
            {
                GroupID = "test",
                OrderedMemberList = ["L", "R", "S"],
                OrderedTransitivePredecessorsLookup = new Dictionary<string, List<string>>
                {
                    ["L"] = [],
                    ["R"] = [],
                    ["S"] = ["L"],
                },
            }
        );

        // prepare to monitor item order
        List<string> resultOrder = [];
        void AddToResult(ReadOnlyGiveEventArgs args) => resultOrder.Add(args.Item.Name);
        foreach (Item i in p.Items)
        {
            i.AfterGive += AddToResult;
        }
        // start IC w/o errors
        Assert.True(host.RunStartNewLifecycle());
        // give items
        GiveInfo gi = new();
        p.GiveAll(gi);
        // assert
        Assert.Equal(expectedOutput, resultOrder);
    }

    // A few samples of the same LeftBiasedSplitCloak setup, with some of the items defined only after profile load.
    [Theory]
    [InlineData((string[])["L", "R"], (string[])["S"], (string[])["L", "R", "S"])]
    [InlineData((string[])["S", "R"], (string[])["L"], (string[])["L", "R", "S"])]
    [InlineData((string[])["R"], (string[])["S", "L"], (string[])["R", "L", "S"])]
    public void LateItemLoadProgressionTest(
        string[] firstInput,
        string[] secondInput,
        string[] expectedOutput
    )
    {
        // item/placement setup
        DefineTaggedItem("L");
        DefineTaggedItem("R");
        DefineTaggedItem("S");
        Placement p1 = CreatePlacement(firstInput);
        profile.AddPlacement(p1);

        profile.Modules.Add(
            new ProgressiveItemGroupModule
            {
                GroupID = "test",
                OrderedMemberList = ["L", "R", "S"],
                OrderedTransitivePredecessorsLookup = new Dictionary<string, List<string>>
                {
                    ["L"] = [],
                    ["R"] = [],
                    ["S"] = ["L"],
                },
            }
        );
        // prepare to monitor item order
        List<string> resultOrder = [];
        void AddToResult(ReadOnlyGiveEventArgs args) => resultOrder.Add(args.Item.Name);
        foreach (Item i in p1.Items)
        {
            i.AfterGive += AddToResult;
        }
        // start IC w/o errors
        Assert.True(host.RunStartNewLifecycle());
        // give items
        GiveInfo gi = new();
        p1.GiveAll(gi);
        // define second placement
        Placement p2 = CreatePlacement(secondInput);
        profile.AddPlacement(p2);
        foreach (Item i in p2.Items)
        {
            i.AfterGive += AddToResult;
        }
        // give items
        gi = new();
        p2.GiveAll(gi);
        // assert
        Assert.Equal(expectedOutput, resultOrder);
    }

    // A model in which there are 3 items: N,C,E. (from Silksong, Needolin, Beastling's Call, Elegy of the Deep).
    // N must be given before C or E. There are no other constraints.
    // Say we are Call-biased. If C and E are the first two items collected (in either order), we give Needolin and Call.
    // Then the condition that items are commutative determines what the items give when collected in any order, in the table below.
    // We also test a few combinations with duplicates of progression-maximal items.
    [Theory]
    [InlineData((string[])["N", "C", "E"], (string[])["N", "C", "E"])]
    [InlineData((string[])["N", "E", "C"], (string[])["N", "E", "C"])]
    [InlineData((string[])["C", "N", "E"], (string[])["N", "C", "E"])]
    [InlineData((string[])["C", "E", "N"], (string[])["N", "C", "E"])]
    [InlineData((string[])["E", "N", "C"], (string[])["N", "E", "C"])]
    [InlineData((string[])["E", "C", "N"], (string[])["N", "C", "E"])]
    [InlineData((string[])["E", "E", "C", "N"], (string[])["N", "E", "C", "E"])]
    [InlineData((string[])["C", "C", "E", "N"], (string[])["N", "C", "C", "E"])]
    [InlineData((string[])["E", "C", "E", "N"], (string[])["N", "C", "E", "E"])]
    [InlineData((string[])["C", "E", "C", "N"], (string[])["N", "C", "C", "E"])]
    [InlineData((string[])["E", "E", "N", "C"], (string[])["N", "E", "E", "C"])]
    [InlineData((string[])["C", "C", "N", "E"], (string[])["N", "C", "C", "E"])]
    public void NeedolinProgressionTest(string[] input, string[] expectedOutput)
    {
        // item/placement setup
        DefineTaggedItem("N");
        DefineTaggedItem("C");
        DefineTaggedItem("E");
        Placement p = CreatePlacement(input);
        profile.AddPlacement(p);

        profile.Modules.Add(
            new ProgressiveItemGroupModule
            {
                GroupID = "test",
                OrderedMemberList = ["N", "E", "C"],
                OrderedTransitivePredecessorsLookup = new Dictionary<string, List<string>>
                {
                    ["N"] = [],
                    ["C"] = ["N"],
                    ["E"] = ["N"],
                },
            }
        );
        // prepare to monitor item order
        List<string> resultOrder = [];
        void AddToResult(ReadOnlyGiveEventArgs args) => resultOrder.Add(args.Item.Name);
        foreach (Item i in p.Items)
        {
            i.AfterGive += AddToResult;
        }
        // start IC w/o errors
        Assert.True(host.RunStartNewLifecycle());
        // give items
        GiveInfo gi = new();
        p.GiveAll(gi);
        // assert
        Assert.Equal(expectedOutput, resultOrder);
    }

    // an abstract model with four items: M,S,H1,H2.
    // In this model, the first two items must be given before either H1 or H2 can be given. There are no other constraints.
    // We give H1 and H2 different preferences for how to be replaced; H1 prefers to be replaced by M, and H2 prefers to be replaced by S.
    // We test a sampling of permutations of the four items, along with a scenario where duplicates of H1 and H2 are included.
    [Theory]
    [InlineData((string[])["M", "S", "H1", "H2"], (string[])["M", "S", "H1", "H2"])]
    [InlineData((string[])["H1", "S", "M", "H2"], (string[])["M", "S", "H1", "H2"])]
    [InlineData((string[])["H2", "H1", "S", "M"], (string[])["S", "M", "H2", "H1"])]
    [InlineData((string[])["H1", "H2", "S", "M"], (string[])["M", "S", "H2", "H1"])]
    [InlineData((string[])["H1", "H2", "M", "S"], (string[])["M", "S", "H2", "H1"])]
    [InlineData(
        (string[])["H2", "H2", "H1", "H1", "M", "S", "H1", "H2"],
        (string[])["S", "M", "H2", "H2", "H1", "H1", "H1", "H2"]
    )]
    // to explain this last case, recall that items are sorted according to member order before being replaced by the module. So the output is obtained by:
    // H2 -> S; new item is S
    // H2 -> S, H2 -> M; new item is M
    // H1 -> M, H2 -> S, H2 -> H2; new item is H2 (note that H1 was collected last, but is sorted before H2)
    // H1 -> M, H1 -> S, H2 -> H2, H2 -> H2 (dupe); new item is H2 (dupe)
    // M -> M, H1 -> S, H1 -> H1, H2 -> H2, H2 -> H2 (dupe); new item is H1
    // M -> M, S -> S, H1 -> H1, H1 -> H1 (dupe), H2 -> H2, H2 -> H2 (dupe); new item is H1 (dupe)
    // M -> M, S -> S, H1 -> H1, H1 -> H1 (dupe), H1 -> H1 (dupe), H2 -> H2, H2 -> H2 (dupe); new item is H1 (dupe)
    // M -> M, S -> S, H1 -> H1, H1 -> H1 (dupe), H1 -> H1 (dupe), H2 -> H2, H2 -> H2 (dupe), H2 -> H2 (dupe); new item is H2 (dupe)
    public void FourItemProgressionTest(string[] input, string[] expectedOutput)
    {
        // item/placement setup
        DefineTaggedItem("M");
        DefineTaggedItem("S");
        DefineTaggedItem("H1");
        DefineTaggedItem("H2");
        Placement p = CreatePlacement(input);
        profile.AddPlacement(p);

        profile.Modules.Add(
            new ProgressiveItemGroupModule
            {
                GroupID = "test",
                OrderedMemberList = ["M", "S", "H1", "H2"],
                OrderedTransitivePredecessorsLookup = new Dictionary<string, List<string>>
                {
                    ["M"] = [],
                    ["S"] = [],
                    ["H1"] = ["M", "S"],
                    ["H2"] = ["S", "M"],
                },
            }
        );
        // prepare to monitor item order
        List<string> resultOrder = [];
        void AddToResult(ReadOnlyGiveEventArgs args) => resultOrder.Add(args.Item.Name);
        foreach (Item i in p.Items)
        {
            i.AfterGive += AddToResult;
        }
        // start IC w/o errors
        Assert.True(host.RunStartNewLifecycle());
        // give items
        GiveInfo gi = new();
        p.GiveAll(gi);
        // assert
        Assert.Equal(expectedOutput, resultOrder);
    }

    [Fact]
    public void MissingMemberTest1()
    {
        profile.Modules.Add(
            new ProgressiveItemGroupModule
            {
                GroupID = "test",
                OrderedMemberList = ["X", "Y", "Z"],
                OrderedTransitivePredecessorsLookup = new Dictionary<string, List<string>>
                {
                    ["X"] = [],
                    ["Y"] = ["X"],
                },
            }
        );
        // start IC w/ errors
        Assert.False(host.RunStartNewLifecycle());
        // retrieve error message
        string err = Assert.Single(host.ErrorMessages)!;
        output.WriteLine(err);
        Assert.StartsWith(
            "Error initializing module ProgressiveItemGroupModule:\n"
                + "System.InvalidOperationException: "
                + "Item Z appears in data of ProgressiveItemGroupModule with GroupID test, "
                + "but item is not both an entry of the member list and a key of the predecessor lookup.",
            err
        );
    }

    [Fact]
    public void MissingMemberTest2()
    {
        profile.Modules.Add(
            new ProgressiveItemGroupModule
            {
                GroupID = "test",
                OrderedMemberList = ["X", "Y"],
                OrderedTransitivePredecessorsLookup = new Dictionary<string, List<string>>
                {
                    ["X"] = [],
                    ["Y"] = ["X"],
                    ["Z"] = ["X"],
                },
            }
        );
        // start IC w/ errors
        Assert.False(host.RunStartNewLifecycle());
        // retrieve error message
        string err = Assert.Single(host.ErrorMessages)!;
        output.WriteLine(err);
        Assert.StartsWith(
            "Error initializing module ProgressiveItemGroupModule:\n"
                + "System.InvalidOperationException: "
                + "Item Z appears in data of ProgressiveItemGroupModule with GroupID test, "
                + "but item is not both an entry of the member list and a key of the predecessor lookup.",
            err
        );
    }

    [Fact]
    public void MissingMemberTest3()
    {
        profile.Modules.Add(
            new ProgressiveItemGroupModule
            {
                GroupID = "test",
                OrderedMemberList = ["X", "Y"],
                OrderedTransitivePredecessorsLookup = new Dictionary<string, List<string>>
                {
                    ["X"] = ["Z"],
                    ["Y"] = ["X"],
                },
            }
        );
        // start IC w/ errors
        Assert.False(host.RunStartNewLifecycle());
        // retrieve error message
        string err = Assert.Single(host.ErrorMessages)!;
        output.WriteLine(err);
        Assert.StartsWith(
            "Error initializing module ProgressiveItemGroupModule:\n"
                + "System.InvalidOperationException: "
                + "Item Z appears in data of ProgressiveItemGroupModule with GroupID test, "
                + "but item is not both an entry of the member list and a key of the predecessor lookup.",
            err
        );
    }

    [Fact]
    public void TransitivityTest()
    {
        profile.Modules.Add(
            new ProgressiveItemGroupModule
            {
                GroupID = "test",
                OrderedMemberList = ["X", "Y", "Z"],
                OrderedTransitivePredecessorsLookup = new Dictionary<string, List<string>>
                {
                    ["X"] = [],
                    ["Y"] = ["X"],
                    ["Z"] = ["Y"],
                },
            }
        );
        // start IC w/ errors
        Assert.False(host.RunStartNewLifecycle());
        // retrieve error message
        string err = Assert.Single(host.ErrorMessages)!;
        output.WriteLine(err);
        Assert.StartsWith(
            "Error initializing module ProgressiveItemGroupModule:\n"
                + "System.InvalidOperationException: "
                + "ProgressiveItemGroupTag for Z with GroupID test is missing the transitive predecessor X of Y.",
            err
        );
    }

    [Fact]
    public void IrreflexivityTest()
    {
        profile.Modules.Add(
            new ProgressiveItemGroupModule
            {
                GroupID = "test",
                OrderedMemberList = ["X"],
                OrderedTransitivePredecessorsLookup = new Dictionary<string, List<string>>
                {
                    ["X"] = ["X"],
                },
            }
        );
        // start IC w/ errors
        Assert.False(host.RunStartNewLifecycle());
        // retrieve error message
        string err = Assert.Single(host.ErrorMessages)!;
        output.WriteLine(err);
        Assert.StartsWith(
            "Error initializing module ProgressiveItemGroupModule:\n"
                + "System.InvalidOperationException: "
                + "ProgressiveItemGroupTag for X with GroupID test declares X as its own predecessor.",
            err
        );
    }

    [Fact]
    public void OrderConsistencyTest()
    {
        profile.Modules.Add(
            new ProgressiveItemGroupModule
            {
                GroupID = "test",
                OrderedMemberList = ["Y", "X"],
                OrderedTransitivePredecessorsLookup = new Dictionary<string, List<string>>
                {
                    ["X"] = [],
                    ["Y"] = ["X"],
                },
            }
        );
        // start IC w/ errors
        Assert.False(host.RunStartNewLifecycle());
        // retrieve error message
        string err = Assert.Single(host.ErrorMessages)!;
        output.WriteLine(err);
        Assert.StartsWith(
            "Error initializing module ProgressiveItemGroupModule:\n"
                + "System.InvalidOperationException: "
                + "Y is declared as a predecessor of X, but Y occurs after X in the OrderedMemberList for ProgressiveItemGroupModule with GroupID test.",
            err
        );
    }

    [Fact]
    public void UnexpectedMemberTest()
    {
        // item/placement setup
        DefineTaggedItem("X");
        DefineTaggedItem("Y");
        DefineTaggedItem("Z");
        Placement p = CreatePlacement(["X", "Y", "Z"]);
        profile.AddPlacement(p);

        profile.Modules.Add(
            new ProgressiveItemGroupModule
            {
                GroupID = "test",
                OrderedMemberList = ["X", "Y"],
                OrderedTransitivePredecessorsLookup = new Dictionary<string, List<string>>
                {
                    ["X"] = [],
                    ["Y"] = ["X"],
                },
            }
        );
        // start IC w/ errors
        Assert.False(host.RunStartNewLifecycle());
        // retrieve error message
        string err = Assert.Single(host.ErrorMessages)!;
        output.WriteLine(err);
        Assert.StartsWith(
            "Error loading ProgressiveItemGroupTag:\n"
                + "System.InvalidOperationException: "
                + "Item Z tagged with ProgressiveItemGroupTag with GroupID test was not declared on the module.",
            err
        );
    }

    private void DefineTaggedItem(string name)
    {
        Item i = new NullItem { Name = name };
        i.AddTag(new ProgressiveItemGroupTag { GroupID = "test" });
        ItemChangerHost.Singleton.Finder.DefineItem(i, overwrite: true);
    }

    private Placement CreatePlacement(IEnumerable<string> itemNames)
    {
        return new AutoPlacement("Test placement")
        {
            Location = new EmptyLocation { Name = "Test location" },
        }.Add(itemNames.Select(ItemChangerHost.Singleton.Finder.GetItem)!);
    }
}
