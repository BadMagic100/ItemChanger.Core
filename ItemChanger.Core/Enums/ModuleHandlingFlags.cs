using System;

namespace ItemChanger.Enums;

/// <summary>
/// Enum which provides additional information for serialization and other module handling purposes.
/// </summary>
[Flags]
public enum ModuleHandlingFlags
{
    /// <summary>No flags.</summary>
    None = 0,

    /// <summary>
    /// If set, and an error occurs when deserializing this object as part of a ModuleCollection's modules list, an InvalidModule will be created with the data of this object, and deserialization will continue.
    /// </summary>
    AllowDeserializationFailure = 1 << 0,

    /// <summary>
    /// If set, indicates to consumers that this module should be removed if the current IC data is copied into a new profile.
    /// </summary>
    RemoveOnNewProfile = 1 << 1,
}
