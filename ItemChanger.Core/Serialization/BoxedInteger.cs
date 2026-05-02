namespace ItemChanger.Serialization;

/// <summary>
/// An integer provider which represents a constant value.
/// </summary>
public class BoxedInteger : IWritableValueProvider<int>
{
    /// <inheritdoc/>
    public required int Value { get; set; }
}
