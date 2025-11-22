using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ItemChanger.Modules;

/// <summary>
/// An interface implemented by modules for sharing information between assemblies that do not strongly reference each other.
/// </summary>
public interface IInteropModule
{
    /// <summary>
    /// A description of the module that can be recognized by consumers.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Returns true if the property name corresponds to a non-null value of the specified type, and outputs the casted value.
    /// </summary>
    bool TryGetProperty<T>(string propertyName, [NotNullWhen(true)] out T? value);
}

/// <summary>
/// Module which provides the default implementation of IInteropModule.
/// </summary>
public class InteropModule : Module, IInteropModule
{
    /// <inheritdoc/>
    public required string Message { get; set; }

    /// <summary>
    /// A customizable property bag exposed to other modules.
    /// </summary>
    public Dictionary<string, object?> Properties { get; set; } = new();

    /// <inheritdoc/>
    public bool TryGetProperty<T>(string propertyName, [NotNullWhen(true)] out T? value)
    {
        if (
            propertyName == null
            || Properties == null
            || !Properties.TryGetValue(propertyName, out object? val)
            || val is not T t
        )
        {
            value = default;
            return false;
        }

        value = t;
        return true;
    }

    /// <inheritdoc/>
    protected override void DoLoad() { }

    /// <inheritdoc/>
    protected override void DoUnload() { }
}
