using System;
using System.Collections.Generic;
using ItemChanger.Enums;
using ItemChanger.Extensions;
using ItemChanger.Logging;
using Newtonsoft.Json;

namespace ItemChanger.Tags;

/// <summary>
/// Base class for lightweight attachments that can both describe and modify placements, items, locations, and other taggable objects (tags frequently hook behavior in addition to carrying metadata).
/// </summary>
public abstract class Tag
{
    /// <summary>
    /// Whether the tag has been loaded
    /// </summary>
    [JsonIgnore]
    public bool Loaded { get; private set; }

    private List<IDisposable> disposables = [];

    /// <summary>
    /// Method to implement optional loading logic, called once during loading.
    /// </summary>
    /// <param name="parent">The object this tag is applied to</param>
    protected virtual void DoLoad(TaggableObject parent) { }

    /// <summary>
    /// Method to implement optional unloading logic, called once during unloading.
    /// </summary>
    /// <param name="parent">The object this tag is applied to</param>
    protected virtual void DoUnload(TaggableObject parent) { }

    /// <summary>
    /// Loads the tag. If the tag is already loaded, does nothing.
    /// </summary>
    public void LoadOnce(TaggableObject parent)
    {
        if (!Loaded)
        {
            try
            {
                DoLoad(parent);
            }
            catch (Exception e)
            {
                LoggerProxy.LogError($"Error loading {GetType().Name}:\n{e}");
            }
            Loaded = true;
        }
    }

    /// <summary>
    /// Unloads the tag. If the tag is not loaded, does nothing.
    /// </summary>
    public void UnloadOnce(TaggableObject parent)
    {
        if (Loaded)
        {
            try
            {
                DoUnload(parent);
            }
            catch (Exception e)
            {
                LoggerProxy.LogError($"Error unloading {GetType().Name}:\n{e}");
            }
            finally
            {
                disposables.DisposeAll();
            }
            Loaded = false;
        }
    }

    /// <summary>
    /// Registers a disposable such as a hook subscription for cleanup when the tag unloads.
    /// </summary>
    /// <param name="disposable">The disposable to register</param>
    public void Using(IDisposable disposable)
    {
        disposables.Add(disposable);
    }

    /// <summary>
    /// Additional information for serialization and other tag handling purposes.
    /// </summary>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public virtual TagHandlingFlags TagHandlingProperties { get; set; }
}
