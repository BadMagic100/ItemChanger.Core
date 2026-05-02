namespace ItemChanger.Serialization;

/// <summary>
/// A bool provider which represents a constant value.
/// </summary>
public class BoxedBool : IWritableValueProvider<bool>
{
    /// <inheritdoc/>
    public required bool Value { get; set; }
}
