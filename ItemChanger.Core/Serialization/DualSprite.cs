using Newtonsoft.Json;
using UnityEngine;

namespace ItemChanger.Serialization;

/// <summary>
/// A sprite provider that selects between two sprites based on a test.
/// </summary>
public class DualSprite(
    IValueProvider<bool> test,
    IValueProvider<Sprite> trueSprite,
    IValueProvider<Sprite> falseSprite
) : IValueProvider<Sprite>
{
    /// <summary>
    /// Boolean controlling which sprite is used.
    /// </summary>
    public IValueProvider<bool> Test => test;

    /// <summary>
    /// Sprite returned when <see cref="Test"/> is true.
    /// </summary>
    public IValueProvider<Sprite> TrueSprite => trueSprite;

    /// <summary>
    /// Sprite returned when <see cref="Test"/> is false.
    /// </summary>
    public IValueProvider<Sprite> FalseSprite => falseSprite;

    /// <inheritdoc/>
    [JsonIgnore]
    public Sprite Value => Test.Value ? TrueSprite.Value : FalseSprite.Value;
}
