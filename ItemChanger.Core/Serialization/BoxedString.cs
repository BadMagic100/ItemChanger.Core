namespace ItemChanger.Serialization;

/// <summary>
/// A string provider that represents a constant value
/// </summary>
public class BoxedString : IWritableValueProvider<string>
{
    /// <inheritdoc/>
    public required string Value { get; set; }
}
