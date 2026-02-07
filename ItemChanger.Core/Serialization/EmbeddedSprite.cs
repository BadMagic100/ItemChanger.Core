using Newtonsoft.Json;
using UnityEngine;

namespace ItemChanger.Serialization;

/// <summary>
/// A Sprite provider which retrieves its sprite from a <see cref="ItemChanger.SpriteManager"/>.
/// </summary>
public abstract class EmbeddedSprite : IValueProvider<Sprite>
{
    /// <summary>
    /// The key of the sprite in the SpriteManager
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// The sprite manager which will provide the sprite implementation.
    /// </summary>
    [JsonIgnore]
    public abstract SpriteManager SpriteManager { get; }

    /// <inheritdoc/>
    [JsonIgnore]
    public Sprite Value => SpriteManager.GetSprite(Key);
}
