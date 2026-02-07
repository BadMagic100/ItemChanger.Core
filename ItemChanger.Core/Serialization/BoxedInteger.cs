namespace ItemChanger.Serialization;

/// <summary>
/// An integer provider which represents a constant value.
/// </summary>
public class BoxedInteger(int Value) : IWritableValueProvider<int>
{
    /// <inheritdoc/>
    public int Value { get; set; } = Value;
}
