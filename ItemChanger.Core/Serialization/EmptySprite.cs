using Newtonsoft.Json;
using UnityEngine;

namespace ItemChanger.Serialization;

/// <summary>
/// A sprite provider with no content
/// </summary>
public class EmptySprite : IValueProvider<Sprite>
{
    private Sprite? cachedSprite;

    /// <inheritdoc/>
    [JsonIgnore]
    public Sprite Value
    {
        get
        {
            if (cachedSprite == null)
            {
                Texture2D tex = new Texture2D(1, 1);
                byte[] data = [0, 0, 0, 0];
                tex.LoadRawTextureData(data);
                tex.Apply();
                cachedSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.zero);
            }
            return cachedSprite;
        }
    }
}
