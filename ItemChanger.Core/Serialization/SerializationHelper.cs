using System.IO;
using ItemChanger.Serialization.Converters;
using Newtonsoft.Json;

namespace ItemChanger.Serialization;

/// <summary>
/// Utility class containing the necessary serializer configuration to read and write ItemChanger objects to a stream as JSON.
/// </summary>
public static class SerializationHelper
{
    /// <summary>
    /// Shared serializer instance configured with all ItemChanger converters.
    /// </summary>
    public static JsonSerializer Serializer
    {
        get
        {
            if (field == null)
            {
                JsonSerializer js = new()
                {
                    DefaultValueHandling = DefaultValueHandling.Include,
                    Formatting = Formatting.Indented,
                    TypeNameHandling = TypeNameHandling.Auto,
                };

                js.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                js.Converters.Add(new VectorConverter());
                field = js;
            }
            return field;
        }
    }

    /// <summary>
    /// Utility to serialize an object with the necessary metadata for polymorphic deserialization.
    /// The stream will be closed after writing.
    /// </summary>
    /// <param name="stream">Stream to serialize to</param>
    /// <param name="o">The object to be serialized</param>
    public static void Serialize(Stream stream, object o)
    {
        using StreamWriter sw = new(stream);
        Serializer.Serialize(sw, o);
    }

    /// <summary>
    /// Utility to deserialize an object polymorphically for use in Finder (typically created from <see cref="Serialize(Stream, object)"/>).
    /// The stream will be closed after reading.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to</typeparam>
    /// <param name="stream">The stream to read from</param>
    public static T? DeserializeResource<T>(Stream stream)
    {
        using StreamReader sr = new(stream);
        return Serializer.Deserialize<T>(new JsonTextReader(sr));
    }
}
