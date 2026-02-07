namespace ItemChanger.Serialization;

/// <summary>
/// A string provider that represents a constant value
/// </summary>
public class BoxedString(string Value) : IWritableValueProvider<string>
{
    /// <inheritdoc/>
    public string Value { get; set; } = Value;
}
