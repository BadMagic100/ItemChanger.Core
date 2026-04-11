using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;

namespace ItemChanger.Serialization.Converters;

internal sealed class NonStringDictionaryAsArrayResolver : DefaultContractResolver
{
    protected override JsonContract CreateContract(Type objectType)
    {
        if (
            IsDictionaryWithNonStringKey(objectType)
            || objectType.GetInterfaces().Any(IsDictionaryWithNonStringKey)
        )
        {
            return base.CreateArrayContract(objectType);
        }

        return base.CreateContract(objectType);
    }

    private bool IsDictionaryWithNonStringKey(Type type)
    {
        if (!type.IsGenericType)
        {
            return false;
        }

        Type def = type.GetGenericTypeDefinition();
        return (def == typeof(IDictionary<,>) || def == typeof(IReadOnlyDictionary<,>))
            && type.GenericTypeArguments[0] != typeof(string);
    }
}
