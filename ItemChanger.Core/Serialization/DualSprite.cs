using Newtonsoft.Json;
using UnityEngine;

namespace ItemChanger.Serialization;

/// <summary>
/// A sprite provider that selects between two sprites based on a test.
/// </summary>
public class DualSprite : IValueProvider<Sprite>
{
    /// <summary>
    /// Boolean controlling which sprite is used.
    /// </summary>
    public required IValueProvider<bool> Test { get; init; }

    /// <summary>
    /// Sprite returned when <see cref="Test"/> is true.
    /// </summary>
    public required IValueProvider<Sprite> TrueSprite { get; init; }

    /// <summary>
    /// Sprite returned when <see cref="Test"/> is false.
    /// </summary>
    public required IValueProvider<Sprite> FalseSprite { get; init; }

    /// <inheritdoc/>
    [JsonIgnore]
    public Sprite Value => Test.Value ? TrueSprite.Value : FalseSprite.Value;
}
