using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ItemChanger.Serialization.Converters;
using ItemChanger.Tags;
using ItemChanger.Tags.Constraints;
using Newtonsoft.Json;

namespace ItemChanger;

/// <summary>
/// Base class that provides tag storage and lifecycle management for derived objects.
/// </summary>
public class TaggableObject
{
    [JsonProperty(nameof(Tags))]
    [JsonConverter(typeof(TagListDeserializer))]
    private List<Tag> tags = [];

    /// <summary>
    /// Snapshot of tags currently attached to this object.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<Tag> Tags
    {
        get => tags;
        init => tags = [.. value];
    }

    private bool _tagsLoaded;

    /// <summary>
    /// Loads all tags, calling <see cref="Tag.LoadOnce(TaggableObject)"/> when the tag list is initialized.
    /// </summary>
    protected void LoadTags()
    {
        _tagsLoaded = true;
        if (tags == null)
        {
            return;
        }

        for (int i = 0; i < tags.Count; i++)
        {
            tags[i].LoadOnce(this);
        }
    }

    /// <summary>
    /// Unloads all tags, calling <see cref="Tag.UnloadOnce(TaggableObject)"/> when the tag list is initialized.
    /// </summary>
    protected void UnloadTags()
    {
        _tagsLoaded = false;
        if (tags == null)
        {
            return;
        }

        for (int i = 0; i < tags.Count; i++)
        {
            tags[i].UnloadOnce(this);
        }
    }

    /// <summary>
    /// Adds a new tag instance of type <typeparamref name="T"/>.
    /// </summary>
    public T AddTag<T>()
        where T : Tag, new()
    {
        tags ??= [];
        T t = new();
        CheckTagConstraints(t);
        if (_tagsLoaded)
        {
            t.LoadOnce(this);
        }

        tags.Add(t);
        return t;
    }

    /// <summary>
    /// Adds the provided tag instance to the object.
    /// </summary>
    public void AddTag(Tag t)
    {
        tags ??= [];
        CheckTagConstraints(t);
        if (_tagsLoaded)
        {
            t.LoadOnce(this);
        }

        tags.Add(t);
    }

    /// <summary>
    /// Adds a collection of tags.
    /// </summary>
    public void AddTags(IEnumerable<Tag> ts)
    {
        tags ??= [];
        foreach (Tag t in ts)
        {
            CheckTagConstraints(t);
            if (_tagsLoaded)
            {
                t.LoadOnce(this);
            }
            tags.Add(t);
        }
    }

    /// <summary>
    /// Retrieves the first tag of type <typeparamref name="T"/>, or null if none exist.
    /// </summary>
    public T? GetTag<T>()
    {
        return tags == null ? default : tags.OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// Attempts to retrieve a tag of type <typeparamref name="T"/>.
    /// </summary>
    public bool GetTag<T>(out T t)
        where T : class
    {
        t = GetTag<T>()!;
        return t != null;
    }

    /// <summary>
    /// Enumerates all tags of type <typeparamref name="T"/>.
    /// </summary>
    public IEnumerable<T> GetTags<T>()
    {
        return tags?.OfType<T>() ?? [];
    }

    /// <summary>
    /// Retrieves or creates a tag of type <typeparamref name="T"/>.
    /// </summary>
    public T GetOrAddTag<T>()
        where T : Tag, new()
    {
        tags ??= [];
        return tags.OfType<T>().FirstOrDefault() ?? AddTag<T>();
    }

    /// <summary>
    /// Checks whether a tag of type <typeparamref name="T"/> exists.
    /// </summary>
    public bool HasTag<T>()
        where T : Tag
    {
        return tags?.OfType<T>()?.Any() ?? false;
    }

    /// <summary>
    /// Removes all tags of type <typeparamref name="T"/>.
    /// </summary>
    public void RemoveTags<T>()
    {
        if (_tagsLoaded && tags != null)
        {
            foreach (Tag t in tags.Where(t => t is T))
            {
                t.UnloadOnce(this);
            }
        }
        tags = tags?.Where(t => t is not T)?.ToList() ?? [];
    }

    private void CheckTagConstraints(Tag t)
    {
        Type runtimeType = GetType();
        Type tagType = t.GetType();
        List<TagConstrainedToAttribute> attributes =
        [
            .. tagType.GetCustomAttributes<TagConstrainedToAttribute>(false),
        ];
        if (attributes.Count == 0)
        {
            return;
        }
        bool valid = false;
        foreach (TagConstrainedToAttribute attribute in attributes)
        {
            if (attribute.TaggableObjectType.IsAssignableFrom(runtimeType))
            {
                valid = true;
                break;
            }
        }
        if (!valid)
        {
            throw new ArgumentException(
                $"No constraint of tag {tagType.FullName} was satisfied by {runtimeType.FullName}"
            );
        }
    }
}
