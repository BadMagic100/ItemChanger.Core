using Newtonsoft.Json;

namespace ItemChanger.Serialization;

/// <summary>
/// A bool provider that negates the result of the wrapped bool.
/// </summary>
public class Negation : IValueProvider<bool>
{
    /// <summary>
    /// Wrapped bool whose result is negated.
    /// </summary>
    public required IValueProvider<bool> Bool { get; init; }

    /// <inheritdoc/>
    [JsonIgnore]
    public bool Value => !Bool.Value;
}
