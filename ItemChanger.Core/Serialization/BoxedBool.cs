namespace ItemChanger.Serialization;

/// <summary>
/// A bool provider which represents a constant value.
/// </summary>
public class BoxedBool(bool value) : IWritableValueProvider<bool>
{
    /// <inheritdoc/>
    public bool Value { get; set; } = value;
}
