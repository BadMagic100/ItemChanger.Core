using System.Text;
using ItemChanger.Serialization;
using Snapshooter.Xunit3;

namespace ItemChanger.Tests.Serialization.Converters;

public class NonStringDictionaryAsArrayResolverTests
{
    private enum ChoiceKeys
    {
        Foo,
        Bar,
    }

    private class Demo
    {
        public required IReadOnlyDictionary<string, int> IReadonlyStringDict { get; set; }

        public required IReadOnlyDictionary<ChoiceKeys, int> IReadonlyEnumDict { get; set; }

        public required IDictionary<string, int> IStringDict { get; set; }

        public required IDictionary<ChoiceKeys, int> IEnumDict { get; set; }

        public required Dictionary<string, int> StringDict { get; set; }

        public required Dictionary<ChoiceKeys, int> EnumDict { get; set; }

        public static readonly Demo DemoInstance = new()
        {
            IReadonlyStringDict = new Dictionary<string, int>() { ["foo"] = 1, ["bar"] = 2 },
            IReadonlyEnumDict = new Dictionary<ChoiceKeys, int>()
            {
                [ChoiceKeys.Foo] = 1,
                [ChoiceKeys.Bar] = 2,
            },
            IStringDict = new Dictionary<string, int>() { ["foo"] = 1, ["bar"] = 2 },
            IEnumDict = new Dictionary<ChoiceKeys, int>()
            {
                [ChoiceKeys.Foo] = 1,
                [ChoiceKeys.Bar] = 2,
            },
            StringDict = new Dictionary<string, int>() { ["foo"] = 1, ["bar"] = 2 },
            EnumDict = new Dictionary<ChoiceKeys, int>()
            {
                [ChoiceKeys.Foo] = 1,
                [ChoiceKeys.Bar] = 2,
            },
        };
    }

    [Fact]
    public void FullWashSerDeIsConsistent()
    {
        Demo demo1 = Demo.DemoInstance;
        using MemoryStream write1 = new();
        SerializationHelper.Serialize(write1, demo1);
        byte[] firstSnapshot = write1.ToArray();

        using MemoryStream read = new(firstSnapshot);
        Demo? demo2 = SerializationHelper.Deserialize<Demo>(read);

        using MemoryStream write2 = new();
        SerializationHelper.Serialize(write2, demo2);
        byte[] secondSnapshot = write2.ToArray();

        // they'd better be the same!
        string firstJson = Encoding.UTF8.GetString(firstSnapshot);
        string secondJson = Encoding.UTF8.GetString(secondSnapshot);

        Assert.Equal(firstJson, secondJson);
    }

    [Fact]
    public void SerializationMatchesSnapshot()
    {
        using MemoryStream ms = new();
        SerializationHelper.Serialize(ms, Demo.DemoInstance);

        string json = Encoding.UTF8.GetString(ms.ToArray());
        Snapshot.Match(json);
    }
}
