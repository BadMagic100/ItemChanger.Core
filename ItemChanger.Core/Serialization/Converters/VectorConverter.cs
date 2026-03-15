using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ItemChanger.Serialization.Converters;

internal sealed class VectorConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Vector2) || objectType == typeof(Vector3);
    }

    public override object? ReadJson(
        JsonReader reader,
        Type objectType,
        object? existingValue,
        JsonSerializer serializer
    )
    {
        if (reader.TokenType == JsonToken.StartObject)
        {
            JObject obj = serializer.Deserialize<JObject>(reader)!;
            float x = obj["x"]?.Value<float>() ?? 0;
            float y = obj["y"]?.Value<float>() ?? 0;
            float z = obj["z"]?.Value<float>() ?? 0;
            if (objectType == typeof(Vector3))
            {
                return new Vector3(x, y, z);
            }
            else
            {
                return new Vector2(x, y);
            }
        }
        return null;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        float x,
            y,
            z = 0;
        if (value is Vector2 v2)
        {
            x = v2.x;
            y = v2.y;
        }
        else if (value is Vector3 v3)
        {
            x = v3.x;
            y = v3.y;
            z = v3.z;
        }
        else
        {
            throw new NotSupportedException();
        }

        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(x);
        writer.WritePropertyName("y");
        writer.WriteValue(y);
        writer.WritePropertyName("z");
        writer.WriteValue(z);
        writer.WriteEndObject();
    }
}
