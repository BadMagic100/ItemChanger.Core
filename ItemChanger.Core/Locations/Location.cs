using System;
using System.Collections.Generic;
using ItemChanger.Enums;
using ItemChanger.Extensions;
using ItemChanger.Logging;
using ItemChanger.Placements;
using Newtonsoft.Json;

namespace ItemChanger.Locations;

/// <summary>
/// The base class for all locations. Locations are used by placements to place items.
/// <br/>Usually the location contains raw data and an implementation that may be customizable to an extent by the placement.
/// </summary>
public abstract class Location : TaggableObject, IFinderCloneable
{
    /// <summary>
    /// Whether the location is loaded.
    /// </summary>
    [JsonIgnore]
    public bool Loaded { get; private set; }

    private List<IDisposable> disposables = [];

    /// <summary>
    /// The name of the location. Location names are often, but not always, distinct.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The scene name of the location. Locations can however make changes which affect more than one scene, and rarely may choose not to use this field, in which case it can be safely set null.
    /// </summary>
    public string? SceneName { get; init; }

    /// <summary>
    /// Fetches the sceneName field and produces an error if it is null.
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    [JsonIgnore]
    public string UnsafeSceneName =>
        SceneName
        ?? throw new InvalidOperationException($"Scene name of location {Name} is not defined.");

    /// <summary>
    /// The flingType of the location, specifying how geo and similar objects are to be flung.
    /// </summary>
    public FlingType FlingType { get; init; }

    /// <summary>
    /// The placement holding the location. Location implementations can assume that this is non-null
    /// by the time they are loaded.
    /// </summary>
    [JsonIgnore]
    public Placement? Placement { get; set; }

    /// <summary>
    /// Loads the location. If the location is already loaded, does nothing. Typically called by the location's placement during loading.
    /// <br/>Execution order is (modules load -> placement tags load -> items load -> placements load)
    /// </summary>
    public void LoadOnce()
    {
        if (!Loaded)
        {
            try
            {
                LoadTags();
                DoLoad();
            }
            catch (Exception e)
            {
                LoggerProxy.LogError($"Error loading location {Name}:\n{e}");
            }
            Loaded = true;
        }
    }

    /// <summary>
    /// Unloads the location. If the location is not loaded, does nothing. Typically called by the location's placement during unloading.
    /// <br/>Execution order is (modules unload -> placement tags unload -> items unload -> placements unload)
    /// </summary>
    public void UnloadOnce()
    {
        if (Loaded)
        {
            try
            {
                UnloadTags();
                DoUnload();
            }
            catch (Exception e)
            {
                LoggerProxy.LogError($"Error unloading location {Name}:\n{e}");
            }
            finally
            {
                disposables.DisposeAll();
            }
            Loaded = false;
        }
    }

    /// <summary>
    /// Registers a disposable such as a hook subscription for cleanup when the location unloads.
    /// </summary>
    /// <param name="disposable">The disposable to register</param>
    public void Using(IDisposable disposable)
    {
        disposables.Add(disposable);
    }

    /// <summary>
    /// Allows the location to initialize and set up any hooks. Called once during loading.
    /// </summary>
    protected abstract void DoLoad();

    /// <summary>
    /// Allows the location to dispose any hooks. Called once during unloading.
    /// </summary>
    protected abstract void DoUnload();

    /// <summary>
    /// Creates a default placement for this location.
    /// </summary>
    public abstract Placement Wrap();
}
