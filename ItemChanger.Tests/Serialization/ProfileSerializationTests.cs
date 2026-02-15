using System.Text;
using ItemChanger.Items;
using ItemChanger.Locations;
using ItemChanger.Modules;
using ItemChanger.Placements;
using ItemChanger.Serialization;
using ItemChanger.Tags;
using ItemChanger.Tests.Fixtures;
using Newtonsoft.Json.Linq;
using Snapshooter.Xunit3;

namespace ItemChanger.Tests.Serialization;

[Collection(RequiresHostCollection.NAME)]
public class ProfileSerializationTests : IDisposable
{
    private readonly TestHost host;
    private ItemChangerProfile profile;

    public ProfileSerializationTests()
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
    public void FullWashSerDeIsConsistent()
    {
        profile.Modules.Add(new InteropModule() { Message = "foo" });
        profile.Modules.Add(new InteropModule() { Message = "bar" });

        Item a = CreateTaggedItem("A");
        Item b = CreateTaggedItem("B");
        Item c = CreateTaggedItem("C");
        Placement p = CreatePlacement([a, b, c]);
        profile.AddPlacement(p);

        // new game
        host.RunStartNewLifecycle();

        // first save
        using MemoryStream ms = new();
        host.RunLeaveLifecycle();
        profile.ToStream(ms);
        profile.Dispose();
        byte[] firstSaveSnapshot = ms.ToArray();

        // load from file and continue game
        using MemoryStream ms2 = new(firstSaveSnapshot);
        profile = ItemChangerProfile.FromStream(host, ms2);
        host.RunContinueLifecycle();

        // second save
        using MemoryStream ms3 = new();
        host.RunLeaveLifecycle();
        profile.ToStream(ms3);
        profile.Dispose();
        byte[] secondSaveSnapshot = ms3.ToArray();

        // they'd better be the same!
        string firstJson = Encoding.UTF8.GetString(firstSaveSnapshot);
        string secondJson = Encoding.UTF8.GetString(secondSaveSnapshot);

        Assert.Equal(firstJson, secondJson);
    }

    [Fact]
    public void NonTrivialSerializationMatchesSnapshot()
    {
        profile.Modules.Add(new InteropModule() { Message = "foo" });
        profile.Modules.Add(new InteropModule() { Message = "bar" });

        Item a = CreateTaggedItem("A");
        Item b = CreateTaggedItem("B");
        Item c = CreateTaggedItem("C");
        Placement p = CreatePlacement(a, b, c);
        profile.AddPlacement(p);

        // new game
        host.RunStartNewLifecycle();

        // save
        using MemoryStream ms = new();
        host.RunLeaveLifecycle();
        profile.ToStream(ms);
        profile.Dispose();
        byte[] snapshot = ms.ToArray();
        string snapshotJson = Encoding.UTF8.GetString(snapshot);

        Snapshot.Match(snapshotJson);
    }

    [Fact]
    public void InvalidModulesAndTagSerializationMatchesSnapshot()
    {
        Item a = CreateTaggedItem("A");
        CostTag ct = new() { Cost = new DollarCost() { Amount = 1 } };
        Exception err = new TypeLoadException();
        JToken tok = JToken.FromObject(ct, SerializationHelper.Serializer);
        InvalidTag it = new() { JSON = tok, DeserializationError = err };
        a.AddTag(it);
        Placement p = CreatePlacement(a);

        InteropModule mod = new() { Message = "foo" };
        JToken tok2 = JToken.FromObject(mod, SerializationHelper.Serializer);
        InvalidModule im = new() { JSON = tok2, DeserializationError = err };

        profile.AddPlacement(p);
        profile.Modules.Add(im);

        // new game
        host.RunStartNewLifecycle();

        // save
        using MemoryStream ms = new();
        host.RunLeaveLifecycle();
        profile.ToStream(ms);
        profile.Dispose();
        byte[] snapshot = ms.ToArray();
        string snapshotJson = Encoding.UTF8.GetString(snapshot);

        // note - in normal usage a serialization failure would be the cause here,
        // and the loaded JToken would have type naming tokens. We don't have them in this snapshot.
        Snapshot.Match(snapshotJson);
    }

    private Item CreateTaggedItem(string name)
    {
        Item i = new NullItem { Name = name };
        i.AddTag(new InteropTag { Message = "test" });
        return i;
    }

    private Placement CreatePlacement(params Item[] items)
    {
        return new AutoPlacement("Test placement")
        {
            Location = new EmptyLocation { Name = "Test location" },
        }.Add(items);
    }
}
