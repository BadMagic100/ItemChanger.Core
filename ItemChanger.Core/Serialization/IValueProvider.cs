namespace ItemChanger.Serialization;

/// <summary>
/// Interface to provide a computed value in a serializable manner.
/// </summary>
/// <typeparam name="T">The value type</typeparam>
public interface IValueProvider<out T> : IFinderCloneable
{
    /// <summary>
    /// The defined value
    /// </summary>
    public T Value { get; }
}

/// <summary>
/// Interface to read and write a computed value in a serializable manner.
/// </summary>
/// <typeparam name="T">The value type</typeparam>
public interface IWritableValueProvider<T> : IValueProvider<T>
{
    /// <inheritdoc/>
    public new T Value { get; set; }
}
