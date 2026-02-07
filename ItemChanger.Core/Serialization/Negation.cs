using Newtonsoft.Json;

namespace ItemChanger.Serialization;

/// <summary>
/// A bool provider that negates the result of the wrapped bool.
/// </summary>
[method: JsonConstructor]
public class Negation(IValueProvider<bool> @bool) : IValueProvider<bool>
{
    /// <summary>
    /// Wrapped bool whose result is negated.
    /// </summary>
    public IValueProvider<bool> Bool => @bool;

    /// <inheritdoc/>
    [JsonIgnore]
    public bool Value => !Bool.Value;
}
