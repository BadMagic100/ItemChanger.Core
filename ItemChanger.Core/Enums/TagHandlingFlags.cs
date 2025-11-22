using System;

namespace ItemChanger.Enums;

/// <summary>
/// Enum which provides additional options for serialization and other tag handling purposes.
/// </summary>
[Flags]
public enum TagHandlingFlags
{
    /// <summary>No options.</summary>
    None = 0,

    /// <summary>
    /// If set, and an error occurs when deserializing this object as part of a TaggableObject's tags list, an InvalidTag will be created with the data of this object, and deserialization will continue.
    /// </summary>
    AllowDeserializationFailure = 1,

    /// <summary>
    /// If set, indicates to consumers that this tag should be removed if the current IC data is copied into a new profile.
    /// </summary>
    RemoveOnNewProfile = 2,
}
